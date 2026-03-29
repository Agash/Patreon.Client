using System.Text.Json.Serialization;

namespace Patreon.Client.Models;

/// <summary>
/// Attributes of a Patreon campaign resource (API v2).
/// </summary>
public sealed class CampaignAttributes
{
    /// <summary>Gets the name of what the creator is creating.</summary>
    [JsonPropertyName("creation_name")]
    public string? CreationName { get; init; }

    /// <summary>Gets the campaign name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>Gets the campaign summary / description.</summary>
    [JsonPropertyName("summary")]
    public string? Summary { get; init; }

    /// <summary>Gets the current number of active patrons.</summary>
    [JsonPropertyName("patron_count")]
    public int PatronCount { get; init; }

    /// <summary>Gets the currency used for pledges (e.g. <c>USD</c>).</summary>
    [JsonPropertyName("currency")]
    public string? Currency { get; init; }

    /// <summary>Gets the canonical Patreon URL for this campaign.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; init; }

    /// <summary>Gets the URL of the campaign cover image.</summary>
    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; init; }

    /// <summary>Gets the date/time the campaign was published.</summary>
    [JsonPropertyName("published_at")]
    public string? PublishedAt { get; init; }

    /// <summary>Gets the date/time the campaign was created.</summary>
    [JsonPropertyName("created_at")]
    public string? CreatedAt { get; init; }

    /// <summary>Gets a value indicating whether the campaign uses monthly billing.</summary>
    [JsonPropertyName("is_monthly")]
    public bool IsMonthly { get; init; }

    /// <summary>Gets a value indicating whether the campaign is marked as NSFW.</summary>
    [JsonPropertyName("is_nsfw")]
    public bool IsNsfw { get; init; }

    /// <summary>Gets the associated Discord server ID, if any.</summary>
    [JsonPropertyName("discord_server_id")]
    public string? DiscordServerId { get; init; }

    /// <summary>Gets the campaign's vanity URL segment.</summary>
    [JsonPropertyName("vanity")]
    public string? Vanity { get; init; }
}
