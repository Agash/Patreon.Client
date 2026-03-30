using System.Text.Json.Serialization;

namespace Patreon.Client.Models;

/// <summary>
/// Attributes of a Patreon address resource (API v2).
/// Shipping addresses are associated with members that have pledged to a tier
/// requiring a physical address (<see cref="TierAttributes.RequiresShipping"/>).
/// Retrieve by including <c>address</c> when fetching a member resource.
/// </summary>
public sealed class AddressAttributes
{
    /// <summary>Gets the addressee name (person or organisation the package is addressed to).</summary>
    [JsonPropertyName("addressee")]
    public string? Addressee { get; init; }

    /// <summary>Gets the first line of the street address.</summary>
    [JsonPropertyName("line_1")]
    public string? Line1 { get; init; }

    /// <summary>Gets the second line of the street address (apartment, suite, etc.).</summary>
    [JsonPropertyName("line_2")]
    public string? Line2 { get; init; }

    /// <summary>Gets the city.</summary>
    [JsonPropertyName("city")]
    public string? City { get; init; }

    /// <summary>Gets the state, province, or region.</summary>
    [JsonPropertyName("state")]
    public string? State { get; init; }

    /// <summary>Gets the postal or ZIP code.</summary>
    [JsonPropertyName("postal_code")]
    public string? PostalCode { get; init; }

    /// <summary>Gets the ISO 3166-1 alpha-2 country code (e.g. <c>US</c>).</summary>
    [JsonPropertyName("country")]
    public string? Country { get; init; }

    /// <summary>Gets the phone number associated with this address, if provided.</summary>
    [JsonPropertyName("phone_number")]
    public string? PhoneNumber { get; init; }

    /// <summary>Gets the date/time this address was created.</summary>
    [JsonPropertyName("created_at")]
    public string? CreatedAt { get; init; }
}
