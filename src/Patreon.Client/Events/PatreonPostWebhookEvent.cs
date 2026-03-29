using Patreon.Client.Models;

namespace Patreon.Client.Events;

/// <summary>
/// Raised when Patreon sends a post lifecycle event.
/// Event types: <c>posts:publish</c>, <c>posts:update</c>, <c>posts:delete</c>.
/// </summary>
public sealed record PatreonPostWebhookEvent : PatreonWebhookEvent
{
    /// <summary>Gets the post attributes from the webhook payload.</summary>
    public required PostAttributes? Attributes { get; init; }
}
