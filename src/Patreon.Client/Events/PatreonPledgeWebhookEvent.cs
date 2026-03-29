using Patreon.Client.Models;

namespace Patreon.Client.Events;

/// <summary>
/// Raised when Patreon sends a pledge lifecycle event.
/// Event types: <c>members:pledge:create</c>, <c>members:pledge:update</c>, <c>members:pledge:delete</c>.
/// </summary>
public sealed record PatreonPledgeWebhookEvent : PatreonWebhookEvent
{
    /// <summary>Gets the member attributes from the webhook payload (pledge events use the member resource).</summary>
    public required MemberAttributes? Attributes { get; init; }
}
