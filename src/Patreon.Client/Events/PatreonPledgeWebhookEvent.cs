using Patreon.Client.JsonApi;
using Patreon.Client.Models;

namespace Patreon.Client.Events;

/// <summary>
/// Raised when Patreon sends a pledge lifecycle event.
/// Event types: <c>members:pledge:create</c>, <c>members:pledge:update</c>, <c>members:pledge:delete</c>.
/// </summary>
/// <remarks>
/// Pledge events use the <c>member</c> resource type. The full JSON:API document is available
/// via <see cref="Document"/>, giving access to <c>data.relationships</c> (e.g. currently
/// entitled tier IDs) and side-loaded included resources.
/// Use <see cref="Patreon.Client.JsonApi.JsonApiIncludedIndex"/> to resolve included resources.
/// </remarks>
public sealed record PatreonPledgeWebhookEvent : PatreonWebhookEvent
{
    /// <summary>
    /// Gets the full JSON:API document payload from the webhook, including side-loaded
    /// relationships and included resources.
    /// </summary>
    public required JsonApiDocument<MemberAttributes>? Document { get; init; }

    /// <summary>Convenience accessor for the primary resource attributes.</summary>
    public MemberAttributes? Attributes => Document?.Data?.Attributes;
}
