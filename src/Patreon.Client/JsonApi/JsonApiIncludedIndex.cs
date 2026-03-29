using System.Text.Json;

namespace Patreon.Client.JsonApi;

/// <summary>
/// Builds a lookup index from a JSON:API <c>included</c> array, enabling O(1)
/// resolution of relationship references by <c>(type, id)</c> pair.
/// </summary>
/// <remarks>
/// <para>
/// In JSON:API responses, related resources are side-loaded into the top-level
/// <c>included</c> array as generic JSON elements. The primary resource's
/// <c>relationships</c> object contains <c>{ "data": { "type": "tier", "id": "123" } }</c>
/// references. This index lets you resolve those references to their full attribute objects.
/// </para>
/// <para>
/// Example:
/// <code>
/// var index = new JsonApiIncludedIndex(document.Included);
/// TierAttributes? tier = index.TryGetAttributesAs&lt;TierAttributes&gt;("tier", tierId);
/// </code>
/// </para>
/// </remarks>
public sealed class JsonApiIncludedIndex
{
    private static readonly JsonSerializerOptions s_options = new(JsonSerializerDefaults.Web);

    private readonly IReadOnlyDictionary<(string Type, string Id), JsonElement> _index;

    /// <summary>
    /// Gets the number of included resources indexed.
    /// </summary>
    public int Count => _index.Count;

    /// <summary>
    /// Initializes a new <see cref="JsonApiIncludedIndex"/> from a JSON:API included array.
    /// Passing <see langword="null"/> or an empty list results in an empty index.
    /// </summary>
    /// <param name="included">The <c>included</c> array from a JSON:API document.</param>
    public JsonApiIncludedIndex(IReadOnlyList<JsonElement>? included)
    {
        if (included is null || included.Count == 0)
        {
            _index = new Dictionary<(string, string), JsonElement>(0);
            return;
        }

        Dictionary<(string, string), JsonElement> dict = new(included.Count);
        foreach (JsonElement item in included)
        {
            if (!item.TryGetProperty("type", out JsonElement typeElem)
                || !item.TryGetProperty("id", out JsonElement idElem))
            {
                continue;
            }

            string? type = typeElem.GetString();
            string? id = idElem.GetString();
            if (!string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(id))
            {
                dict[(type, id)] = item;
            }
        }

        _index = dict;
    }

    /// <summary>
    /// Returns <see langword="true"/> if a resource with the given type and ID is in the index.
    /// </summary>
    public bool Contains(string type, string id)
    {
        ArgumentException.ThrowIfNullOrEmpty(type);
        ArgumentException.ThrowIfNullOrEmpty(id);
        return _index.ContainsKey((type, id));
    }

    /// <summary>
    /// Attempts to retrieve the raw <see cref="JsonElement"/> for a resource by type and ID.
    /// </summary>
    /// <param name="type">The JSON:API resource type (e.g. <c>tier</c>, <c>user</c>).</param>
    /// <param name="id">The resource ID.</param>
    /// <param name="element">When found, the full resource JSON element including attributes and relationships.</param>
    /// <returns><see langword="true"/> if the resource was found; otherwise <see langword="false"/>.</returns>
    public bool TryGet(string type, string id, out JsonElement element)
    {
        ArgumentException.ThrowIfNullOrEmpty(type);
        ArgumentException.ThrowIfNullOrEmpty(id);
        return _index.TryGetValue((type, id), out element);
    }

    /// <summary>
    /// Attempts to deserialize the <c>attributes</c> object of an included resource as <typeparamref name="T"/>.
    /// Returns <see langword="null"/> if the resource is not found or deserialization fails.
    /// </summary>
    /// <typeparam name="T">The attributes type to deserialize into.</typeparam>
    /// <param name="type">The JSON:API resource type.</param>
    /// <param name="id">The resource ID.</param>
    /// <param name="options">Optional custom JSON serializer options. Defaults to web-defaults.</param>
    /// <returns>The deserialized attributes, or <see langword="null"/>.</returns>
    public T? TryGetAttributesAs<T>(string type, string id, JsonSerializerOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(type);
        ArgumentException.ThrowIfNullOrEmpty(id);

        if (!_index.TryGetValue((type, id), out JsonElement element))
        {
            return default;
        }

        if (!element.TryGetProperty("attributes", out JsonElement attrsElem))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(attrsElem, options ?? s_options);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    /// <summary>
    /// Resolves all resources of a given type from the index.
    /// Useful for iterating all side-loaded tiers, users, etc.
    /// </summary>
    /// <param name="type">The JSON:API resource type to filter by.</param>
    /// <returns>A sequence of raw JSON elements for matching resources.</returns>
    public IEnumerable<JsonElement> GetAllOfType(string type)
    {
        ArgumentException.ThrowIfNullOrEmpty(type);
        foreach (((string t, string _) key, JsonElement value) in _index)
        {
            if (string.Equals(key.t, type, StringComparison.Ordinal))
            {
                yield return value;
            }
        }
    }
}
