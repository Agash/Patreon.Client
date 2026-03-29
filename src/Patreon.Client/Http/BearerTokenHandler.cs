using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Patreon.Client.Options;

namespace Patreon.Client.Http;

/// <summary>
/// A <see cref="DelegatingHandler"/> that injects a Bearer token authorization header
/// into every outbound Patreon API request.
/// </summary>
public sealed class BearerTokenHandler : DelegatingHandler
{
    private readonly IOptionsMonitor<PatreonClientOptions> _options;

    /// <summary>
    /// Initializes a new instance of <see cref="BearerTokenHandler"/>.
    /// </summary>
    /// <param name="options">The options monitor providing the current access token.</param>
    public BearerTokenHandler(IOptionsMonitor<PatreonClientOptions> options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        string token = _options.CurrentValue.AccessToken;

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
