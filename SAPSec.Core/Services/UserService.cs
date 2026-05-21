using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Core.Services.Helper;
using System.Security.Claims;
using System.Text;

namespace SAPSec.Core.Services;

public class UserService(
    IDsiClient dsiApiService,
    IHttpContextAccessor httpContextAccessor,
    ILogger<UserService> logger) : IUserService
{
    private readonly IDsiClient _dsiApiService = dsiApiService ?? throw new ArgumentNullException(nameof(dsiApiService));
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    private readonly ILogger<UserService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private const string OrganisationSessionKey = "CurrentOrganisationId";

    public async Task<User?> GetUserFromClaimsAsync(ClaimsPrincipal principal)
    {
        if (principal == null || !principal.Identity?.IsAuthenticated == true)
        {
            return null;
        }

        try
        {
            var userId = GetUserId(principal);
            var email = GetUserEmail(principal);
            var givenName = principal.FindFirst(ClaimTypes.GivenName)?.Value ?? string.Empty;
            var familyName = principal.FindFirst(ClaimTypes.Surname)?.Value ?? string.Empty;
            var name = principal.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

            var hasOrganisationClaim = principal.HasClaim(c => c.Type == "organisation");
            var organisationClaim = principal.FindFirst("organisation")?.Value;
            _logger.LogInformation(
                "Organisation claim raw value for user {UserId}: {OrganisationClaim}",
                userId,
                organisationClaim);
            var organisations = organisationClaim.DeserializeToList<Organisation>(logger: _logger);

            _logger.LogInformation(
                "Resolved organisations from claims for user {UserId}. HasOrganisationClaim: {HasOrganisationClaim}. ParsedOrganisationCount: {OrganisationCount}",
                userId,
                hasOrganisationClaim,
                organisations.Count);

            if (!organisations.Any() && !string.IsNullOrEmpty(userId))
            {
                try
                {
                    _logger.LogInformation("No organisations in claims, fetching from API for user {UserId}", userId);
                    var userInfo = await _dsiApiService.GetUserAsync(userId);
                    if (userInfo != null && userInfo.Organisations != null)
                    {
                        organisations = userInfo.Organisations;
                        _logger.LogInformation(
                            "Resolved organisations from DSI API for user {UserId}. ApiOrganisationCount: {OrganisationCount}",
                            userId,
                            organisations.Count);
                    }
                    else
                    {
                        _logger.LogWarning("DSI API returned no organisations for user {UserId}", userId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch organisations from API for user {UserId}, using claims only", userId);
                }
            }

            return new User
            {
                Sub = userId ?? string.Empty,
                Email = email ?? string.Empty,
                GivenName = givenName,
                FamilyName = familyName,
                Name = name,
                Organisations = organisations
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user from claims");
            return null;
        }
    }

    public async Task<Organisation?> GetCurrentOrganisationAsync(ClaimsPrincipal principal)
    {
        var user = await GetUserFromClaimsAsync(principal);
        if (user == null || !user.Organisations.Any())
        {
            _logger.LogWarning(
                "Unable to resolve current organisation for user {UserId}. UserResolved: {UserResolved}. OrganisationCount: {OrganisationCount}",
                GetUserId(principal),
                user != null,
                user?.Organisations.Count ?? 0);
            return null;
        }

        var httpContext = _httpContextAccessor.HttpContext;
        string? selectedOrgId = null;
        if (httpContext?.Session != null)
        {
            if (httpContext.Session.TryGetValue(OrganisationSessionKey, out var value) && value != null)
            {
                selectedOrgId = Encoding.UTF8.GetString(value);
            }
        }

        _logger.LogInformation(
            "Resolving current organisation for user {UserId}. SessionOrganisationIdPresent: {HasSessionOrganisationId}. OrganisationCount: {OrganisationCount}",
            GetUserId(principal),
            !string.IsNullOrEmpty(selectedOrgId),
            user.Organisations.Count);

        if (!string.IsNullOrEmpty(selectedOrgId))
        {
            var selectedOrg = user.Organisations.FirstOrDefault(o => o.Id == selectedOrgId);
            if (selectedOrg != null)
            {
                _logger.LogInformation(
                    "Resolved current organisation from session for user {UserId}. OrganisationId: {OrganisationId}. Category: {OrganisationCategory}. Urn: {OrganisationUrn}",
                    GetUserId(principal),
                    selectedOrg.Id,
                    selectedOrg.Category?.Name,
                    selectedOrg.Urn);
                return selectedOrg;
            }

            _logger.LogWarning(
                "Session organisation id {OrganisationId} was not found in resolved organisations for user {UserId}",
                selectedOrgId,
                GetUserId(principal));
        }

        var fallbackOrganisation = user.Organisations.First();
        _logger.LogInformation(
            "Falling back to first organisation for user {UserId}. OrganisationId: {OrganisationId}. Category: {OrganisationCategory}. Urn: {OrganisationUrn}",
            GetUserId(principal),
            fallbackOrganisation.Id,
            fallbackOrganisation.Category?.Name,
            fallbackOrganisation.Urn);

        return fallbackOrganisation;
    }

    public Task<bool> SetCurrentOrganisationAsync(ClaimsPrincipal principal, string organisationId)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return Task.FromResult(false);
            }

            httpContext.Session.Set(OrganisationSessionKey, Encoding.UTF8.GetBytes(organisationId));
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting current organisation {OrganisationId}", organisationId);
            return Task.FromResult(false);
        }
    }

    public string? GetUserId(ClaimsPrincipal principal)
    {
        return principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal?.FindFirst("sub")?.Value;
    }

    public string? GetUserEmail(ClaimsPrincipal principal)
    {
        return principal?.FindFirst(ClaimTypes.Email)?.Value
            ?? principal?.FindFirst("email")?.Value;
    }

    public bool IsAuthenticated(ClaimsPrincipal principal)
    {
        return principal?.Identity?.IsAuthenticated == true;
    }

    public bool HasRole(ClaimsPrincipal principal, string role)
    {
        return principal?.IsInRole(role) == true;
    }
}
