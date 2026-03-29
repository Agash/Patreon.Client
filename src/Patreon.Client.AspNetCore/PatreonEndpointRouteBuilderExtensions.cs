using Agash.Webhook.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Patreon.Client.Events;
using Patreon.Client.Webhooks;

namespace Patreon.Client.AspNetCore;

/// <summary>
/// Provides endpoint mapping extensions for exposing <see cref="PatreonWebhookHandler"/>
/// through ASP.NET Core minimal APIs.
/// </summary>
public static class PatreonEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps a Patreon webhook endpoint using the supplied endpoint options.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder to extend.</param>
    /// <param name="pattern">The route pattern to map.</param>
    /// <param name="configure">The callback used to configure endpoint options.</param>
    /// <returns>An endpoint convention builder for further configuration.</returns>
    public static IEndpointConventionBuilder MapPatreonWebhook(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        Action<PatreonWebhookEndpointOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentException.ThrowIfNullOrEmpty(pattern);
        ArgumentNullException.ThrowIfNull(configure);

        PatreonWebhookEndpointOptions options = new()
        {
            ResolveWebhookOptionsAsync = static (_, _) => Task.FromResult(new PatreonWebhookOptions
            {
                WebhookSecret = string.Empty,
            }),
        };

        configure(options);

        return endpoints.MapPost(pattern, async (HttpContext context) =>
        {
            PatreonWebhookHandler handler = context.RequestServices.GetRequiredService<PatreonWebhookHandler>();

            PatreonWebhookOptions webhookOptions =
                await options.ResolveWebhookOptionsAsync(context, context.RequestAborted).ConfigureAwait(false);

            WebhookRequest request =
                await HttpContextWebhookRequestMapper.FromHttpContextAsync(context, context.RequestAborted)
                    .ConfigureAwait(false);

            WebhookHandleResult<PatreonWebhookEvent> result =
                await handler.HandleAsync(request, webhookOptions, context.RequestAborted)
                    .ConfigureAwait(false);

            if (result.Event is PatreonWebhookEvent evt && options.OnEventAsync is not null)
            {
                await options.OnEventAsync(evt, context, context.RequestAborted).ConfigureAwait(false);
            }

            if (options.OnResultAsync is not null)
            {
                await options.OnResultAsync(result, context, context.RequestAborted).ConfigureAwait(false);
            }

            await WebhookResponseHttpContextWriter.WriteAsync(context, result.Response, context.RequestAborted)
                .ConfigureAwait(false);
        });
    }

    /// <summary>
    /// Maps a Patreon webhook endpoint using direct delegate overloads.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder to extend.</param>
    /// <param name="pattern">The route pattern to map.</param>
    /// <param name="resolveWebhookOptionsAsync">The callback used to resolve webhook options per request.</param>
    /// <param name="onEventAsync">An optional callback invoked for each successfully parsed event.</param>
    /// <param name="onResultAsync">An optional callback invoked after every request with the full handle result.</param>
    /// <returns>An endpoint convention builder for further configuration.</returns>
    public static IEndpointConventionBuilder MapPatreonWebhook(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        Func<HttpContext, CancellationToken, Task<PatreonWebhookOptions>> resolveWebhookOptionsAsync,
        Func<PatreonWebhookEvent, HttpContext, CancellationToken, Task>? onEventAsync = null,
        Func<WebhookHandleResult<PatreonWebhookEvent>, HttpContext, CancellationToken, Task>? onResultAsync = null)
    {
        ArgumentNullException.ThrowIfNull(resolveWebhookOptionsAsync);

        return endpoints.MapPatreonWebhook(
            pattern,
            options =>
            {
                options.ResolveWebhookOptionsAsync = resolveWebhookOptionsAsync;
                options.OnEventAsync = onEventAsync;
                options.OnResultAsync = onResultAsync;
            });
    }
}
