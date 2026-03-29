using Agash.Webhook.Abstractions;
using Microsoft.AspNetCore.Http;
using Patreon.Client.Events;
using Patreon.Client.Webhooks;

namespace Patreon.Client.AspNetCore;

/// <summary>
/// Options for a Patreon webhook endpoint registered via
/// <see cref="PatreonEndpointRouteBuilderExtensions"/>.
/// </summary>
public sealed class PatreonWebhookEndpointOptions
{
    /// <summary>
    /// Gets or sets the delegate that resolves the per-request <see cref="PatreonWebhookOptions"/>.
    /// </summary>
    public required Func<HttpContext, CancellationToken, Task<PatreonWebhookOptions>> ResolveWebhookOptionsAsync { get; set; }

    /// <summary>
    /// Gets or sets an optional callback invoked for each successfully parsed event.
    /// </summary>
    public Func<PatreonWebhookEvent, HttpContext, CancellationToken, Task>? OnEventAsync { get; set; }

    /// <summary>
    /// Gets or sets an optional callback invoked after every request with the full handle result.
    /// </summary>
    public Func<WebhookHandleResult<PatreonWebhookEvent>, HttpContext, CancellationToken, Task>? OnResultAsync { get; set; }
}
