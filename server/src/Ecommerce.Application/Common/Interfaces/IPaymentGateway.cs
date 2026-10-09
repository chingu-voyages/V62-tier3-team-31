namespace Ecommerce.Application.Common.Interfaces;

public sealed record PaymentLineItem(string Title, long UnitAmountInCents, int Quantity);

public sealed record PaymentSessionRequest(
    Guid OrderId,
    string? CustomerEmail,
    IReadOnlyList<PaymentLineItem> Items,
    string SuccessUrl,
    string CancelUrl);

public sealed record PaymentSession(string Id, string Url);

public interface IPaymentGateway
{
    /// <summary>
    /// Creates a hosted payment page. Throws PaymentGatewayException when the provider fails.
    /// </summary>
    Task<PaymentSession> CreateCheckoutSessionAsync(
        PaymentSessionRequest request,
        CancellationToken cancellationToken);
}
