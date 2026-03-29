using System.Security.Cryptography;
using System.Text;

namespace Patreon.Client.Webhooks;

/// <summary>
/// Verifies Patreon webhook request signatures.
/// </summary>
/// <remarks>
/// Patreon signs webhook deliveries using HMAC-MD5. The key is the webhook secret
/// (UTF-8 encoded) and the message is the raw request body bytes. The resulting
/// digest is expressed as a lowercase hexadecimal string and placed in the
/// <c>X-Patreon-Signature</c> header.
/// </remarks>
public sealed class PatreonWebhookSignatureVerifier
{
    /// <summary>The header name that carries the HMAC-MD5 signature.</summary>
    public const string SignatureHeaderName = "X-Patreon-Signature";

    /// <summary>The header name that carries the Patreon event type.</summary>
    public const string EventHeaderName = "X-Patreon-Event";

    /// <summary>
    /// Verifies the Patreon webhook signature contained in <paramref name="headers"/>.
    /// </summary>
    /// <param name="body">The raw, unparsed request body bytes.</param>
    /// <param name="headers">The request headers.</param>
    /// <param name="webhookSecret">The webhook secret configured in the Patreon creator portal.</param>
    /// <returns>
    /// <see langword="true"/> when the received signature matches the computed digest;
    /// otherwise <see langword="false"/>.
    /// </returns>
    public bool Verify(
        byte[] body,
        IReadOnlyDictionary<string, string[]> headers,
        string webhookSecret)
    {
        ArgumentNullException.ThrowIfNull(body);
        ArgumentNullException.ThrowIfNull(headers);
        ArgumentException.ThrowIfNullOrEmpty(webhookSecret);

        if (!headers.TryGetValue(SignatureHeaderName, out string[]? sigValues)
            || sigValues.Length == 0
            || string.IsNullOrWhiteSpace(sigValues[0]))
        {
            return false;
        }

        string receivedSig = sigValues[0];

        byte[] keyBytes = Encoding.UTF8.GetBytes(webhookSecret);

        // Patreon requires HMAC-MD5. This is an unusual choice by Patreon's API design
        // and is explicitly documented in their official webhook docs.
#pragma warning disable CA5351 // Do Not Use Broken Cryptographic Algorithms
        using HMACMD5 hmac = new(keyBytes);
#pragma warning restore CA5351
        byte[] hash = hmac.ComputeHash(body);
        string computed = Convert.ToHexString(hash).ToLowerInvariant();

        return CryptographicOperations.FixedTimeEquals(
            Encoding.ASCII.GetBytes(computed),
            Encoding.ASCII.GetBytes(receivedSig));
    }
}
