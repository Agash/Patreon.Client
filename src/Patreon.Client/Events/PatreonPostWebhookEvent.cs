using Patreon.Client.JsonApi;
using Patreon.Client.Models;

namespace Patreon.Client.Events;

/// <summary>
/// Raised when Patreon sends a post lifecycle event.
/// Event types: <c>posts:publish</c>, <c>posts:update</c>, <c>posts:delete</c>.
/// </summary>
/// <remarks>
/// The full JSON:API document is available via <see cref="Document"/>, giving access to
/// related tiers and side-loaded campaign data in the <c>included</c> array.
/// Use <see cref="Patreon.Client.JsonApi.JsonApiIncludedIndex"/> to resolve included resources.
/// </remarks>
public sealed record PatreonPostWebhookEvent : PatreonWebhookEvent
{
    /// <summary>
    /// Gets the full JSON:API document payload from the webhook, including side-loaded
    /// relationships and included resources.
    /// </summary>
    public required JsonApiDocument<PostAttributes>? Document { get; init; }

    /// <summary>Convenience accessor for the primary resource attributes.</summary>
    public PostAttributes? Attributes => Document?.Data?.Attributes;
}
