namespace Ecommerce.Application.DTOs;

public sealed record CartDto(
    IReadOnlyList<CartItemDto> Items,
    int ItemCount,
    decimal Subtotal);

public sealed record CartItemDto(
    Guid ProductId,
    string Title,
    string? ImageUrl,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal,
    int StockQuantity,
    bool Available);

public sealed class AddCartItemRequestDto
{
    public Guid? ProductId { get; set; }
    public int? Quantity { get; set; }
}

public sealed class UpdateCartItemRequestDto
{
    public int? Quantity { get; set; }
}