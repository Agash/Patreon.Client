using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Patreon.Client.Abstractions;
using Patreon.Client.JsonApi;
using Patreon.Client.Models;

namespace Patreon.Client;

/// <summary>
/// Default <see cref="IPatreonApiClient"/> implementation backed by an <see cref="HttpClient"/>.
/// </summary>
public sealed class PatreonApiClient : IPatreonApiClient
{
    private static readonly JsonSerializerOptions s_jsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly ILogger<PatreonApiClient> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="PatreonApiClient"/>.
    /// </summary>
    /// <param name="httpClient">The <see cref="HttpClient"/> pre-configured with the base address and auth handler.</param>
    /// <param name="logger">The logger instance.</param>
    public PatreonApiClient(HttpClient httpClient, ILogger<PatreonApiClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<JsonApiDocument<UserAttributes>?> GetIdentityAsync(
        IEnumerable<string>? fields = null,
        IEnumerable<string>? include = null,
        CancellationToken cancellationToken = default)
    {
        string url = BuildUrl("identity", "user", fields, include);
        return GetAsync<JsonApiDocument<UserAttributes>>(url, cancellationToken);
    }

    /// <inheritdoc />
    public Task<JsonApiCollectionDocument<CampaignAttributes>?> GetCampaignsAsync(
        IEnumerable<string>? fields = null,
        IEnumerable<string>? include = null,
        CancellationToken cancellationToken = default)
    {
        string url = BuildUrl("campaigns", "campaign", fields, include);
        return GetAsync<JsonApiCollectionDocument<CampaignAttributes>>(url, cancellationToken);
    }

    /// <inheritdoc />
    public Task<JsonApiDocument<CampaignAttributes>?> GetCampaignAsync(
        string campaignId,
        IEnumerable<string>? fields = null,
        IEnumerable<string>? include = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(campaignId);
        string url = BuildUrl($"campaigns/{Uri.EscapeDataString(campaignId)}", "campaign", fields, include);
        return GetAsync<JsonApiDocument<CampaignAttributes>>(url, cancellationToken);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<JsonApiResource<MemberAttributes>> GetCampaignMembersAsync(
        string campaignId,
        IEnumerable<string>? fields = null,
        IEnumerable<string>? include = null,
        int pageSize = 20,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(campaignId);

        string? cursor = null;

        do
        {
            string url = BuildMembersUrl(campaignId, "member", fields, include, pageSize, cursor);
            JsonApiCollectionDocument<MemberAttributes>? page =
                await GetAsync<JsonApiCollectionDocument<MemberAttributes>>(url, cancellationToken)
                    .ConfigureAwait(false);

            if (page?.Data is null || page.Data.Count == 0)
            {
                yield break;
            }

            foreach (JsonApiResource<MemberAttributes> resource in page.Data)
            {
                yield return resource;
            }

            cursor = page.Meta?.Pagination?.Cursors?.Next;
        }
        while (!string.IsNullOrEmpty(cursor));
    }

    /// <inheritdoc />
    public Task<JsonApiDocument<MemberAttributes>?> GetMemberAsync(
        string memberId,
        IEnumerable<string>? fields = null,
        IEnumerable<string>? include = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(memberId);
        string url = BuildUrl($"members/{Uri.EscapeDataString(memberId)}", "member", fields, include);
        return GetAsync<JsonApiDocument<MemberAttributes>>(url, cancellationToken);
    }

    /// <inheritdoc />
    public Task<JsonApiCollectionDocument<PostAttributes>?> GetCampaignPostsAsync(
        string campaignId,
        IEnumerable<string>? fields = null,
        IEnumerable<string>? include = null,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(campaignId);
        string url = BuildPagedUrl(
            $"campaigns/{Uri.EscapeDataString(campaignId)}/posts",
            "post", fields, include, pageSize, null);
        return GetAsync<JsonApiCollectionDocument<PostAttributes>>(url, cancellationToken);
    }

    /// <inheritdoc />
    public Task<JsonApiDocument<PostAttributes>?> GetPostAsync(
        string postId,
        IEnumerable<string>? fields = null,
        IEnumerable<string>? include = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(postId);
        string url = BuildUrl($"posts/{Uri.EscapeDataString(postId)}", "post", fields, include);
        return GetAsync<JsonApiDocument<PostAttributes>>(url, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<JsonApiResource<TierAttributes>>?> GetCampaignTiersAsync(
        string campaignId,
        IEnumerable<string>? tierFields = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(campaignId);
        string url = BuildUrl(
            $"campaigns/{Uri.EscapeDataString(campaignId)}",
            "campaign",
            fields: null,
            include: ["tiers"]);

        string[]? tierFieldArr = tierFields?.ToArray();
        if (tierFieldArr is { Length: > 0 })
        {
            url += $"&fields[tier]={Uri.EscapeDataString(string.Join(",", tierFieldArr))}";
        }

        JsonApiDocument<CampaignAttributes>? doc =
            await GetAsync<JsonApiDocument<CampaignAttributes>>(url, cancellationToken)
                .ConfigureAwait(false);

        if (doc?.Included is null)
            return null;

        List<JsonApiResource<TierAttributes>> tiers = [];
        foreach (JsonElement item in doc.Included)
        {
            if (!item.TryGetProperty("type", out JsonElement typeElem)
                || typeElem.GetString() != "tier")
            {
                continue;
            }

            string tierId = item.TryGetProperty("id", out JsonElement idElem)
                ? idElem.GetString() ?? string.Empty
                : string.Empty;

            TierAttributes? attrs = null;
            if (item.TryGetProperty("attributes", out JsonElement attrsElem))
            {
                try { attrs = attrsElem.Deserialize<TierAttributes>(s_jsonOptions); }
                catch (JsonException) { }
            }

            tiers.Add(new JsonApiResource<TierAttributes>
            {
                Id = tierId,
                Type = "tier",
                Attributes = attrs,
            });
        }

        return tiers;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<JsonApiResource<BenefitAttributes>>?> GetCampaignBenefitsAsync(
        string campaignId,
        IEnumerable<string>? benefitFields = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(campaignId);
        string url = BuildUrl(
            $"campaigns/{Uri.EscapeDataString(campaignId)}",
            "campaign",
            fields: null,
            include: ["benefits"]);

        string[]? benefitFieldArr = benefitFields?.ToArray();
        if (benefitFieldArr is { Length: > 0 })
        {
            url += $"&fields[benefit]={Uri.EscapeDataString(string.Join(",", benefitFieldArr))}";
        }

        JsonApiDocument<CampaignAttributes>? doc =
            await GetAsync<JsonApiDocument<CampaignAttributes>>(url, cancellationToken)
                .ConfigureAwait(false);

        if (doc?.Included is null)
            return null;

        List<JsonApiResource<BenefitAttributes>> benefits = [];
        foreach (JsonElement item in doc.Included)
        {
            if (!item.TryGetProperty("type", out JsonElement typeElem)
                || typeElem.GetString() != "benefit")
            {
                continue;
            }

            string benefitId = item.TryGetProperty("id", out JsonElement idElem)
                ? idElem.GetString() ?? string.Empty
                : string.Empty;

            BenefitAttributes? attrs = null;
            if (item.TryGetProperty("attributes", out JsonElement attrsElem))
            {
                try { attrs = attrsElem.Deserialize<BenefitAttributes>(s_jsonOptions); }
                catch (JsonException) { }
            }

            benefits.Add(new JsonApiResource<BenefitAttributes>
            {
                Id = benefitId,
                Type = "benefit",
                Attributes = attrs,
            });
        }

        return benefits;
    }

    /// <inheritdoc />
    public Task<JsonApiCollectionDocument<WebhookAttributes>?> GetWebhooksAsync(
        CancellationToken cancellationToken = default) =>
        GetAsync<JsonApiCollectionDocument<WebhookAttributes>>("webhooks", cancellationToken);

    /// <inheritdoc />
    public async Task<JsonApiDocument<WebhookAttributes>?> CreateWebhookAsync(
        string uri,
        IReadOnlyList<string> triggers,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(uri);
        ArgumentNullException.ThrowIfNull(triggers);

        object payload = new
        {
            data = new
            {
                type = "webhook",
                attributes = new { uri, triggers },
            },
        };

        using HttpResponseMessage response = await _httpClient
            .PostAsJsonAsync("webhooks", payload, cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Patreon API POST webhooks returned HTTP {StatusCode}.",
                (int)response.StatusCode);
            return null;
        }

        using System.IO.Stream stream =
            await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return await JsonSerializer
            .DeserializeAsync<JsonApiDocument<WebhookAttributes>>(stream, s_jsonOptions, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<JsonApiDocument<WebhookAttributes>?> UpdateWebhookAsync(
        string webhookId,
        bool? paused = null,
        string? uri = null,
        IReadOnlyList<string>? triggers = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(webhookId);

        Dictionary<string, object?> attributes = [];
        if (paused.HasValue) attributes["paused"] = paused.Value;
        if (uri is not null) attributes["uri"] = uri;
        if (triggers is not null) attributes["triggers"] = triggers;

        object payload = new
        {
            data = new
            {
                type = "webhook",
                id = webhookId,
                attributes,
            },
        };

        using HttpRequestMessage request = new(
            new HttpMethod("PATCH"),
            $"webhooks/{Uri.EscapeDataString(webhookId)}")
        {
            Content = JsonContent.Create(payload),
        };

        using HttpResponseMessage response = await _httpClient
            .SendAsync(request, cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Patreon API PATCH webhooks/{WebhookId} returned HTTP {StatusCode}.",
                webhookId,
                (int)response.StatusCode);
            return null;
        }

        using System.IO.Stream stream =
            await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return await JsonSerializer
            .DeserializeAsync<JsonApiDocument<WebhookAttributes>>(stream, s_jsonOptions, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DeleteWebhookAsync(string webhookId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(webhookId);

        using HttpResponseMessage response = await _httpClient
            .DeleteAsync($"webhooks/{Uri.EscapeDataString(webhookId)}", cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Patreon API DELETE webhooks/{WebhookId} returned HTTP {StatusCode}.",
                webhookId,
                (int)response.StatusCode);
        }
    }

    private async Task<T?> GetAsync<T>(string url, CancellationToken cancellationToken)
    {
        using HttpResponseMessage response =
            await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Patreon API request to {Url} returned HTTP {StatusCode}.",
                url,
                (int)response.StatusCode);
            return default;
        }

        using System.IO.Stream stream =
            await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        return await JsonSerializer.DeserializeAsync<T>(stream, s_jsonOptions, cancellationToken)
            .ConfigureAwait(false);
    }

    private static string BuildUrl(
        string path,
        string resourceType,
        IEnumerable<string>? fields,
        IEnumerable<string>? include)
    {
        StringBuilder sb = new(path);
        bool hasQuery = false;

        string[]? fieldArr = fields?.ToArray();
        if (fieldArr is { Length: > 0 })
        {
            sb.Append(hasQuery ? '&' : '?');
            sb.Append("fields[");
            sb.Append(resourceType);
            sb.Append("]=");
            sb.Append(Uri.EscapeDataString(string.Join(",", fieldArr)));
            hasQuery = true;
        }

        string[]? includeArr = include?.ToArray();
        if (includeArr is { Length: > 0 })
        {
            sb.Append(hasQuery ? '&' : '?');
            sb.Append("include=");
            sb.Append(Uri.EscapeDataString(string.Join(",", includeArr)));
        }

        return sb.ToString();
    }

    private static string BuildPagedUrl(
        string path,
        string resourceType,
        IEnumerable<string>? fields,
        IEnumerable<string>? include,
        int pageSize,
        string? cursor)
    {
        StringBuilder sb = new(path);
        bool hasQuery = false;

        string[]? fieldArr = fields?.ToArray();
        if (fieldArr is { Length: > 0 })
        {
            sb.Append(hasQuery ? '&' : '?');
            sb.Append("fields[");
            sb.Append(resourceType);
            sb.Append("]=");
            sb.Append(Uri.EscapeDataString(string.Join(",", fieldArr)));
            hasQuery = true;
        }

        string[]? includeArr = include?.ToArray();
        if (includeArr is { Length: > 0 })
        {
            sb.Append(hasQuery ? '&' : '?');
            sb.Append("include=");
            sb.Append(Uri.EscapeDataString(string.Join(",", includeArr)));
            hasQuery = true;
        }

        sb.Append(hasQuery ? '&' : '?');
        sb.Append("page[count]=");
        sb.Append(pageSize);
        hasQuery = true;

        if (!string.IsNullOrEmpty(cursor))
        {
            sb.Append(hasQuery ? '&' : '?');
            sb.Append("page[cursor]=");
            sb.Append(Uri.EscapeDataString(cursor));
        }

        return sb.ToString();
    }

    private static string BuildMembersUrl(
        string campaignId,
        string resourceType,
        IEnumerable<string>? fields,
        IEnumerable<string>? include,
        int pageSize,
        string? cursor)
    {
        return BuildPagedUrl(
            $"campaigns/{Uri.EscapeDataString(campaignId)}/members",
            resourceType, fields, include, pageSize, cursor);
    }
}
