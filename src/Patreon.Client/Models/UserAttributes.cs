using System.Text.Json.Serialization;

namespace Patreon.Client.Models;

/// <summary>
/// Attributes of a Patreon user resource (API v2).
/// </summary>
public sealed class UserAttributes
{
    /// <summary>Gets the user's full name.</summary>
    [JsonPropertyName("full_name")]
    public string? FullName { get; init; }

    /// <summary>Gets the user's email address.</summary>
    [JsonPropertyName("email")]
    public string? Email { get; init; }

    /// <summary>Gets the URL of the user's profile image.</summary>
    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; init; }

    /// <summary>Gets the canonical Patreon profile URL.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; init; }

    /// <summary>Gets the user's vanity URL segment.</summary>
    [JsonPropertyName("vanity")]
    public string? Vanity { get; init; }
}
