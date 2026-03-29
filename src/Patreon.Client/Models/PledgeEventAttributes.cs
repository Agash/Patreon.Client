using System.Text.Json.Serialization;

namespace Patreon.Client.Models;

/// <summary>
/// Attributes of a Patreon pledge event resource, representing a single action in a member's pledge history.
/// </summary>
public sealed class PledgeEventAttributes
{
    /// <summary>
    /// Gets the type of pledge action.
    /// Known values: <c>pledge_start</c>, <c>pledge_upgrade</c>, <c>pledge_downgrade</c>,
    /// <c>pledge_delete</c>, <c>subscription</c>.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    /// <summary>Gets the pledge amount in cents at the time of this event.</summary>
    [JsonPropertyName("amount_cents")]
    public int AmountCents { get; init; }

    /// <summary>Gets the ISO 4217 currency code for this event's amount.</summary>
    [JsonPropertyName("currency_code")]
    public string? CurrencyCode { get; init; }

    /// <summary>Gets the ISO 8601 timestamp when this pledge event occurred.</summary>
    [JsonPropertyName("date")]
    public string? Date { get; init; }

    /// <summary>Gets the payment status at the time of this event.</summary>
    [JsonPropertyName("payment_status")]
    public string? PaymentStatus { get; init; }

    /// <summary>Gets the tier ID associated with this pledge event, if any.</summary>
    [JsonPropertyName("tier_id")]
    public string? TierId { get; init; }

    /// <summary>Gets the tier title at the time of this pledge event.</summary>
    [JsonPropertyName("tier_title")]
    public string? TierTitle { get; init; }
}
