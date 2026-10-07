using System.Text.Json;
using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Core.Entities;
using Ecommerce.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ecommerce.Infrastructure.Services;

public sealed class StripeWebhookService : IStripeWebhookService
{
    private static readonly TimeSpan SignatureTolerance = TimeSpan.FromMinutes(5);

    private readonly ApplicationDbContext _context;
    private readonly StripeSettings _settings;
    private readonly ILogger<StripeWebhookService> _logger;

    public StripeWebhookService(
        ApplicationDbContext context,
        IOptions<StripeSettings> settings,
        ILogger<StripeWebhookService> logger)
    {
        _context = context;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<StripeWebhookResult> HandleAsync(
        byte[] payload,
        string? signatureHeader,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.WebhookSecret))
            throw new InvalidOperationException("Stripe:WebhookSecret is not configured.");

        // Nothing is parsed or trusted until the signature over the raw body checks out.
        if (!StripeSignature.IsValid(payload, signatureHeader, _settings.WebhookSecret, DateTimeOffset.UtcNow, SignatureTolerance))
            return StripeWebhookResult.InvalidSignature;

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(payload);
        }
        catch (JsonException)
        {
            return StripeWebhookResult.InvalidPayload;
        }

        using (document)
        {
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
                return StripeWebhookResult.InvalidPayload;

            var eventId = ReadString(root, "id");
            var eventType = ReadString(root, "type");

            if (eventId is null
                || eventType is null
                || !root.TryGetProperty("data", out var data)
                || data.ValueKind != JsonValueKind.Object
                || !data.TryGetProperty("object", out var eventObject)
                || eventObject.ValueKind != JsonValueKind.Object)
            {
                return StripeWebhookResult.InvalidPayload;
            }

            // The event marker and the order changes commit together or not at all. If anything below
            // throws, the marker rolls back too, so Stripe's retry can do the work properly.
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var inserted = await _context.Database.ExecuteSqlInterpolatedAsync(
                $@"INSERT INTO ""StripeEvents"" (""Id"", ""EventType"", ""ProcessedAt"")
                   VALUES ({eventId}, {eventType}, {DateTime.UtcNow})
                   ON CONFLICT (""Id"") DO NOTHING",
                cancellationToken);

            if (inserted == 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                return StripeWebhookResult.Duplicate;
            }

            await ApplyAsync(eventType, eventObject, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return StripeWebhookResult.Processed;
        }
    }

    private async Task ApplyAsync(string type, JsonElement obj, CancellationToken cancellationToken)
    {
        switch (type)
        {
            case "checkout.session.completed":
                await OnSessionCompletedAsync(obj, cancellationToken);
                break;
            case "checkout.session.expired":
                await OnSessionExpiredAsync(obj, cancellationToken);
                break;
            case "payment_intent.payment_failed":
                await OnPaymentFailedAsync(obj, cancellationToken);
                break;
            case "charge.refunded":
                await OnChargeRefundedAsync(obj, cancellationToken);
                break;
            case "charge.dispute.created":
                await OnDisputeCreatedAsync(obj, cancellationToken);
                break;
            default:
                _logger.LogInformation("Ignoring Stripe event type {Type}", type);
                break;
        }
    }

    private async Task OnSessionCompletedAsync(JsonElement session, CancellationToken cancellationToken)
    {
        var order = await FindOrderByIdAsync(ReadString(session, "client_reference_id"), includeItems: true, cancellationToken);
        if (order is null)
        {
            _logger.LogWarning("Stripe completed a checkout session for an unknown order");
            return;
        }

        // A failed order can still be paid if the customer retries inside the same Stripe session.
        if (order.Status is not (OrderStatus.Pending or OrderStatus.Failed))
            return;

        if (ReadString(session, "payment_status") != "paid")
        {
            _logger.LogInformation("Order {OrderId} completed without payment yet, waiting", order.Id);
            return;
        }

        var now = DateTime.UtcNow;
        order.Status = OrderStatus.Paid;
        order.PaidAt = now;
        order.UpdatedAt = now;
        order.StripePaymentIntentId = ReadString(session, "payment_intent");
        order.FailureReason = null;

        var oversold = false;

        // Products are locked one row at a time, always in the same order, so two payments for the
        // last item cannot both read the same stock number.
        foreach (var item in order.Items.OrderBy(i => i.ProductId))
        {
            var locked = await _context.Products
                .FromSqlInterpolated($@"SELECT * FROM ""Products"" WHERE ""Id"" = {item.ProductId} FOR UPDATE")
                .ToListAsync(cancellationToken);

            var product = locked.FirstOrDefault();
            if (product is null)
                continue;

            if (product.StockQuantity < item.Quantity)
                oversold = true;

            product.StockQuantity = Math.Max(0, product.StockQuantity - item.Quantity);
            product.UpdatedAt = now;
        }

        if (oversold)
        {
            // The money was taken, so the order stays paid. The note tells a human to refund it.
            order.FailureReason = "Out of stock after payment. Refund this order manually.";
            _logger.LogWarning("Order {OrderId} was paid but is out of stock and needs a manual refund", order.Id);
        }

        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == order.UserId, cancellationToken);

        if (cart is not null && cart.Items.Count > 0)
        {
            _context.CartItems.RemoveRange(cart.Items);
            cart.UpdatedAt = now;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task OnSessionExpiredAsync(JsonElement session, CancellationToken cancellationToken)
    {
        var order = await FindOrderByIdAsync(ReadString(session, "client_reference_id"), includeItems: false, cancellationToken);
        if (order is null || order.Status != OrderStatus.Pending)
            return;

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task OnPaymentFailedAsync(JsonElement intent, CancellationToken cancellationToken)
    {
        string? orderIdText = null;
        if (intent.TryGetProperty("metadata", out var metadata) && metadata.ValueKind == JsonValueKind.Object)
            orderIdText = ReadString(metadata, "order_id");

        var order = await FindOrderByIdAsync(orderIdText, includeItems: false, cancellationToken);
        if (order is null || order.Status != OrderStatus.Pending)
            return;

        string? reason = null;
        if (intent.TryGetProperty("last_payment_error", out var error) && error.ValueKind == JsonValueKind.Object)
            reason = ReadString(error, "message");

        order.Status = OrderStatus.Failed;
        order.FailureReason = reason is null ? "Payment failed" : Truncate(reason, 500);
        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task OnChargeRefundedAsync(JsonElement charge, CancellationToken cancellationToken)
    {
        var order = await FindOrderByPaymentIntentAsync(ReadString(charge, "payment_intent"), cancellationToken);
        if (order is null)
        {
            _logger.LogWarning("Stripe refunded a charge for an unknown order");
            return;
        }

        if (!charge.TryGetProperty("amount_refunded", out var refundedElement)
            || !refundedElement.TryGetInt64(out var refundedCents)
            || refundedCents <= 0)
        {
            return;
        }

        // Stripe sends the running total, so setting it (not adding to it) is safe to repeat.
        var refunded = Math.Min(refundedCents / 100m, order.TotalAmount);
        order.RefundedAmount = refunded;
        order.Status = refunded >= order.TotalAmount ? OrderStatus.Refunded : OrderStatus.PartiallyRefunded;
        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task OnDisputeCreatedAsync(JsonElement dispute, CancellationToken cancellationToken)
    {
        var order = await FindOrderByPaymentIntentAsync(ReadString(dispute, "payment_intent"), cancellationToken);
        if (order is null)
        {
            _logger.LogWarning("Stripe opened a dispute for an unknown order");
            return;
        }

        order.Status = OrderStatus.Disputed;
        order.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<Order?> FindOrderByIdAsync(string? idText, bool includeItems, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(idText, out var orderId))
            return null;

        var query = _context.Orders.AsQueryable();
        if (includeItems)
            query = query.Include(o => o.Items);

        return await query.FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
    }

    private async Task<Order?> FindOrderByPaymentIntentAsync(string? paymentIntentId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(paymentIntentId))
            return null;

        return await _context.Orders
            .FirstOrDefaultAsync(o => o.StripePaymentIntentId == paymentIntentId, cancellationToken);
    }

    private static string? ReadString(JsonElement element, string name) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(name, out var value)
        && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max];
}
