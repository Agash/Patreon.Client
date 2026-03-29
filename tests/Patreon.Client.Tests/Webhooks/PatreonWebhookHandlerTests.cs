using System.Security.Cryptography;
using System.Text;
using Agash.Webhook.Abstractions;
using Patreon.Client.Events;
using Patreon.Client.Webhooks;
using Xunit;

namespace Patreon.Client.Tests.Webhooks;

public sealed class PatreonWebhookHandlerTests
{
    private const string Secret = "test-webhook-secret";

    private readonly PatreonWebhookSignatureVerifier _verifier = new();
    private readonly PatreonWebhookHandler _handler;

    public PatreonWebhookHandlerTests()
    {
        _handler = new PatreonWebhookHandler(_verifier);
    }

    private static string Sign(byte[] body, string secret)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(secret);
#pragma warning disable CA5351
        using HMACMD5 hmac = new(keyBytes);
#pragma warning restore CA5351
        byte[] hash = hmac.ComputeHash(body);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static WebhookRequest BuildRequest(
        string body,
        string secret,
        string eventType = "members:create",
        string method = "POST",
        string contentType = "application/json")
    {
        byte[] bodyBytes = Encoding.UTF8.GetBytes(body);
        string sig = Sign(bodyBytes, secret);

        return new WebhookRequest
        {
            Method = method,
            Path = "/webhooks/patreon",
            ContentType = contentType,
            Body = bodyBytes,
            Headers = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                [PatreonWebhookSignatureVerifier.SignatureHeaderName] = [sig],
                [PatreonWebhookSignatureVerifier.EventHeaderName] = [eventType],
                ["Content-Type"] = [contentType],
            },
        };
    }

    private static PatreonWebhookOptions Options => new() { WebhookSecret = Secret };

    [Fact]
    public async Task HandleAsyncValidMemberCreateEventReturnsMemberWebhookEvent()
    {
        const string body = """
            {
              "data": {
                "id": "member-abc-123",
                "type": "member",
                "attributes": {
                  "full_name": "Jane Patron",
                  "email": "jane@example.com",
                  "patron_status": "active_patron",
                  "currently_entitled_amount_cents": 500,
                  "campaign_lifetime_support_cents": 1500,
                  "last_charge_status": "Paid",
                  "will_pay_amount_cents": 500
                }
              }
            }
            """;

        WebhookHandleResult<PatreonWebhookEvent> result =
            await _handler.HandleAsync(BuildRequest(body, Secret, "members:create"), Options);

        Assert.True(result.IsAuthenticated);
        Assert.True(result.IsKnownEvent);
        Assert.Equal(200, result.Response.StatusCode);

        PatreonMemberWebhookEvent member = Assert.IsType<PatreonMemberWebhookEvent>(result.Event);
        Assert.Equal("members:create", member.EventType);
        Assert.Equal("member-abc-123", member.ResourceId);
        Assert.Equal("member", member.ResourceType);
        Assert.NotNull(member.Attributes);
        Assert.Equal("Jane Patron", member.Attributes.FullName);
        Assert.Equal("active_patron", member.Attributes.PatronStatus);
        Assert.Equal(500, member.Attributes.CurrentlyEntitledAmountCents);
    }

    [Fact]
    public async Task HandleAsyncValidPledgeUpdateEventReturnsPledgeWebhookEvent()
    {
        const string body = """
            {
              "data": {
                "id": "member-pledge-456",
                "type": "member",
                "attributes": {
                  "full_name": "Bob Supporter",
                  "patron_status": "active_patron",
                  "currently_entitled_amount_cents": 1000,
                  "will_pay_amount_cents": 1000
                }
              }
            }
            """;

        WebhookHandleResult<PatreonWebhookEvent> result =
            await _handler.HandleAsync(BuildRequest(body, Secret, "members:pledge:update"), Options);

        Assert.True(result.IsAuthenticated);
        Assert.True(result.IsKnownEvent);
        Assert.Equal(200, result.Response.StatusCode);

        PatreonPledgeWebhookEvent pledge = Assert.IsType<PatreonPledgeWebhookEvent>(result.Event);
        Assert.Equal("members:pledge:update", pledge.EventType);
        Assert.Equal("member-pledge-456", pledge.ResourceId);
        Assert.NotNull(pledge.Attributes);
        Assert.Equal("Bob Supporter", pledge.Attributes.FullName);
    }

    [Fact]
    public async Task HandleAsyncValidPostPublishEventReturnsPostWebhookEvent()
    {
        const string body = """
            {
              "data": {
                "id": "post-789",
                "type": "post",
                "attributes": {
                  "title": "New exclusive post!",
                  "content": "Thanks for supporting me.",
                  "url": "https://www.patreon.com/posts/post-789",
                  "is_public": false
                }
              }
            }
            """;

        WebhookHandleResult<PatreonWebhookEvent> result =
            await _handler.HandleAsync(BuildRequest(body, Secret, "posts:publish"), Options);

        Assert.True(result.IsAuthenticated);
        Assert.True(result.IsKnownEvent);
        Assert.Equal(200, result.Response.StatusCode);

        PatreonPostWebhookEvent post = Assert.IsType<PatreonPostWebhookEvent>(result.Event);
        Assert.Equal("posts:publish", post.EventType);
        Assert.Equal("post-789", post.ResourceId);
        Assert.Equal("post", post.ResourceType);
        Assert.NotNull(post.Attributes);
        Assert.Equal("New exclusive post!", post.Attributes.Title);
        Assert.False(post.Attributes.IsPublic);
    }

    [Fact]
    public async Task HandleAsyncUnknownEventTypeReturnsUnknownWebhookEvent()
    {
        const string body = """{"data":{"id":"res-1","type":"something","attributes":{}}}""";

        WebhookHandleResult<PatreonWebhookEvent> result =
            await _handler.HandleAsync(BuildRequest(body, Secret, "some:future:event"), Options);

        Assert.True(result.IsAuthenticated);
        Assert.False(result.IsKnownEvent);
        Assert.Equal(200, result.Response.StatusCode);

        PatreonUnknownWebhookEvent unknown = Assert.IsType<PatreonUnknownWebhookEvent>(result.Event);
        Assert.Equal("some:future:event", unknown.EventType);
    }

    [Fact]
    public async Task HandleAsyncInvalidSignatureReturns401Unauthenticated()
    {
        const string body = """{"data":{"id":"x","type":"member","attributes":{}}}""";
        byte[] bodyBytes = Encoding.UTF8.GetBytes(body);

        WebhookRequest request = new()
        {
            Method = "POST",
            Path = "/webhooks/patreon",
            ContentType = "application/json",
            Body = bodyBytes,
            Headers = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                [PatreonWebhookSignatureVerifier.SignatureHeaderName] = ["badbadbadbad"],
                [PatreonWebhookSignatureVerifier.EventHeaderName] = ["members:create"],
                ["Content-Type"] = ["application/json"],
            },
        };

        WebhookHandleResult<PatreonWebhookEvent> result =
            await _handler.HandleAsync(request, Options);

        Assert.Equal(401, result.Response.StatusCode);
        Assert.False(result.IsAuthenticated);
        Assert.Null(result.Event);
    }

    [Fact]
    public async Task HandleAsyncNonPostMethodReturns405()
    {
        const string body = """{"data":{"id":"x","type":"member","attributes":{}}}""";

        WebhookHandleResult<PatreonWebhookEvent> result =
            await _handler.HandleAsync(BuildRequest(body, Secret, method: "GET"), Options);

        Assert.Equal(405, result.Response.StatusCode);
        Assert.False(result.IsAuthenticated);
        Assert.Null(result.Event);
    }

    [Fact]
    public async Task HandleAsyncNonJsonContentTypeReturns400()
    {
        const string body = "not-json";
        byte[] bodyBytes = Encoding.UTF8.GetBytes(body);
        string sig = Sign(bodyBytes, Secret);

        WebhookRequest request = new()
        {
            Method = "POST",
            Path = "/webhooks/patreon",
            ContentType = "text/plain",
            Body = bodyBytes,
            Headers = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                [PatreonWebhookSignatureVerifier.SignatureHeaderName] = [sig],
                [PatreonWebhookSignatureVerifier.EventHeaderName] = ["members:create"],
                ["Content-Type"] = ["text/plain"],
            },
        };

        WebhookHandleResult<PatreonWebhookEvent> result =
            await _handler.HandleAsync(request, Options);

        Assert.Equal(400, result.Response.StatusCode);
        Assert.False(result.IsAuthenticated);
        Assert.Null(result.Event);
    }

    [Fact]
    public async Task HandleAsyncMalformedJsonReturns400AfterAuth()
    {
        const string badBody = "{ this is not valid json !!!";
        byte[] bodyBytes = Encoding.UTF8.GetBytes(badBody);
        string sig = Sign(bodyBytes, Secret);

        WebhookRequest request = new()
        {
            Method = "POST",
            Path = "/webhooks/patreon",
            ContentType = "application/json",
            Body = bodyBytes,
            Headers = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                [PatreonWebhookSignatureVerifier.SignatureHeaderName] = [sig],
                [PatreonWebhookSignatureVerifier.EventHeaderName] = ["members:create"],
                ["Content-Type"] = ["application/json"],
            },
        };

        WebhookHandleResult<PatreonWebhookEvent> result =
            await _handler.HandleAsync(request, Options);

        Assert.Equal(400, result.Response.StatusCode);
        Assert.True(result.IsAuthenticated);
        Assert.Null(result.Event);
        Assert.NotNull(result.FailureReason);
    }
}
