namespace Ecommerce.Application.DTOs;

public sealed class CheckoutRequestDto
{
    public string? ShippingName { get; set; }
    public string? ShippingPhone { get; set; }
    public string? ShippingLine1 { get; set; }
    public string? ShippingLine2 { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingState { get; set; }
    public string? ShippingPostalCode { get; set; }
    public string? ShippingCountry { get; set; }
}

public sealed record CheckoutResponseDto(Guid OrderId, string CheckoutUrl);
