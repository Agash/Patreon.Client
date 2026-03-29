using System.Text.Json;
using System.Text.Json.Serialization;
using Patreon.Client.JsonApi;
using Patreon.Client.Models;

namespace Patreon.Client.Webhooks;

/// <summary>
/// Source-generated JSON serialization context for Patreon API types.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(JsonApiDocument<MemberAttributes>))]
[JsonSerializable(typeof(JsonApiDocument<CampaignAttributes>))]
[JsonSerializable(typeof(JsonApiDocument<TierAttributes>))]
[JsonSerializable(typeof(JsonApiDocument<PostAttributes>))]
[JsonSerializable(typeof(JsonApiDocument<UserAttributes>))]
[JsonSerializable(typeof(JsonApiCollectionDocument<MemberAttributes>))]
[JsonSerializable(typeof(JsonApiCollectionDocument<CampaignAttributes>))]
[JsonSerializable(typeof(JsonApiCollectionDocument<TierAttributes>))]
[JsonSerializable(typeof(JsonApiCollectionDocument<PostAttributes>))]
[JsonSerializable(typeof(MemberAttributes))]
[JsonSerializable(typeof(CampaignAttributes))]
[JsonSerializable(typeof(TierAttributes))]
[JsonSerializable(typeof(PostAttributes))]
[JsonSerializable(typeof(UserAttributes))]
internal sealed partial class PatreonJsonContext : JsonSerializerContext
{
    /// <summary>
    /// Gets a pre-configured <see cref="JsonSerializerOptions"/> instance suitable
    /// for deserializing Patreon API responses. The options instance uses the web
    /// defaults (camelCase) but relies on explicit <c>[JsonPropertyName]</c> attributes
    /// for snake_case field mapping.
    /// </summary>
    internal static readonly JsonSerializerOptions WebOptions = new(JsonSerializerDefaults.Web);
}
