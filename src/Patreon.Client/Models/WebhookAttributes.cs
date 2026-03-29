using System.Text.Json.Serialization;

namespace Patreon.Client.Models;

/// <summary>
/// Attributes of a Patreon webhook resource.
/// </summary>
/// <remarks>
/// Webhooks are created and managed via the Patreon API v2 with the
/// <c>w:campaigns.webhook</c> OAuth scope. The webhook's <c>secret</c> is only
/// returned on creation — it cannot be retrieved afterwards.
/// </remarks>
public sealed class WebhookAttributes
{
    /// <summary>Gets the webhook delivery URL.</summary>
    [JsonPropertyName("uri")]
    public string? Uri { get; init; }

    /// <summary>
    /// Gets the list of event types this webhook is subscribed to.
    /// Possible values: <c>members:create</c>, <c>members:update</c>, <c>members:delete</c>,
    /// <c>members:pledge:create</c>, <c>members:pledge:update</c>, <c>members:pledge:delete</c>,
    /// <c>posts:publish</c>, <c>posts:update</c>, <c>posts:delete</c>.
    /// </summary>
    [JsonPropertyName("triggers")]
    public IReadOnlyList<string>? Triggers { get; init; }

    /// <summary>Gets whether this webhook is currently paused.</summary>
    [JsonPropertyName("paused")]
    public bool Paused { get; init; }

    /// <summary>Gets the number of consecutive failed delivery attempts.</summary>
    [JsonPropertyName("num_consecutive_times_failed")]
    public int NumConsecutiveTimesFailed { get; init; }

    /// <summary>Gets the ISO 8601 timestamp of the last delivery attempt.</summary>
    [JsonPropertyName("last_attempted_at")]
    public string? LastAttemptedAt { get; init; }

    /// <summary>
    /// Gets the HMAC-MD5 signing secret used to verify webhook deliveries.
    /// Only returned on initial webhook creation — not available on subsequent reads.
    /// </summary>
    [JsonPropertyName("secret")]
    public string? Secret { get; init; }
}
