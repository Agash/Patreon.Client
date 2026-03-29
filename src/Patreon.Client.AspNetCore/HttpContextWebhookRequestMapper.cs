using Agash.Webhook.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Patreon.Client.AspNetCore;

/// <summary>
/// Converts ASP.NET Core <see cref="HttpContext"/> instances into transport-neutral
/// <see cref="WebhookRequest"/> objects.
/// </summary>
public static class HttpContextWebhookRequestMapper
{
    /// <summary>
    /// Creates a <see cref="WebhookRequest"/> from the specified <see cref="HttpContext"/>.
    /// The request body is buffered so that it can be read multiple times.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns>A transport-neutral <see cref="WebhookRequest"/> representation of the request.</returns>
    public static async Task<WebhookRequest> FromHttpContextAsync(
        HttpContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        Dictionary<string, string[]> headers = new(StringComparer.OrdinalIgnoreCase);

        foreach ((string key, StringValues value) in context.Request.Headers)
        {
            headers[key] = [.. value.Select(static x => x ?? string.Empty)];
        }

        if (context.Request.Body.CanSeek)
        {
            context.Request.Body.Position = 0;
        }

        byte[] body;
        using (MemoryStream buffer = new())
        {
            await context.Request.Body.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);
            body = buffer.ToArray();
        }

        if (context.Request.Body.CanSeek)
        {
            context.Request.Body.Position = 0;
        }

        return new WebhookRequest
        {
            Method = context.Request.Method,
            Path = context.Request.Path.HasValue ? context.Request.Path.Value! : "/",
            QueryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : null,
            ContentType = context.Request.ContentType,
            Headers = headers,
            Body = body,
        };
    }
}
