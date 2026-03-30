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

    /// <summary>Gets a shortened teaser of the post content shown to non-patrons.</summary>
    [JsonPropertyName("content_teaser")]
    public string? ContentTeaser { get; init; }

    /// <summary>Gets the canonical URL of the post.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; init; }

    /// <summary>Gets the date/time the post was published.</summary>
    [JsonPropertyName("published_at")]
    public string? PublishedAt { get; init; }

    /// <summary>Gets a value indicating whether the post is publicly visible.</summary>
    [JsonPropertyName("is_public")]
    public bool IsPublic { get; init; }

    /// <summary>
    /// Gets the post type.
    /// Common values: <c>text_only</c>, <c>image_file</c>, <c>video_embed</c>,
    /// <c>video_external_url</c>, <c>audio_file</c>, <c>audio_embed</c>, <c>link</c>.
    /// </summary>
    [JsonPropertyName("post_type")]
    public string? PostType { get; init; }

    /// <summary>Gets the embed metadata if this post links to external media (e.g. YouTube).</summary>
    [JsonPropertyName("embed")]
    public PostEmbedData? Embed { get; init; }

    /// <summary>Gets the cover image metadata for this post, if any.</summary>
    [JsonPropertyName("image")]
    public PostImageData? Image { get; init; }

    /// <summary>Gets the internal app ID associated with this post, if any.</summary>
    [JsonPropertyName("app_id")]
    public long? AppId { get; init; }

    /// <summary>Gets the internal app status of this post (e.g. <c>published</c>, <c>draft</c>).</summary>
    [JsonPropertyName("app_status")]
    public string? AppStatus { get; init; }

    /// <summary>
    /// Gets the minimum pledge amount in cents required to view this post, or <see langword="null"/> for free.
    /// </summary>
    [JsonPropertyName("min_cents_pledged_to_view")]
    public int? MinCentsPledgedToView { get; init; }

    /// <summary>Gets the number of patrons who can see this post.</summary>
    [JsonPropertyName("patron_count")]
    public int PatronCount { get; init; }

    /// <summary>Gets the number of likes on this post.</summary>
    [JsonPropertyName("like_count")]
    public int LikeCount { get; init; }

    /// <summary>Gets the number of comments on this post.</summary>
    [JsonPropertyName("comment_count")]
    public int CommentCount { get; init; }

    /// <summary>Gets a value indicating whether the post was made by the campaign owner.</summary>
    [JsonPropertyName("was_posted_by_campaign_owner")]
    public bool WasPostedByCampaignOwner { get; init; }

    /// <summary>Gets the date/time at which the post's visibility will change, if scheduled.</summary>
    [JsonPropertyName("change_visibility_at")]
    public string? ChangeVisibilityAt { get; init; }

    /// <summary>Gets the scheduled publish date/time if the post has not yet been published.</summary>
    [JsonPropertyName("scheduled_for")]
    public string? ScheduledFor { get; init; }
}
