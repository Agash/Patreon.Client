using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Patreon.Client.Abstractions;
using Patreon.Client.Http;
using Patreon.Client.Options;
using Patreon.Client.Webhooks;

namespace Patreon.Client.DependencyInjection;

/// <summary>
/// Provides dependency injection registration helpers for Patreon.Client.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Patreon client services to the specified service collection using the provided options.
    /// </summary>
    /// <param name="services">The service collection to modify.</param>
    /// <param name="configure">A delegate that configures the <see cref="PatreonClientOptions"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddPatreonClient(
        this IServiceCollection services,
        Action<PatreonClientOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.Configure(configure);
        return services.AddPatreonClientCore();
    }

    /// <summary>
    /// Adds Patreon client services to the specified service collection.
    /// <see cref="PatreonClientOptions"/> must already be registered in the container.
    /// </summary>
    /// <param name="services">The service collection to modify.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddPatreonClient(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddPatreonClientCore();
    }

    private static IServiceCollection AddPatreonClientCore(this IServiceCollection services)
    {
        services.AddOptions<PatreonClientOptions>();

        services.TryAddTransient<BearerTokenHandler>();

        services
            .AddHttpClient<IPatreonApiClient, PatreonApiClient>(static (sp, client) =>
            {
                client.BaseAddress = new Uri("https://www.patreon.com/api/oauth2/v2/");
                client.DefaultRequestHeaders.UserAgent.ParseAdd(
                    "Patreon.Client/1.0 (https://github.com/Agash/Patreon.Client)");
            })
            .AddHttpMessageHandler<BearerTokenHandler>();

        services.TryAddSingleton<PatreonWebhookSignatureVerifier>();
        services.TryAddSingleton<PatreonWebhookHandler>();

        return services;
    }
}
