using Patreon.Client.JsonApi;
using Patreon.Client.Models;

namespace Patreon.Client.Events;

/// <summary>
/// Raised when Patreon sends a member lifecycle event.
/// Event types: <c>members:create</c>, <c>members:update</c>, <c>members:delete</c>.
/// </summary>
/// <remarks>
/// The full JSON:API document is available via <see cref="Document"/>, giving access to
/// <c>data.relationships</c> (e.g. entitled tier IDs) and the <c>included</c> array of
/// side-loaded resources. Use <see cref="Patreon.Client.JsonApi.JsonApiIncludedIndex"/> to
/// resolve included resources by <c>(type, id)</c>.
/// </remarks>
public sealed record PatreonMemberWebhookEvent : PatreonWebhookEvent
{
    /// <summary>
    /// Gets the full JSON:API document payload from the webhook, including side-loaded
    /// relationships and included resources.
    /// </summary>
    public required JsonApiDocument<MemberAttributes>? Document { get; init; }

    /// <summary>Convenience accessor for the primary resource attributes.</summary>
    public MemberAttributes? Attributes => Document?.Data?.Attributes;

    /// <summary>
    /// Gets the IDs of the tiers the member is currently entitled to.
    /// Extracted from <c>data.relationships.currently_entitled_tiers.data[].id</c>
    /// at parse time so callers do not need to navigate the raw JSON.
    /// </summary>
    public IReadOnlyList<string> EntitledTierIds { get; init; } = [];
}
