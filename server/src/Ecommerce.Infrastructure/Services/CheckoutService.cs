using System.Text.RegularExpressions;
using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Application.DTOs;
using Ecommerce.Core.Entities;
using Ecommerce.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ecommerce.Infrastructure.Services;

public sealed class CheckoutService : ICheckoutService
{
    private readonly ApplicationDbContext _context;
    private readonly IPaymentGateway _gateway;
    private readonly StripeSettings _settings;
    private readonly ILogger<CheckoutService> _logger;

    public CheckoutService(
        ApplicationDbContext context,
        IPaymentGateway gateway,
        IOptions<StripeSettings> settings,
        ILogger<CheckoutService> logger)
    {
        _context = context;
        _gateway = gateway;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<CheckoutResponseDto> CreateSessionAsync(
        Guid userId,
        CheckoutRequestDto? request,
        CancellationToken cancellationToken)
    {
        var shipping = ValidateShipping(request);

        var cart = await _context.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (cart is null || cart.Items.Count == 0)
            throw new CartConflictException("Your cart is empty");

        var lines = cart.Items
            .OrderBy(i => i.CreatedAt)
            .ThenBy(i => i.Id)
            .ToList();

        // Prices and stock are always read here, never taken from the client.
        foreach (var line in lines)
        {
            var product = line.Product;

            if (!product.IsActive)
                throw new CartConflictException($"{product.Title} is no longer available");
            if (product.StockQuantity <= 0)
                throw new CartConflictException($"{product.Title} is out of stock");
            if (line.Quantity > product.StockQuantity)
                throw new CartConflictException($"Only {product.StockQuantity} of {product.Title} left in stock");
        }

        var frontendUrl = _settings.FrontendUrl.TrimEnd('/');
        if (frontendUrl.Length == 0)
            throw new InvalidOperationException("Cors:FrontendOrigin is not configured.");

        var email = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.Email)
            .FirstOrDefaultAsync(cancellationToken);

        var order = new Order
        {
            UserId = userId,
            Status = OrderStatus.Pending,
            FulfillmentStatus = FulfillmentStatus.Unfulfilled,
            TotalAmount = lines.Sum(l => l.Product.Price * l.Quantity),
            ShippingName = shipping.Name,
            ShippingPhone = shipping.Phone,
            ShippingAddressLine1 = shipping.Line1,
            ShippingAddressLine2 = shipping.Line2,
            ShippingCity = shipping.City,
            ShippingState = shipping.State,
            ShippingPostalCode = shipping.PostalCode,
            ShippingCountry = shipping.Country
        };

        // Prices and titles are frozen on the order here, so later catalogue changes never alter it.
        foreach (var line in lines)
        {
            order.Items.Add(new OrderItem
            {
                ProductId = line.ProductId,
                ProductTitle = line.Product.Title,
                Quantity = line.Quantity,
                UnitPrice = line.Product.Price
            });
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        var items = lines
            .Select(l => new PaymentLineItem(l.Product.Title, ToCents(l.Product.Price), l.Quantity))
            .ToList();

        PaymentSession session;
        try
        {
            session = await _gateway.CreateCheckoutSessionAsync(
                new PaymentSessionRequest(
                    order.Id,
                    email,
                    items,
                    $"{frontendUrl}/checkout/success?orderId={order.Id}",
                    $"{frontendUrl}/checkout/cancel?orderId={order.Id}"),
                cancellationToken);
        }
        catch (PaymentGatewayException ex)
        {
            _logger.LogError(ex, "Could not create a Stripe session for order {OrderId}", order.Id);

            order.Status = OrderStatus.Failed;
            order.FailureReason = "Could not start payment";
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(CancellationToken.None);

            throw new CartConflictException("We couldn't start the payment. Please try again.");
        }

        order.StripeSessionId = session.Id;
        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        // The cart is left alone on purpose. It is emptied by the webhook once payment succeeds.
        return new CheckoutResponseDto(order.Id, session.Url);
    }

    private static long ToCents(decimal price) =>
        (long)Math.Round(price * 100m, MidpointRounding.AwayFromZero);

    private sealed record ShippingInput(
        string Name,
        string? Phone,
        string Line1,
        string? Line2,
        string City,
        string? State,
        string PostalCode,
        string Country);

    private static ShippingInput ValidateShipping(CheckoutRequestDto? request)
    {
        var errors = new Dictionary<string, string[]>();

        static string? Clean(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        var name = Clean(request?.ShippingName);
        var phone = Clean(request?.ShippingPhone);
        var line1 = Clean(request?.ShippingLine1);
        var line2 = Clean(request?.ShippingLine2);
        var city = Clean(request?.ShippingCity);
        var state = Clean(request?.ShippingState);
        var postalCode = Clean(request?.ShippingPostalCode);
        var country = Clean(request?.ShippingCountry);

        void Required(string key, string label, string? value, int max)
        {
            if (value is null)
                errors[key] = [$"{label} is required."];
            else if (value.Length > max)
                errors[key] = [$"{label} must be {max} characters or fewer."];
        }

        void Optional(string key, string label, string? value, int max)
        {
            if (value is not null && value.Length > max)
                errors[key] = [$"{label} must be {max} characters or fewer."];
        }

        Required("shippingName", "Name", name, 200);
        Optional("shippingPhone", "Phone", phone, 30);
        Required("shippingLine1", "Address line 1", line1, 255);
        Optional("shippingLine2", "Address line 2", line2, 255);
        Required("shippingCity", "City", city, 100);
        Optional("shippingState", "State", state, 100);
        Required("shippingPostalCode", "Postal code", postalCode, 20);

        if (country is null)
            errors["shippingCountry"] = ["Country is required."];
        else if (!Regex.IsMatch(country, "^[A-Za-z]{2}$"))
            errors["shippingCountry"] = ["Country must be a 2-letter code like US or GB."];

        if (errors.Count > 0)
            throw new InvalidRequestException(errors);

        return new ShippingInput(
            name!, phone, line1!, line2, city!, state, postalCode!, country!.ToUpperInvariant());
    }
}
