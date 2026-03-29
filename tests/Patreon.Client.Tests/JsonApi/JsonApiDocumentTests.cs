using System.Text.Json;
using Patreon.Client.JsonApi;
using Patreon.Client.Models;
using Xunit;

namespace Patreon.Client.Tests.JsonApi;

public sealed class JsonApiDocumentTests
{
    private static readonly JsonSerializerOptions s_options = new(JsonSerializerDefaults.Web);

    [Fact]
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

        Assert.NotNull(doc);
        Assert.NotNull(doc.Data);
        Assert.Equal("mem-1", doc.Data.Id);
        Assert.Equal("member", doc.Data.Type);
        Assert.NotNull(doc.Data.Attributes);
        Assert.Equal("Alice", doc.Data.Attributes.FullName);
        Assert.Equal("active_patron", doc.Data.Attributes.PatronStatus);
        Assert.Equal(300, doc.Data.Attributes.CurrentlyEntitledAmountCents);
        Assert.Equal(900, doc.Data.Attributes.CampaignLifetimeSupportCents);
    }

    [Fact]
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

        Assert.NotNull(doc);
        Assert.NotNull(doc.Data);
        Assert.Single(doc.Data);

        JsonApiResource<CampaignAttributes> resource = doc.Data[0];
        Assert.Equal("camp-99", resource.Id);
        Assert.Equal("campaign", resource.Type);
        Assert.NotNull(resource.Attributes);
        Assert.Equal("My Art Campaign", resource.Attributes.Name);
        Assert.Equal(42, resource.Attributes.PatronCount);
        Assert.Equal("USD", resource.Attributes.Currency);
        Assert.True(resource.Attributes.IsMonthly);
    }

    [Fact]
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

        Assert.NotNull(doc);
        Assert.NotNull(doc.Meta);
        Assert.NotNull(doc.Meta.Pagination);
        Assert.Equal(200, doc.Meta.Pagination.Total);
        Assert.NotNull(doc.Meta.Pagination.Cursors);
        Assert.Equal("cursor-abc123", doc.Meta.Pagination.Cursors.Next);
    }

    [Fact]
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

        Assert.NotNull(doc);
        Assert.Null(doc.Data);
        Assert.NotNull(doc.Errors);
        Assert.Single(doc.Errors);
        Assert.Equal("UnauthorizedError", doc.Errors[0].CodeName);
        Assert.Equal("401", doc.Errors[0].Status);
    }
}
