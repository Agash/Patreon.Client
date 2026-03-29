using System.Text.Json.Serialization;

namespace Patreon.Client.JsonApi;

/// <summary>Represents JSON:API document-level metadata.</summary>
public sealed class JsonApiMeta
{
    /// <summary>Gets pagination information.</summary>
    [JsonPropertyName("pagination")]
    public JsonApiPagination? Pagination { get; init; }
}

/// <summary>Represents JSON:API pagination metadata.</summary>
public sealed class JsonApiPagination
{
    /// <summary>Gets the total number of resources.</summary>
    [JsonPropertyName("total")]
    public int Total { get; init; }

    /// <summary>Gets the pagination cursors.</summary>
    [JsonPropertyName("cursors")]
    public JsonApiCursors? Cursors { get; init; }
}

/// <summary>Represents JSON:API pagination cursor values.</summary>
public sealed class JsonApiCursors
{
    /// <summary>Gets the cursor pointing to the next page of results.</summary>
    [JsonPropertyName("next")]
    public string? Next { get; init; }
}

/// <summary>Represents JSON:API document-level links.</summary>
public sealed class JsonApiLinks
{
    /// <summary>Gets the URL for the next page of results.</summary>
    [JsonPropertyName("next")]
    public string? Next { get; init; }

    /// <summary>Gets the self URL for the current document.</summary>
    [JsonPropertyName("self")]
    public string? Self { get; init; }
}

/// <summary>Represents a JSON:API error object.</summary>
public sealed class JsonApiError
{
    /// <summary>Gets the numeric error code.</summary>
    [JsonPropertyName("code")]
    public string? Code { get; init; }

    /// <summary>Gets the named error code.</summary>
    [JsonPropertyName("code_name")]
    public string? CodeName { get; init; }

    /// <summary>Gets a human-readable error detail.</summary>
    [JsonPropertyName("detail")]
    public string? Detail { get; init; }

    /// <summary>Gets the HTTP status code associated with the error.</summary>
    [JsonPropertyName("status")]
    public string? Status { get; init; }

    /// <summary>Gets a short error title.</summary>
    [JsonPropertyName("title")]
    public string? Title { get; init; }
}
