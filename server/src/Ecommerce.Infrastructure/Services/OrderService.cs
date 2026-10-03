using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Application.DTOs;
using Ecommerce.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Services;

public sealed class OrderService(ApplicationDbContext context) : IOrderService
{
    public async Task<PagedResponseDto<OrderSummaryDto>> GetOrdersAsync(
        Guid userId,
        int page,
        int limit,
        CancellationToken cancellationToken)
    {
        var paidOrders = context.Orders
            .AsNoTracking()
            .Where(order => order.UserId == userId && order.PaidAt != null);

        var total = await paidOrders.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(total / (double)limit);

        if (totalPages == 0 || page > totalPages)
            return new PagedResponseDto<OrderSummaryDto>(Array.Empty<OrderSummaryDto>(), page, limit, total, totalPages);

        var rows = await paidOrders
            .OrderByDescending(order => order.PaidAt)
            .ThenBy(order => order.Id)
            .Skip((int)((long)(page - 1) * limit))
            .Take(limit)
            .Select(order => new
            {
                order.Id,
                order.Status,
                order.FulfillmentStatus,
                order.TotalAmount,
                order.CreatedAt,
                order.PaidAt,
                ItemCount = order.Items.Sum(item => (int?)item.Quantity) ?? 0
            })
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(order => new OrderSummaryDto(
                order.Id,
                FormatStatus(order.Status),
                FormatStatus(order.FulfillmentStatus),
                RoundMoney(order.TotalAmount),
                order.ItemCount,
                ToUtcOffset(order.CreatedAt),
                order.PaidAt is null ? null : ToUtcOffset(order.PaidAt.Value)))
            .ToArray();

        return new PagedResponseDto<OrderSummaryDto>(items, page, limit, total, totalPages);
    }

    public async Task<OrderDto?> GetOrderAsync(Guid userId, Guid orderId, CancellationToken cancellationToken)
    {
        var order = await context.Orders
            .AsNoTracking()
            .Include(candidate => candidate.User)
            .Include(candidate => candidate.Items)
                .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(
                candidate => candidate.Id == orderId && candidate.UserId == userId,
                cancellationToken);

        if (order is null)
            return null;

        var items = order.Items
            .OrderBy(item => item.Id)
            .Select(item => new OrderItemDto(
                item.ProductId,
                item.ProductTitle,
                item.Product.ImageUrl,
                RoundMoney(item.UnitPrice),
                item.Quantity,
                RoundMoney(item.UnitPrice * item.Quantity)))
            .ToArray();

        var name = string.Join(
            ' ',
            new[] { order.User.FirstName, order.User.LastName }
                .Where(part => !string.IsNullOrWhiteSpace(part))
                .Select(part => part!.Trim()));

        return new OrderDto(
            order.Id,
            FormatStatus(order.Status),
            FormatStatus(order.FulfillmentStatus),
            RoundMoney(order.TotalAmount),
            RoundMoney(order.RefundedAmount),
            new OrderShippingDto(
                name,
                null,
                order.ShippingAddressLine1,
                order.ShippingAddressLine2,
                order.ShippingCity,
                string.IsNullOrWhiteSpace(order.ShippingState) ? null : order.ShippingState,
                order.ShippingPostalCode,
                order.ShippingCountry),
            items,
            ToUtcOffset(order.CreatedAt),
            order.PaidAt is null ? null : ToUtcOffset(order.PaidAt.Value));
    }

    private static decimal RoundMoney(decimal amount) =>
        decimal.Round(amount, 2, MidpointRounding.AwayFromZero);

    private static DateTimeOffset ToUtcOffset(DateTime value) =>
        value.Kind switch
        {
            DateTimeKind.Utc => new DateTimeOffset(value),
            DateTimeKind.Local => new DateTimeOffset(value.ToUniversalTime()),
            _ => new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Utc))
        };

    private static string FormatStatus(OrderStatus status) => status switch
    {
        OrderStatus.Pending => "pending",
        OrderStatus.Paid => "paid",
        OrderStatus.Failed => "failed",
        OrderStatus.Cancelled => "cancelled",
        OrderStatus.Refunded => "refunded",
        OrderStatus.PartiallyRefunded => "partially_refunded",
        OrderStatus.Disputed => "disputed",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown order status.")
    };

    private static string FormatStatus(FulfillmentStatus status) => status switch
    {
        FulfillmentStatus.Unfulfilled => "unfulfilled",
        FulfillmentStatus.Processing => "processing",
        FulfillmentStatus.Shipped => "shipped",
        FulfillmentStatus.Delivered => "delivered",
        FulfillmentStatus.Cancelled => "cancelled",
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown fulfillment status.")
    };
}
