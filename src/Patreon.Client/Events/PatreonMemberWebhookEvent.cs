using Patreon.Client.Models;

namespace Patreon.Client.Events;

/// <summary>
/// Raised when Patreon sends a member lifecycle event.
/// Event types: <c>members:create</c>, <c>members:update</c>, <c>members:delete</c>.
/// </summary>
public sealed record PatreonMemberWebhookEvent : PatreonWebhookEvent
{
    /// <summary>Gets the member attributes from the webhook payload.</summary>
    public required MemberAttributes? Attributes { get; init; }
}
