namespace Patreon.Client.Events;

/// <summary>
/// Raised when Patreon sends a webhook event with an unrecognised event type.
/// </summary>
public sealed record PatreonUnknownWebhookEvent : PatreonWebhookEvent;
