using System.Text.Json.Serialization;

namespace Patreon.Client.Models;

/// <summary>
/// Represents the cover image metadata for a Patreon post.
/// </summary>
public sealed class PostImageData
{
    /// <summary>Gets the standard-size image URL.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; init; }

    /// <summary>Gets the large-size image URL.</summary>
    [JsonPropertyName("large_url")]
    public string? LargeUrl { get; init; }

    /// <summary>Gets the thumbnail-size image URL.</summary>
    [JsonPropertyName("thumb_url")]
    public string? ThumbUrl { get; init; }
}
