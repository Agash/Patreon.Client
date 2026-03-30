using System.Text.Json;
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

    /// <summary>Gets the URL of the user's profile image (full-size).</summary>
    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; init; }

    /// <summary>Gets the URL of the user's profile thumbnail image.</summary>
    [JsonPropertyName("thumb_url")]
    public string? ThumbUrl { get; init; }

    /// <summary>Gets the canonical Patreon profile URL.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; init; }

    /// <summary>Gets the user's vanity URL segment.</summary>
    [JsonPropertyName("vanity")]
    public string? Vanity { get; init; }

    /// <summary>Gets the user's biography / about text.</summary>
    [JsonPropertyName("about")]
    public string? About { get; init; }

    /// <summary>Gets the date/time the Patreon account was created.</summary>
    [JsonPropertyName("created")]
    public string? Created { get; init; }

    /// <summary>Gets a value indicating whether this user is a campaign creator.</summary>
    [JsonPropertyName("is_creator")]
    public bool IsCreator { get; init; }

    /// <summary>Gets a value indicating whether this user's email address has been verified.</summary>
    [JsonPropertyName("is_email_verified")]
    public bool IsEmailVerified { get; init; }

    /// <summary>Gets a value indicating whether this user has chosen to hide their pledges publicly.</summary>
    [JsonPropertyName("hide_pledges")]
    public bool HidePledges { get; init; }

    /// <summary>
    /// Gets the user's connected social platform accounts as raw JSON.
    /// The structure is a dictionary keyed by platform name (e.g. <c>youtube</c>, <c>twitter</c>,
    /// <c>twitch</c>, <c>discord</c>) with per-platform <c>url</c> and <c>user_id</c> fields.
    /// Use <see cref="JsonElement.TryGetProperty"/> to access individual platforms.
    /// </summary>
    [JsonPropertyName("social_connections")]
    public JsonElement? SocialConnections { get; init; }
}
