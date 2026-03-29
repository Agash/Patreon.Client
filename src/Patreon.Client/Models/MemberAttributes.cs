using System.Text.Json.Serialization;

namespace Patreon.Client.Models;

/// <summary>
/// Attributes of a Patreon campaign member resource (API v2).
/// </summary>
public sealed class MemberAttributes
{
    /// <summary>Gets the member's full name.</summary>
    [JsonPropertyName("full_name")]
    public string? FullName { get; init; }

    /// <summary>Gets the member's email address.</summary>
    [JsonPropertyName("email")]
    public string? Email { get; init; }

    /// <summary>
    /// Gets the patron's current status.
    /// Values: <c>active_patron</c>, <c>declined_patron</c>, <c>former_patron</c>, or <see langword="null"/>.
    /// </summary>
    [JsonPropertyName("patron_status")]
    public string? PatronStatus { get; init; }

    /// <summary>Gets the total amount the patron is currently entitled to, in cents.</summary>
    [JsonPropertyName("currently_entitled_amount_cents")]
    public int CurrentlyEntitledAmountCents { get; init; }

    /// <summary>Gets the total lifetime support amount for this campaign, in cents.</summary>
    [JsonPropertyName("campaign_lifetime_support_cents")]
    public int CampaignLifetimeSupportCents { get; init; }

    /// <summary>Gets the date of the most recent charge attempt.</summary>
    [JsonPropertyName("last_charge_date")]
    public string? LastChargeDate { get; init; }

    /// <summary>
    /// Gets the result of the most recent charge attempt.
    /// Values: <c>Paid</c>, <c>Declined</c>, <c>Deleted</c>, <c>Pending</c>, <c>Refunded</c>, <c>Fraud</c>, <c>Other</c>.
    /// </summary>
    [JsonPropertyName("last_charge_status")]
    public string? LastChargeStatus { get; init; }

    /// <summary>Gets the date of the next scheduled charge.</summary>
    [JsonPropertyName("next_charge_date")]
    public string? NextChargeDate { get; init; }

    /// <summary>Gets the date the pledge relationship began.</summary>
    [JsonPropertyName("pledge_relationship_start")]
    public string? PledgeRelationshipStart { get; init; }

    /// <summary>Gets the pledge cadence in months (1 = monthly, 12 = annual).</summary>
    [JsonPropertyName("pledge_cadence")]
    public int? PledgeCadence { get; init; }

    /// <summary>Gets the amount the patron will pay at the next charge, in cents.</summary>
    [JsonPropertyName("will_pay_amount_cents")]
    public int WillPayAmountCents { get; init; }

    /// <summary>Gets a value indicating whether the patron is on a free trial.</summary>
    [JsonPropertyName("is_free_trial")]
    public bool IsFreeTrial { get; init; }

    /// <summary>Gets a value indicating whether the membership was gifted.</summary>
    [JsonPropertyName("is_gifted")]
    public bool IsGifted { get; init; }

    /// <summary>Gets the creator's private note about this member.</summary>
    [JsonPropertyName("note")]
    public string? Note { get; init; }
}
