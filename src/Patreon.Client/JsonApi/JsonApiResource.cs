using System.Text.Json;
using System.Text.Json.Serialization;

namespace Patreon.Client.JsonApi;

/// <summary>
/// Represents a single JSON:API resource object.
/// </summary>
/// <typeparam name="TAttributes">The type of the resource attributes.</typeparam>
public sealed class JsonApiResource<TAttributes>
{
    /// <summary>Gets the resource identifier.</summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>Gets the resource type.</summary>
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    /// <summary>Gets the resource attributes.</summary>
    [JsonPropertyName("attributes")]
    public TAttributes? Attributes { get; init; }

    /// <summary>Gets the resource relationships as raw JSON.</summary>
    [JsonPropertyName("relationships")]
    public JsonElement? Relationships { get; init; }
}
