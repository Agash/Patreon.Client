# Patreon.Client

[![NuGet](https://img.shields.io/nuget/v/Patreon.Client.svg?include_prereleases)](https://www.nuget.org/packages/Patreon.Client/)
[![CI](https://github.com/Agash/Patreon.Client/actions/workflows/build.yml/badge.svg)](https://github.com/Agash/Patreon.Client/actions/workflows/build.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE.txt)

A modern .NET 10 client library for the **Patreon API v2** with full webhook support, JSON:API relationship resolution, and automatic webhook management.

## Features

- **Typed JSON:API deserialization** — `JsonApiDocument<T>` envelopes with cursor-paginated `IAsyncEnumerable` for member lists
- **Relationship resolution** — `JsonApiIncludedIndex` lets you resolve side-loaded `included` resources by `(type, id)` in O(1)
- **HMAC-MD5 webhook verification** — per Patreon's specification
- **All 9 webhook event types** — members create/update/delete, pledge create/update/delete, posts publish/update/delete
- **Webhook management API** — create, list, update, and delete webhooks programmatically (auto-registration)
- **REST endpoints** — identity, campaigns, members (paginated), posts, tiers, webhooks
- **`IHttpClientFactory`-backed** — no static `HttpClient`, proper lifetime management
- **`Microsoft.Extensions.*`-native** — `IOptions<T>`, `ILogger<T>`, DI extensions
- **.NET 10 / C# 14** — `System.Text.Json` source generation, no Newtonsoft.Json

## Packages

| Package | Description |
|---------|-------------|
| `Patreon.Client` | Core library — API client, webhook handler, models |
| `Patreon.Client.DependencyInjection` | `IServiceCollection` extensions |
| `Patreon.Client.AspNetCore` | `IEndpointRouteBuilder.MapPatreonWebhook()` minimal API extension |

## Quick Start — Webhook Receiver

```csharp
// Program.cs (ASP.NET Core minimal API)
using Patreon.Client.DependencyInjection;
using Patreon.Client.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddPatreonClient(options =>
{
    options.AccessToken = builder.Configuration["Patreon:AccessToken"]!;
});

var app = builder.Build();

app.MapPatreonWebhook("/webhooks/patreon", options =>
{
    options.ResolveWebhookOptionsAsync = (ctx, ct) => ValueTask.FromResult(
        new PatreonWebhookOptions { WebhookSecret = builder.Configuration["Patreon:WebhookSecret"]! });

    options.OnEventAsync = async (evt, ctx, ct) =>
    {
        if (evt is PatreonMemberWebhookEvent member)
        {
            Console.WriteLine($"Member {member.EventType}: {member.Attributes?.FullName}");

            // Access relationship data
            var index = new JsonApiIncludedIndex(member.Document?.Included);
            // Resolve entitled tiers from the relationships object...
        }
    };
});

app.Run();
```

## Relationship Resolution

Patreon uses the [JSON:API](https://jsonapi.org/) specification. Webhook payloads include a `data.relationships` object with references to related resources, and an `included` array with the full resource objects.

```csharp
// Resolve side-loaded tier from a member webhook event
PatreonMemberWebhookEvent evt = ...;
var index = new JsonApiIncludedIndex(evt.Document?.Included);

// Get IDs from relationships (raw JsonElement)
// evt.Document?.Data?.Relationships contains the JSON:API relationships object

// Resolve a tier by known ID
TierAttributes? tier = index.TryGetAttributesAs<TierAttributes>("tier", "tier-id-123");
if (tier is not null)
{
    Console.WriteLine($"Tier: {tier.Title} (${tier.AmountCents / 100.0:F2}/mo)");
}
```

## Webhook Management (Auto-Registration)

```csharp
// Create a webhook (secret only returned once — save it!)
var client = serviceProvider.GetRequiredService<IPatreonApiClient>();
var webhook = await client.CreateWebhookAsync(
    uri: "https://your-tunnel.devtunnels.ms/webhooks/patreon",
    triggers: ["members:create", "members:pledge:create", "members:pledge:update"]);

Console.WriteLine($"Webhook ID: {webhook?.Data?.Id}");
Console.WriteLine($"Signing secret: {webhook?.Data?.Attributes?.Secret}"); // Save this!

// List existing webhooks
var webhooks = await client.GetWebhooksAsync();

// Unpause after failures
await client.UpdateWebhookAsync(webhookId, paused: false);

// Delete
await client.DeleteWebhookAsync(webhookId);
```

## Supported Webhook Event Types

| Event | Type |
|-------|------|
| Member joined | `members:create` |
| Member updated | `members:update` |
| Member left | `members:delete` |
| Pledge created | `members:pledge:create` |
| Pledge updated | `members:pledge:update` |
| Pledge deleted | `members:pledge:delete` |
| Post published | `posts:publish` |
| Post updated | `posts:update` |
| Post deleted | `posts:delete` |

## REST API

```csharp
var client = serviceProvider.GetRequiredService<IPatreonApiClient>();

// Get authenticated user
var identity = await client.GetIdentityAsync(fields: ["full_name", "email"]);

// Get all campaigns
var campaigns = await client.GetCampaignsAsync(
    fields: ["name", "patron_count", "url"],
    include: ["tiers"]);

// Page through all members
await foreach (var member in client.GetCampaignMembersAsync(
    campaignId,
    fields: ["full_name", "patron_status", "currently_entitled_amount_cents"],
    include: ["currently_entitled_tiers", "user"],
    pageSize: 100))
{
    Console.WriteLine($"{member.Attributes?.FullName}: {member.Attributes?.PatronStatus}");
}

// Get a campaign's tiers
var tiers = await client.GetCampaignTiersAsync(campaignId,
    tierFields: ["title", "amount_cents", "patron_count"]);
```

## Webhook Signature Verification

Patreon signs webhook deliveries using **HMAC-MD5** (per Patreon's specification). The signature is in the `X-Patreon-Signature` header as a lowercase hex string. This library verifies it automatically using `System.Security.Cryptography.HMACMD5` with a timing-safe comparison.

## License

MIT
