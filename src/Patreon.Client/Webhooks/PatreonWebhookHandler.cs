using System.Text.Json;
using Agash.Webhook.Abstractions;
using Patreon.Client.Events;
using Patreon.Client.Models;

namespace Patreon.Client.Webhooks;

/// <summary>
/// Transport-neutral Patreon webhook handler. Verifies the HMAC-MD5 signature
/// and deserializes the payload into a typed <see cref="PatreonWebhookEvent"/>.
/// </summary>
public sealed class PatreonWebhookHandler
{
    private readonly PatreonWebhookSignatureVerifier _verifier;

    /// <summary>
    /// Initializes a new instance of <see cref="PatreonWebhookHandler"/>.
    /// </summary>
    /// <param name="verifier">The signature verifier to use.</param>
    public PatreonWebhookHandler(PatreonWebhookSignatureVerifier verifier)
    {
        _verifier = verifier ?? throw new ArgumentNullException(nameof(verifier));
    }

    /// <summary>
    /// Handles an incoming Patreon webhook request.
    /// </summary>
    /// <param name="request">The transport-neutral webhook request.</param>
    /// <param name="options">The webhook options containing the secret.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns>
    /// A <see cref="WebhookHandleResult{T}"/> describing the outcome of handling the request.
    /// </returns>
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
        {
            return Task.FromResult(Failure(405, false, false, "Patreon webhooks must use POST."));
        }

        if (!request.HasContentType("application/json"))
        {
            return Task.FromResult(Failure(400, false, false, "Expected application/json content type."));
        }

        byte[] body = request.Body ?? [];

        if (!_verifier.Verify(body, request.Headers, options.WebhookSecret))
        {
            return Task.FromResult(Failure(401, false, false,
                $"Invalid or missing Patreon webhook signature (header: {PatreonWebhookSignatureVerifier.SignatureHeaderName})."));
        }

        string eventType = "unknown";
        if (request.Headers.TryGetValue(PatreonWebhookSignatureVerifier.EventHeaderName, out string[]? evtValues)
            && evtValues.Length > 0
            && !string.IsNullOrWhiteSpace(evtValues[0]))
        {
            eventType = evtValues[0];
        }

        try
        {
            using JsonDocument doc = JsonDocument.Parse(body);
            JsonElement root = doc.RootElement;

            string resourceId = string.Empty;
            string resourceType = string.Empty;

            if (root.TryGetProperty("data", out JsonElement dataElem))
            {
                if (dataElem.TryGetProperty("id", out JsonElement idElem))
                {
                    resourceId = idElem.GetString() ?? string.Empty;
                }

                if (dataElem.TryGetProperty("type", out JsonElement typeElem))
                {
                    resourceType = typeElem.GetString() ?? string.Empty;
                }
            }

            PatreonWebhookEvent evt = eventType switch
            {
                "members:create" or "members:update" or "members:delete" =>
                    new PatreonMemberWebhookEvent
                    {
                        EventType = eventType,
                        ResourceId = resourceId,
                        ResourceType = string.IsNullOrEmpty(resourceType) ? "member" : resourceType,
                        Attributes = ParseAttributes<MemberAttributes>(root),
                    },

                "members:pledge:create" or "members:pledge:update" or "members:pledge:delete" =>
                    new PatreonPledgeWebhookEvent
                    {
                        EventType = eventType,
                        ResourceId = resourceId,
                        ResourceType = string.IsNullOrEmpty(resourceType) ? "member" : resourceType,
                        Attributes = ParseAttributes<MemberAttributes>(root),
                    },

                "posts:publish" or "posts:update" or "posts:delete" =>
                    new PatreonPostWebhookEvent
                    {
                        EventType = eventType,
                        ResourceId = resourceId,
                        ResourceType = string.IsNullOrEmpty(resourceType) ? "post" : resourceType,
                        Attributes = ParseAttributes<PostAttributes>(root),
                    },

                _ => new PatreonUnknownWebhookEvent
                {
                    EventType = eventType,
                    ResourceId = resourceId,
                    ResourceType = resourceType,
                },
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

    private static T? ParseAttributes<T>(JsonElement root)
    {
        if (root.TryGetProperty("data", out JsonElement data)
            && data.TryGetProperty("attributes", out JsonElement attrs))
        {
            try
            {
                return attrs.Deserialize<T>(PatreonJsonContext.WebOptions);
            }
            catch (JsonException)
            {
                return default;
            }
        }

        return default;
    }

    private static WebhookHandleResult<PatreonWebhookEvent> Failure(
        int statusCode,
        bool isAuthenticated,
        bool isKnownEvent,
        string reason)
    {
        return new WebhookHandleResult<PatreonWebhookEvent>
        {
            Response = WebhookResponse.Empty(statusCode),
            IsAuthenticated = isAuthenticated,
            IsKnownEvent = isKnownEvent,
            Event = null,
            FailureReason = reason,
        };
    }
}
