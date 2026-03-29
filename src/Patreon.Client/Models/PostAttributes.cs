using System.Text.Json;
using System.Text.Json.Serialization;

namespace Patreon.Client.Models;

/// <summary>
/// Attributes of a Patreon post resource (API v2).
/// </summary>
public sealed class PostAttributes
{
    /// <summary>Gets the post title.</summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>Gets the post content/body.</summary>
    [JsonPropertyName("content")]
    public string? Content { get; init; }

    /// <summary>Gets the canonical URL of the post.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; init; }

    /// <summary>Gets the date/time the post was published.</summary>
    [JsonPropertyName("published_at")]
    public string? PublishedAt { get; init; }

    /// <summary>Gets a value indicating whether the post is publicly visible.</summary>
    [JsonPropertyName("is_public")]
    public bool IsPublic { get; init; }

    /// <summary>Gets the tier access data as raw JSON.</summary>
    [JsonPropertyName("tiers")]
    public JsonElement? Tiers { get; init; }
}
