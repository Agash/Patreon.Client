namespace Patreon.Client.Options;

/// <summary>
/// Configuration options for the Patreon API REST client.
/// </summary>
public sealed class PatreonClientOptions
{
    /// <summary>
    /// Gets or sets the OAuth 2.0 access token used to authenticate API requests.
    /// This is the creator's access token obtained through the Patreon OAuth flow.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;
}
