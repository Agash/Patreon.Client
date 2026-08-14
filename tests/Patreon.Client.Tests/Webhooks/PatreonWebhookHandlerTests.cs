using System.Security.Cryptography;
using System.Text;
using Agash.Webhook.Abstractions;
using Patreon.Client.Events;
using Patreon.Client.JsonApi;
using Patreon.Client.Models;
using Patreon.Client.Webhooks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Patreon.Client.Tests.Webhooks;

[TestClass]
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

    [TestMethod]
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

        Assert.IsTrue(result.IsAuthenticated);
        Assert.IsTrue(result.IsKnownEvent);
        Assert.AreEqual(200, result.Response.StatusCode);

        Assert.IsInstanceOfType<PatreonMemberWebhookEvent>(result.Event);
        var member = (PatreonMemberWebhookEvent)result.Event;
        Assert.AreEqual("members:create", member.EventType);
        Assert.AreEqual("member-abc-123", member.ResourceId);
        Assert.AreEqual("member", member.ResourceType);

        // Convenience property
        Assert.IsNotNull(member.Attributes);
        Assert.AreEqual("Jane Patron", member.Attributes.FullName);
        Assert.AreEqual("active_patron", member.Attributes.PatronStatus);
        Assert.AreEqual(500, member.Attributes.CurrentlyEntitledAmountCents);

        // Full document
        Assert.IsNotNull(member.Document);
        Assert.IsNotNull(member.Document.Data);
        Assert.AreEqual("member-abc-123", member.Document.Data.Id);
        Assert.AreEqual("member", member.Document.Data.Type);
    }

    [TestMethod]
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

        Assert.IsTrue(result.IsAuthenticated);
        Assert.IsTrue(result.IsKnownEvent);
        Assert.AreEqual(200, result.Response.StatusCode);

        Assert.IsInstanceOfType<PatreonPledgeWebhookEvent>(result.Event);
        var pledge = (PatreonPledgeWebhookEvent)result.Event;
        Assert.AreEqual("members:pledge:update", pledge.EventType);
        Assert.AreEqual("member-pledge-456", pledge.ResourceId);

        // Convenience property
        Assert.IsNotNull(pledge.Attributes);
        Assert.AreEqual("Bob Supporter", pledge.Attributes.FullName);

        // Full document
        Assert.IsNotNull(pledge.Document);
        Assert.IsNotNull(pledge.Document.Data);
        Assert.AreEqual("member-pledge-456", pledge.Document.Data.Id);
    }

    [TestMethod]
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

        Assert.IsTrue(result.IsAuthenticated);
        Assert.IsTrue(result.IsKnownEvent);
        Assert.AreEqual(200, result.Response.StatusCode);

        Assert.IsInstanceOfType<PatreonPostWebhookEvent>(result.Event);
        var post = (PatreonPostWebhookEvent)result.Event;
        Assert.AreEqual("posts:publish", post.EventType);
        Assert.AreEqual("post-789", post.ResourceId);
        Assert.AreEqual("post", post.ResourceType);

        // Convenience property
        Assert.IsNotNull(post.Attributes);
        Assert.AreEqual("New exclusive post!", post.Attributes.Title);
        Assert.IsFalse(post.Attributes.IsPublic);

        // Full document
        Assert.IsNotNull(post.Document);
        Assert.IsNotNull(post.Document.Data);
        Assert.AreEqual("post-789", post.Document.Data.Id);
        Assert.AreEqual("post", post.Document.Data.Type);
    }

    [TestMethod]
    public async Task HandleAsyncUnknownEventTypeReturnsUnknownWebhookEvent()
    {
        const string body = """{"data":{"id":"res-1","type":"something","attributes":{}}}""";

        WebhookHandleResult<PatreonWebhookEvent> result =
            await _handler.HandleAsync(BuildRequest(body, Secret, "some:future:event"), Options);

        Assert.IsTrue(result.IsAuthenticated);
        Assert.IsFalse(result.IsKnownEvent);
        Assert.AreEqual(200, result.Response.StatusCode);

        Assert.IsInstanceOfType<PatreonUnknownWebhookEvent>(result.Event);
        var unknown = (PatreonUnknownWebhookEvent)result.Event;
        Assert.AreEqual("some:future:event", unknown.EventType);
    }

    [TestMethod]
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

        Assert.AreEqual(401, result.Response.StatusCode);
        Assert.IsFalse(result.IsAuthenticated);
        Assert.IsNull(result.Event);
    }

    [TestMethod]
    public async Task HandleAsyncNonPostMethodReturns405()
    {
        const string body = """{"data":{"id":"x","type":"member","attributes":{}}}""";

        WebhookHandleResult<PatreonWebhookEvent> result =
            await _handler.HandleAsync(BuildRequest(body, Secret, method: "GET"), Options);

        Assert.AreEqual(405, result.Response.StatusCode);
        Assert.IsFalse(result.IsAuthenticated);
        Assert.IsNull(result.Event);
    }

    [TestMethod]
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

        Assert.AreEqual(400, result.Response.StatusCode);
        Assert.IsFalse(result.IsAuthenticated);
        Assert.IsNull(result.Event);
    }

    [TestMethod]
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

        Assert.AreEqual(400, result.Response.StatusCode);
        Assert.IsTrue(result.IsAuthenticated);
        Assert.IsNull(result.Event);
        Assert.IsNotNull(result.FailureReason);
    }

    [TestMethod]
    public async Task HandleAsyncMemberEventDocumentContainsFullResourceWithIncluded()
    {
        const string body = """
            {
              "data": {
                "id": "member-with-tier",
                "type": "member",
                "attributes": {
                  "full_name": "Alice",
                  "patron_status": "active_patron",
                  "currently_entitled_amount_cents": 1000,
                  "will_pay_amount_cents": 1000
                },
                "relationships": {
                  "currently_entitled_tiers": {
                    "data": [{ "type": "tier", "id": "tier-gold" }]
                  }
                }
              },
              "included": [
                {
                  "type": "tier",
                  "id": "tier-gold",
                  "attributes": {
                    "title": "Gold Tier",
                    "amount_cents": 1000,
                    "published": true
                  }
                }
              ]
            }
            """;

        WebhookHandleResult<PatreonWebhookEvent> result =
            await _handler.HandleAsync(BuildRequest(body, Secret, "members:create"), Options);

        Assert.IsTrue(result.IsAuthenticated);
        Assert.IsTrue(result.IsKnownEvent);

        Assert.IsInstanceOfType<PatreonMemberWebhookEvent>(result.Event);
        var member = (PatreonMemberWebhookEvent)result.Event;
        Assert.IsNotNull(member.Document);
        Assert.IsNotNull(member.Document.Data);
        Assert.AreEqual("member-with-tier", member.Document.Data.Id);

        // Convenience property still works
        Assert.AreEqual("Alice", member.Attributes?.FullName);

        // Included side-loading
        Assert.IsNotNull(member.Document.Included);
        Assert.HasCount(1, member.Document.Included);

        // Relationship resolution via JsonApiIncludedIndex
        JsonApiIncludedIndex index = new(member.Document.Included);
        TierAttributes? tier = index.TryGetAttributesAs<TierAttributes>("tier", "tier-gold");
        Assert.IsNotNull(tier);
        Assert.AreEqual("Gold Tier", tier.Title);
        Assert.AreEqual(1000, tier.AmountCents);

        // EntitledTierIds extracted at parse time
        Assert.HasCount(1, member.EntitledTierIds);
        Assert.AreEqual("tier-gold", member.EntitledTierIds[0]);
    }

    [TestMethod]
    public async Task HandleAsync_PledgeEvent_ExtractsEntitledTierIds()
    {
        string body = $$"""
            {
              "data": {
                "id": "member-pledge",
                "type": "member",
                "attributes": {
                  "full_name": "Bob",
                  "patron_status": "active_patron",
                  "currently_entitled_amount_cents": 2500,
                  "will_pay_amount_cents": 2500
                },
                "relationships": {
                  "currently_entitled_tiers": {
                    "data": [
                      { "type": "tier", "id": "tier-silver" },
                      { "type": "tier", "id": "tier-gold" }
                    ]
                  }
                }
              }
            }
            """;

        WebhookHandleResult<PatreonWebhookEvent> result =
            await _handler.HandleAsync(BuildRequest(body, Secret, "members:pledge:create"), Options);

        Assert.IsTrue(result.IsAuthenticated);
        Assert.IsTrue(result.IsKnownEvent);

        Assert.IsInstanceOfType<PatreonPledgeWebhookEvent>(result.Event);
        var pledge = (PatreonPledgeWebhookEvent)result.Event;
        Assert.AreEqual(2, pledge.EntitledTierIds.Count);
        Assert.Contains("tier-silver", pledge.EntitledTierIds);
        Assert.Contains("tier-gold", pledge.EntitledTierIds);
    }

    [TestMethod]
    public async Task HandleAsync_MemberEvent_NoTierRelationship_EntitledTierIdsIsEmpty()
    {
        string body = """
            {
              "data": {
                "id": "member-no-tiers",
                "type": "member",
                "attributes": {
                  "full_name": "Charlie",
                  "patron_status": "active_patron",
                  "currently_entitled_amount_cents": 0
                }
              }
            }
            """;

        WebhookHandleResult<PatreonWebhookEvent> result =
            await _handler.HandleAsync(BuildRequest(body, Secret, "members:update"), Options);

        Assert.IsTrue(result.IsKnownEvent);
        Assert.IsInstanceOfType<PatreonMemberWebhookEvent>(result.Event);
        var member = (PatreonMemberWebhookEvent)result.Event;
        Assert.IsEmpty(member.EntitledTierIds);
    }
}
