using System.Text.Json;
using Agash.Webhook.Abstractions;
using Patreon.Client.Events;
using Patreon.Client.JsonApi;
using Patreon.Client.Models;

namespace Patreon.Client.Webhooks;

/// <summary>
/// Transport-neutral Patreon webhook handler. Verifies the HMAC-MD5 signature
/// and deserializes the JSON:API payload into a typed <see cref="PatreonWebhookEvent"/>.
/// </summary>
public sealed class PatreonWebhookHandler
{
    private static readonly JsonSerializerOptions s_jsonOptions = new(JsonSerializerDefaults.Web);

    private readonly PatreonWebhookSignatureVerifier _verifier;

    /// <summary>Initializes a new instance of <see cref="PatreonWebhookHandler"/>.</summary>
    public PatreonWebhookHandler(PatreonWebhookSignatureVerifier verifier)
    {
        _verifier = verifier ?? throw new ArgumentNullException(nameof(verifier));
    }

    /// <summary>Handles an incoming Patreon webhook request.</summary>
    public Task<WebhookHandleResult<PatreonWebhookEvent>> HandleAsync(
        WebhookRequest request,
        PatreonWebhookOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrEmpty(options.WebhookSecret);

        cancellationToken.ThrowIfCancellationRequested();

        if (!string.Equals(request.Method, "POST", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(Failure(405, false, false, "Patreon webhooks must use POST."));

        if (!request.HasContentType("application/json"))
            return Task.FromResult(Failure(400, false, false, "Expected application/json content type."));

        byte[] body = request.Body ?? [];

        if (!_verifier.Verify(body, request.Headers, options.WebhookSecret))
        {
            return Task.FromResult(Failure(401, false, false,
                $"Invalid or missing Patreon webhook signature (header: {PatreonWebhookSignatureVerifier.SignatureHeaderName})."));
        }

        string eventType = "unknown";
        if (request.Headers.TryGetValue(PatreonWebhookSignatureVerifier.EventHeaderName, out string[]? evtValues)
            && evtValues.Length > 0 && !string.IsNullOrWhiteSpace(evtValues[0]))
        {
            eventType = evtValues[0];
        }

        try
        {
            PatreonWebhookEvent evt = eventType switch
            {
                "members:create" or "members:update" or "members:delete" =>
                    BuildMemberEvent(eventType, body),

                "members:pledge:create" or "members:pledge:update" or "members:pledge:delete" =>
                    BuildPledgeEvent(eventType, body),

                "posts:publish" or "posts:update" or "posts:delete" =>
                    BuildPostEvent(eventType, body),

                _ => BuildUnknownEvent(eventType, body),
            };

            bool isKnown = evt is not PatreonUnknownWebhookEvent;
            return Task.FromResult(new WebhookHandleResult<PatreonWebhookEvent>
            {
                Response = WebhookResponse.Empty(200),
                IsAuthenticated = true,
                IsKnownEvent = isKnown,
                Event = evt,
                FailureReason = null,
            });
        }
        catch (JsonException ex)
        {
            return Task.FromResult(Failure(400, true, false, $"Failed to parse Patreon webhook JSON: {ex.Message}"));
        }
    }

    private PatreonMemberWebhookEvent BuildMemberEvent(string eventType, byte[] body)
    {
        JsonApiDocument<MemberAttributes>? doc =
            JsonSerializer.Deserialize<JsonApiDocument<MemberAttributes>>(body, s_jsonOptions);

        return new PatreonMemberWebhookEvent
        {
            EventType = eventType,
            ResourceId = doc?.Data?.Id ?? string.Empty,
            ResourceType = doc?.Data?.Type ?? "member",
            Document = doc,
        };
    }

    private PatreonPledgeWebhookEvent BuildPledgeEvent(string eventType, byte[] body)
    {
        JsonApiDocument<MemberAttributes>? doc =
            JsonSerializer.Deserialize<JsonApiDocument<MemberAttributes>>(body, s_jsonOptions);

        return new PatreonPledgeWebhookEvent
        {
            EventType = eventType,
            ResourceId = doc?.Data?.Id ?? string.Empty,
            ResourceType = doc?.Data?.Type ?? "member",
            Document = doc,
        };
    }

    private PatreonPostWebhookEvent BuildPostEvent(string eventType, byte[] body)
    {
        JsonApiDocument<PostAttributes>? doc =
            JsonSerializer.Deserialize<JsonApiDocument<PostAttributes>>(body, s_jsonOptions);

        return new PatreonPostWebhookEvent
        {
            EventType = eventType,
            ResourceId = doc?.Data?.Id ?? string.Empty,
            ResourceType = doc?.Data?.Type ?? "post",
            Document = doc,
        };
    }

    private static PatreonUnknownWebhookEvent BuildUnknownEvent(string eventType, byte[] body)
    {
        // Capture id/type from the raw JSON without forcing full deserialization
        string resourceId = string.Empty;
        string resourceType = string.Empty;

        try
        {
            using JsonDocument doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("data", out JsonElement dataElem))
            {
                if (dataElem.TryGetProperty("id", out JsonElement idElem))
                    resourceId = idElem.GetString() ?? string.Empty;
                if (dataElem.TryGetProperty("type", out JsonElement typeElem))
                    resourceType = typeElem.GetString() ?? string.Empty;
            }
        }
        catch (JsonException) { }

        return new PatreonUnknownWebhookEvent
        {
            EventType = eventType,
            ResourceId = resourceId,
            ResourceType = resourceType,
        };
    }

    private static WebhookHandleResult<PatreonWebhookEvent> Failure(
        int statusCode, bool isAuthenticated, bool isKnownEvent, string reason) =>
        new()
        {
            Response = WebhookResponse.Empty(statusCode),
            IsAuthenticated = isAuthenticated,
            IsKnownEvent = isKnownEvent,
            Event = null,
            FailureReason = reason,
        };
}
