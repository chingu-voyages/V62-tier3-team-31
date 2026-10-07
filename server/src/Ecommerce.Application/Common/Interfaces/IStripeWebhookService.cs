namespace Ecommerce.Application.Common.Interfaces;

public enum StripeWebhookResult
{
    Processed,
    Duplicate,
    InvalidSignature,
    InvalidPayload
}

public interface IStripeWebhookService
{
    /// <summary>
    /// Verifies the signature of the raw request body, then applies the event exactly once.
    /// </summary>
    Task<StripeWebhookResult> HandleAsync(
        byte[] payload,
        string? signatureHeader,
        CancellationToken cancellationToken);
}
