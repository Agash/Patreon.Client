using System.Text.Json;
using Patreon.Client.JsonApi;
using Patreon.Client.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Patreon.Client.Tests.JsonApi;

[TestClass]
public sealed class JsonApiDocumentTests
{
    private static readonly JsonSerializerOptions s_options = new(JsonSerializerDefaults.Web);

    [TestMethod]
    public void DeserializeJsonApiDocumentWithMemberAttributes()
    {
        const string json = """
            {
              "data": {
                "id": "mem-1",
                "type": "member",
                "attributes": {
                  "full_name": "Alice",
                  "patron_status": "active_patron",
                  "currently_entitled_amount_cents": 300,
                  "campaign_lifetime_support_cents": 900,
                  "will_pay_amount_cents": 300
                }
              },
              "meta": {
                "pagination": {
                  "total": 1,
                  "cursors": { "next": null }
                }
              }
            }
            """;

        JsonApiDocument<MemberAttributes>? doc =
            JsonSerializer.Deserialize<JsonApiDocument<MemberAttributes>>(json, s_options);

        Assert.IsNotNull(doc);
        Assert.IsNotNull(doc.Data);
        Assert.AreEqual("mem-1", doc.Data.Id);
        Assert.AreEqual("member", doc.Data.Type);
        Assert.IsNotNull(doc.Data.Attributes);
        Assert.AreEqual("Alice", doc.Data.Attributes.FullName);
        Assert.AreEqual("active_patron", doc.Data.Attributes.PatronStatus);
        Assert.AreEqual(300, doc.Data.Attributes.CurrentlyEntitledAmountCents);
        Assert.AreEqual(900, doc.Data.Attributes.CampaignLifetimeSupportCents);
    }

    [TestMethod]
    public void DeserializeJsonApiCollectionDocumentWithCampaignAttributes()
    {
        const string json = """
            {
              "data": [
                {
                  "id": "camp-99",
                  "type": "campaign",
                  "attributes": {
                    "name": "My Art Campaign",
                    "patron_count": 42,
                    "currency": "USD",
                    "is_monthly": true,
                    "is_nsfw": false
                  }
                }
              ],
              "meta": {
                "pagination": {
                  "total": 1,
                  "cursors": { "next": null }
                }
              }
            }
            """;

        JsonApiCollectionDocument<CampaignAttributes>? doc =
            JsonSerializer.Deserialize<JsonApiCollectionDocument<CampaignAttributes>>(json, s_options);

        Assert.IsNotNull(doc);
        Assert.IsNotNull(doc.Data);
        Assert.HasCount(1, doc.Data);

        JsonApiResource<CampaignAttributes> resource = doc.Data[0];
        Assert.AreEqual("camp-99", resource.Id);
        Assert.AreEqual("campaign", resource.Type);
        Assert.IsNotNull(resource.Attributes);
        Assert.AreEqual("My Art Campaign", resource.Attributes.Name);
        Assert.AreEqual(42, resource.Attributes.PatronCount);
        Assert.AreEqual("USD", resource.Attributes.Currency);
        Assert.IsTrue(resource.Attributes.IsMonthly);
    }

    [TestMethod]
    public void DeserializeJsonApiDocumentWithPaginationCursor()
    {
        const string json = """
            {
              "data": [],
              "meta": {
                "pagination": {
                  "total": 200,
                  "cursors": { "next": "cursor-abc123" }
                }
              }
            }
            """;

        JsonApiCollectionDocument<MemberAttributes>? doc =
            JsonSerializer.Deserialize<JsonApiCollectionDocument<MemberAttributes>>(json, s_options);

        Assert.IsNotNull(doc);
        Assert.IsNotNull(doc.Meta);
        Assert.IsNotNull(doc.Meta.Pagination);
        Assert.AreEqual(200, doc.Meta.Pagination.Total);
        Assert.IsNotNull(doc.Meta.Pagination.Cursors);
        Assert.AreEqual("cursor-abc123", doc.Meta.Pagination.Cursors.Next);
    }

    [TestMethod]
    public void DeserializeJsonApiDocumentWithErrors()
    {
        const string json = """
            {
              "errors": [
                {
                  "code": "1",
                  "code_name": "UnauthorizedError",
                  "detail": "You do not have permission to view this.",
                  "status": "401",
                  "title": "Unauthorized"
                }
              ]
            }
            """;

        JsonApiDocument<MemberAttributes>? doc =
            JsonSerializer.Deserialize<JsonApiDocument<MemberAttributes>>(json, s_options);

        Assert.IsNotNull(doc);
        Assert.IsNull(doc.Data);
        Assert.IsNotNull(doc.Errors);
        Assert.HasCount(1, doc.Errors);
        Assert.AreEqual("UnauthorizedError", doc.Errors[0].CodeName);
        Assert.AreEqual("401", doc.Errors[0].Status);
    }
}
