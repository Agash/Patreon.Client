using System.Text.Json;
using System.Text.Json.Serialization;

namespace Patreon.Client.Models;

/// <summary>
/// Attributes of a Patreon benefit resource (API v2).
/// Benefits describe what patrons receive in return for pledging to a tier.
/// Retrieve by including <c>benefits</c> when fetching a campaign or tier resource.
/// </summary>
public sealed class BenefitAttributes
{
    /// <summary>Gets the benefit title.</summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }

    /// <summary>Gets the benefit description.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// Gets the benefit type.
    /// Built-in values include <c>custom</c>; some integrations expose additional types.
    /// </summary>
    [JsonPropertyName("benefit_type")]
    public string? BenefitType { get; init; }

    /// <summary>Gets the date/time this benefit was created.</summary>
    [JsonPropertyName("created_at")]
    public string? CreatedAt { get; init; }

    /// <summary>
    /// Gets the rule type that governs when deliverables for this benefit are due.
    /// Common values: <c>monthly</c>, <c>per_post</c>.
    /// </summary>
    [JsonPropertyName("rule_type")]
    public string? RuleType { get; init; }

    /// <summary>Gets a value indicating whether this benefit is currently published and visible to patrons.</summary>
    [JsonPropertyName("is_published")]
    public bool IsPublished { get; init; }

    /// <summary>Gets a value indicating whether this benefit has been deleted by the creator.</summary>
    [JsonPropertyName("is_deleted")]
    public bool IsDeleted { get; init; }

    /// <summary>Gets a value indicating whether this benefit has ended (past its end date).</summary>
    [JsonPropertyName("is_ended")]
    public bool IsEnded { get; init; }

    /// <summary>Gets the number of tiers this benefit is attached to.</summary>
    [JsonPropertyName("tiers_count")]
    public int TiersCount { get; init; }

    /// <summary>Gets the number of deliverables due today for this benefit.</summary>
    [JsonPropertyName("deliverables_due_today_count")]
    public int DeliverablesDueTodayCount { get; init; }

    /// <summary>Gets the number of deliverables already marked as delivered for this benefit.</summary>
    [JsonPropertyName("delivered_deliverables_count")]
    public int DeliveredDeliverablesCount { get; init; }

    /// <summary>Gets the number of deliverables not yet marked as delivered for this benefit.</summary>
    [JsonPropertyName("not_delivered_deliverables_count")]
    public int NotDeliveredDeliverablesCount { get; init; }

    /// <summary>Gets the date/time the next deliverable for this benefit is due, if applicable.</summary>
    [JsonPropertyName("next_deliverable_due_date")]
    public string? NextDeliverableDueDate { get; init; }

    /// <summary>Gets an external ID if this benefit is managed by a third-party app integration.</summary>
    [JsonPropertyName("app_external_id")]
    public string? AppExternalId { get; init; }

    /// <summary>Gets extra app-specific metadata for this benefit as raw JSON.</summary>
    [JsonPropertyName("app_meta")]
    public JsonElement? AppMeta { get; init; }
}
