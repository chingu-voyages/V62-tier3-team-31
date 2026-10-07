namespace Ecommerce.Infrastructure.Configuration;

public sealed class StripeSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string ApiBaseUrl { get; set; } = "https://api.stripe.com";
    public string Currency { get; set; } = "usd";

    /// <summary>Where Stripe sends the customer back to. Filled from Cors:FrontendOrigin when empty.</summary>
    public string FrontendUrl { get; set; } = string.Empty;
}
