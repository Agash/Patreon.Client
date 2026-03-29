namespace Patreon.Client.Webhooks;

/// <summary>
/// Options for validating Patreon webhook deliveries.
/// </summary>
public sealed class PatreonWebhookOptions
{
    /// <summary>
    /// Gets or sets the webhook secret configured in the Patreon creator portal.
    /// This is used to verify HMAC-MD5 signatures on incoming webhook requests.
    /// </summary>
    public string WebhookSecret { get; set; } = string.Empty;
}
