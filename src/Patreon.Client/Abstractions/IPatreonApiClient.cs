using Patreon.Client.JsonApi;
using Patreon.Client.Models;

namespace Patreon.Client.Abstractions;

/// <summary>
/// Provides typed access to the Patreon API v2 REST endpoints.
/// </summary>
public interface IPatreonApiClient
{
    /// <summary>
    /// Returns information about the currently authenticated creator.
    /// Corresponds to <c>GET /identity</c>.
    /// </summary>
    /// <param name="fields">Optional set of attribute fields to include (e.g. <c>full_name,email</c>).</param>
    /// <param name="include">Optional related resources to side-load (e.g. <c>memberships</c>).</param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns>A <see cref="JsonApiDocument{T}"/> containing the user resource, or <see langword="null"/>.</returns>
    Task<JsonApiDocument<UserAttributes>?> GetIdentityAsync(
        IEnumerable<string>? fields = null,
        IEnumerable<string>? include = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all campaigns owned by the currently authenticated creator.
    /// Corresponds to <c>GET /campaigns</c>.
    /// </summary>
    /// <param name="fields">Optional set of attribute fields to include.</param>
    /// <param name="include">Optional related resources to side-load.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns>A <see cref="JsonApiCollectionDocument{T}"/> containing campaign resources, or <see langword="null"/>.</returns>
    Task<JsonApiCollectionDocument<CampaignAttributes>?> GetCampaignsAsync(
        IEnumerable<string>? fields = null,
        IEnumerable<string>? include = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a single campaign by ID.
    /// Corresponds to <c>GET /campaigns/{campaignId}</c>.
    /// </summary>
    /// <param name="campaignId">The Patreon campaign ID.</param>
    /// <param name="fields">Optional set of attribute fields to include.</param>
    /// <param name="include">Optional related resources to side-load.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns>A <see cref="JsonApiDocument{T}"/> containing the campaign resource, or <see langword="null"/>.</returns>
    Task<JsonApiDocument<CampaignAttributes>?> GetCampaignAsync(
        string campaignId,
        IEnumerable<string>? fields = null,
        IEnumerable<string>? include = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all members of a campaign, transparently paging through cursor-based pagination.
    /// Corresponds to <c>GET /campaigns/{campaignId}/members</c>.
    /// </summary>
    /// <param name="campaignId">The Patreon campaign ID.</param>
    /// <param name="fields">Optional set of attribute fields to include.</param>
    /// <param name="include">Optional related resources to side-load.</param>
    /// <param name="pageSize">The number of results per page (default: 20, max: 1000).</param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns>An async sequence of member resources.</returns>
    IAsyncEnumerable<JsonApiResource<MemberAttributes>> GetCampaignMembersAsync(
        string campaignId,
        IEnumerable<string>? fields = null,
        IEnumerable<string>? include = null,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a single member by ID.
    /// Corresponds to <c>GET /members/{memberId}</c>.
    /// </summary>
    /// <param name="memberId">The Patreon member ID.</param>
    /// <param name="fields">Optional set of attribute fields to include.</param>
    /// <param name="include">Optional related resources to side-load.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns>A <see cref="JsonApiDocument{T}"/> containing the member resource, or <see langword="null"/>.</returns>
    Task<JsonApiDocument<MemberAttributes>?> GetMemberAsync(
        string memberId,
        IEnumerable<string>? fields = null,
        IEnumerable<string>? include = null,
        CancellationToken cancellationToken = default);
}
