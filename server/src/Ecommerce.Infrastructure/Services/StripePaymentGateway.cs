using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using Ecommerce.Application.Common.Exceptions;
using Ecommerce.Application.Common.Interfaces;
using Ecommerce.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ecommerce.Infrastructure.Services;

/// <summary>
/// Creates Stripe Checkout Sessions through the REST API, so there is no SDK version to keep in step
/// and the base URL can point at a local stub in tests.
/// </summary>
public sealed class StripePaymentGateway : IPaymentGateway
{
    private readonly HttpClient _http;
    private readonly StripeSettings _settings;
    private readonly ILogger<StripePaymentGateway> _logger;

    public StripePaymentGateway(
        HttpClient http,
        IOptions<StripeSettings> settings,
        ILogger<StripePaymentGateway> logger)
    {
        _http = http;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<PaymentSession> CreateCheckoutSessionAsync(
        PaymentSessionRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.SecretKey))
            throw new InvalidOperationException("Stripe:SecretKey is not configured.");

        var orderId = request.OrderId.ToString();
        var form = new List<KeyValuePair<string, string>>
        {
            new("mode", "payment"),
            new("success_url", request.SuccessUrl),
            new("cancel_url", request.CancelUrl),
            new("client_reference_id", orderId),
            new("metadata[order_id]", orderId),
            new("payment_intent_data[metadata][order_id]", orderId)
        };

        if (!string.IsNullOrWhiteSpace(request.CustomerEmail))
            form.Add(new("customer_email", request.CustomerEmail));

        for (var i = 0; i < request.Items.Count; i++)
        {
            var item = request.Items[i];
            var prefix = $"line_items[{i}]";
            form.Add(new($"{prefix}[quantity]", item.Quantity.ToString(CultureInfo.InvariantCulture)));
            form.Add(new($"{prefix}[price_data][currency]", _settings.Currency));
            form.Add(new($"{prefix}[price_data][unit_amount]", item.UnitAmountInCents.ToString(CultureInfo.InvariantCulture)));
            form.Add(new($"{prefix}[price_data][product_data][name]", item.Title));
        }

        var url = _settings.ApiBaseUrl.TrimEnd('/') + "/v1/checkout/sessions";
        using var message = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new FormUrlEncodedContent(form)
        };
        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.SecretKey);
        message.Headers.Add("Idempotency-Key", "checkout-" + orderId);

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(message, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new PaymentGatewayException("Could not reach Stripe.", ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new PaymentGatewayException("Stripe did not answer in time.", ex);
        }

        using (response)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Stripe rejected the checkout session ({Status}): {Body}", (int)response.StatusCode, body);
                throw new PaymentGatewayException("Stripe rejected the request.");
            }

            try
            {
                using var document = JsonDocument.Parse(body);
                var id = document.RootElement.GetProperty("id").GetString();
                var sessionUrl = document.RootElement.GetProperty("url").GetString();

                if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(sessionUrl))
                    throw new PaymentGatewayException("Stripe returned an incomplete session.");

                return new PaymentSession(id, sessionUrl);
            }
            catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException)
            {
                throw new PaymentGatewayException("Stripe returned an unreadable response.", ex);
            }
        }
    }
}
