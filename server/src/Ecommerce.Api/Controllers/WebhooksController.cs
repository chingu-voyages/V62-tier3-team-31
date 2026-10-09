using Ecommerce.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/webhooks")]
public sealed class WebhooksController : ControllerBase
{
    private readonly IStripeWebhookService _webhooks;

    public WebhooksController(IStripeWebhookService webhooks)
    {
        _webhooks = webhooks;
    }

    // Called by Stripe, never by the frontend. The body is read as raw bytes because the signature
    // is computed over the exact bytes Stripe sent; model binding would change them.
    [HttpPost("stripe")]
    public async Task<IActionResult> Stripe(CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream();
        await Request.Body.CopyToAsync(buffer, cancellationToken);
        var signature = Request.Headers["Stripe-Signature"].ToString();

        var result = await _webhooks.HandleAsync(buffer.ToArray(), signature, cancellationToken);

        return result is StripeWebhookResult.InvalidSignature or StripeWebhookResult.InvalidPayload
            ? BadRequest(new { title = "Invalid webhook request", status = StatusCodes.Status400BadRequest })
            : Ok();
    }
}
