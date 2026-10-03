namespace Ecommerce.Application.DTOs;

/// <summary>A compact representation of an order in a user's order history.</summary>
public sealed record OrderSummaryDto(
    Guid Id,
    string Status,
    string FulfillmentStatus,
    decimal TotalAmount,
    int ItemCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset? PaidAt);

/// <summary>The shipping address associated with an order.</summary>
public sealed record OrderShippingDto(
    string Name,
    string? Phone,
    string Line1,
    string? Line2,
    string City,
    string? State,
    string PostalCode,
    string Country);

/// <summary>A frozen item price and title with its current product image.</summary>
public sealed record OrderItemDto(
    Guid ProductId,
    string Title,
    string? ImageUrl,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal);

/// <summary>The full order details returned to its owner.</summary>
public sealed record OrderDto(
    Guid Id,
    string Status,
    string FulfillmentStatus,
    decimal TotalAmount,
    decimal RefundedAmount,
    OrderShippingDto Shipping,
    IReadOnlyList<OrderItemDto> Items,
    DateTimeOffset CreatedAt,
    DateTimeOffset? PaidAt);
