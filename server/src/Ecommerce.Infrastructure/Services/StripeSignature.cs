using System.Security.Cryptography;
using System.Text;

namespace Ecommerce.Infrastructure.Services;

/// <summary>
/// Verifies the Stripe-Signature header: "t=timestamp,v1=hex_hmac". The HMAC-SHA256 is computed over
/// "timestamp.rawBody" with the webhook signing secret, so the body must be the exact bytes Stripe sent.
/// </summary>
public static class StripeSignature
{
    public static bool IsValid(
        byte[] payload,
        string? header,
        string secret,
        DateTimeOffset now,
        TimeSpan tolerance)
    {
        if (string.IsNullOrWhiteSpace(header) || string.IsNullOrEmpty(secret))
            return false;

        long? timestamp = null;
        var signatures = new List<string>();

        foreach (var part in header.Split(','))
        {
            var pair = part.Split('=', 2);
            if (pair.Length != 2)
                continue;

            var key = pair[0].Trim();
            var value = pair[1].Trim();

            if (key == "t" && long.TryParse(value, out var parsed))
                timestamp = parsed;
            else if (key == "v1")
                signatures.Add(value);
        }

        if (timestamp is null || signatures.Count == 0)
            return false;

        DateTimeOffset sentAt;
        try
        {
            sentAt = DateTimeOffset.FromUnixTimeSeconds(timestamp.Value);
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }

        if ((now - sentAt).Duration() > tolerance)
            return false;

        var prefix = Encoding.UTF8.GetBytes(timestamp.Value + ".");
        var signed = new byte[prefix.Length + payload.Length];
        Buffer.BlockCopy(prefix, 0, signed, 0, prefix.Length);
        Buffer.BlockCopy(payload, 0, signed, prefix.Length, payload.Length);

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var expected = hmac.ComputeHash(signed);

        foreach (var candidate in signatures)
        {
            byte[] actual;
            try
            {
                actual = Convert.FromHexString(candidate);
            }
            catch (FormatException)
            {
                continue;
            }

            if (CryptographicOperations.FixedTimeEquals(expected, actual))
                return true;
        }

        return false;
    }
}
