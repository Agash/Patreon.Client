namespace Patreon.Client.Events;

/// <summary>
/// Base record for all Patreon webhook events.
/// </summary>
public abstract record PatreonWebhookEvent
{
    /// <summary>
    /// Gets the event type from the <c>X-Patreon-Event</c> header
    /// (e.g. <c>members:create</c>, <c>posts:publish</c>).
    /// </summary>
    public required string EventType { get; init; }

    /// <summary>Gets the primary resource identifier from the webhook payload.</summary>
    public required string ResourceId { get; init; }

    /// <summary>Gets the primary resource type from the webhook payload (e.g. <c>member</c>, <c>post</c>).</summary>
    public required string ResourceType { get; init; }
}
