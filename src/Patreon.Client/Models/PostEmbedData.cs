using System.Text.Json.Serialization;

namespace Patreon.Client.Models;

/// <summary>
/// Represents the embed metadata attached to a Patreon post (e.g. a YouTube or Vimeo link).
/// </summary>
public sealed class PostEmbedData
{
    /// <summary>Gets the embed URL.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; init; }

    /// <summary>Gets the embed subject/title.</summary>
    [JsonPropertyName("subject")]
    public string? Subject { get; init; }

    /// <summary>Gets the embed description.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>Gets the embed provider name (e.g. "YouTube").</summary>
    [JsonPropertyName("provider")]
    public string? Provider { get; init; }

    /// <summary>Gets the embed provider URL (e.g. "https://www.youtube.com").</summary>
    [JsonPropertyName("provider_url")]
    public string? ProviderUrl { get; init; }

    /// <summary>Gets the embed thumbnail URL.</summary>
    [JsonPropertyName("thumbnail_url")]
    public string? ThumbnailUrl { get; init; }

    /// <summary>Gets the raw oEmbed HTML for rendering the embed.</summary>
    [JsonPropertyName("html")]
    public string? Html { get; init; }
}
