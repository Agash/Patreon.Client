using System.Text.Json.Serialization;

namespace Patreon.Client.Webhooks;

/// <summary>
/// JSON:API request envelope for the webhook create and update endpoints.
/// </summary>
/// <typeparam name="TAttributes">The attributes payload carried by the request.</typeparam>
/// <remarks>
/// Named types rather than anonymous ones so the source generator can emit contracts for them.
/// Anonymous types cannot carry <see cref="JsonSerializableAttribute"/>, which would push these calls
/// onto the reflection-based serializer and break trimming and Native AOT.
/// </remarks>
internal sealed record PatreonWebhookRequest<TAttributes>(
    [property: JsonPropertyName("data")] PatreonWebhookRequestData<TAttributes> Data);

/// <summary>The <c>data</c> member of a JSON:API webhook request.</summary>
/// <typeparam name="TAttributes">The attributes payload carried by the request.</typeparam>
internal sealed record PatreonWebhookRequestData<TAttributes>(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("attributes")] TAttributes Attributes)
{
    /// <summary>The resource id. Omitted on create, required on update.</summary>
    [JsonPropertyName("id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Id { get; init; }
}

/// <summary>Attributes accepted when creating a webhook.</summary>
internal sealed record PatreonWebhookCreateAttributes(
    [property: JsonPropertyName("uri")] string Uri,
    [property: JsonPropertyName("triggers")] IReadOnlyList<string> Triggers);

/// <summary>
/// Attributes accepted when updating a webhook. Every member is optional; unset members are omitted
/// from the request body so a partial update does not clear the fields it does not mention.
/// </summary>
internal sealed record PatreonWebhookUpdateAttributes
{
    /// <summary>Whether delivery is paused.</summary>
    [JsonPropertyName("paused")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Paused { get; init; }

    /// <summary>The destination URI.</summary>
    [JsonPropertyName("uri")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Uri { get; init; }

    /// <summary>The event triggers the webhook subscribes to.</summary>
    [JsonPropertyName("triggers")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? Triggers { get; init; }
}
