using System.Text.Json.Serialization;

namespace Patreon.Client.Models;

/// <summary>
/// Attributes of a Patreon campaign tier resource (API v2).
/// </summary>
public sealed class TierAttributes
{
    /// <summary>Gets the tier title.</summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>Gets the tier description.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>Gets the pledge amount for this tier, in cents.</summary>
    [JsonPropertyName("amount_cents")]
    public int AmountCents { get; init; }

    /// <summary>Gets the current number of patrons at this tier.</summary>
    [JsonPropertyName("patron_count")]
    public int PatronCount { get; init; }

    /// <summary>Gets a value indicating whether this tier is currently published.</summary>
    [JsonPropertyName("published")]
    public bool Published { get; init; }

    /// <summary>Gets the date/time this tier was published.</summary>
    [JsonPropertyName("published_at")]
    public string? PublishedAt { get; init; }

    /// <summary>Gets the date/time this tier was created.</summary>
    [JsonPropertyName("created_at")]
    public string? CreatedAt { get; init; }

    /// <summary>Gets the URL for this tier.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; init; }

    /// <summary>Gets the URL of the tier image.</summary>
    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; init; }

    /// <summary>Gets the maximum number of patrons allowed at this tier, or <see langword="null"/> for unlimited.</summary>
    [JsonPropertyName("user_limit")]
    public int? UserLimit { get; init; }

    /// <summary>Gets the number of remaining slots at this tier, or <see langword="null"/> if unlimited.</summary>
    [JsonPropertyName("remaining")]
    public int? Remaining { get; init; }

    /// <summary>Gets a value indicating whether this tier requires a shipping address.</summary>
    [JsonPropertyName("requires_shipping")]
    public bool RequiresShipping { get; init; }

    /// <summary>Gets the Discord role IDs that patrons at this tier receive.</summary>
    [JsonPropertyName("discord_role_ids")]
    public IReadOnlyList<string>? DiscordRoleIds { get; init; }
}
