using System.Text.Json;
using System.Text.Json.Serialization;

namespace Patreon.Client.JsonApi;

/// <summary>
/// Represents a JSON:API document containing a single resource object.
/// </summary>
/// <typeparam name="TAttributes">The type of the resource attributes.</typeparam>
public sealed class JsonApiDocument<TAttributes>
{
    /// <summary>Gets the primary resource data.</summary>
    [JsonPropertyName("data")]
    public JsonApiResource<TAttributes>? Data { get; init; }

    /// <summary>Gets included side-loaded resources.</summary>
    [JsonPropertyName("included")]
    public IReadOnlyList<JsonElement>? Included { get; init; }

    /// <summary>Gets document-level metadata.</summary>
    [JsonPropertyName("meta")]
    public JsonApiMeta? Meta { get; init; }

    /// <summary>Gets document-level links.</summary>
    [JsonPropertyName("links")]
    public JsonApiLinks? Links { get; init; }

    /// <summary>Gets any errors returned by the API.</summary>
    [JsonPropertyName("errors")]
    public IReadOnlyList<JsonApiError>? Errors { get; init; }
}

/// <summary>
/// Represents a JSON:API document containing a collection of resource objects.
/// </summary>
/// <typeparam name="TAttributes">The type of the resource attributes.</typeparam>
public sealed class JsonApiCollectionDocument<TAttributes>
{
    /// <summary>Gets the primary resource data collection.</summary>
    [JsonPropertyName("data")]
    public IReadOnlyList<JsonApiResource<TAttributes>>? Data { get; init; }

    /// <summary>Gets included side-loaded resources.</summary>
    [JsonPropertyName("included")]
    public IReadOnlyList<JsonElement>? Included { get; init; }

    /// <summary>Gets document-level metadata.</summary>
    [JsonPropertyName("meta")]
    public JsonApiMeta? Meta { get; init; }

    /// <summary>Gets document-level links.</summary>
    [JsonPropertyName("links")]
    public JsonApiLinks? Links { get; init; }

    /// <summary>Gets any errors returned by the API.</summary>
    [JsonPropertyName("errors")]
    public IReadOnlyList<JsonApiError>? Errors { get; init; }
}
