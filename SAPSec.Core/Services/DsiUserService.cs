using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SAPSec.Core.Interfaces.Services;
using System.Text.Json;
using SAPSec.Core.Model;

namespace SAPSec.Core.Services;

public class DsiUserService(
    IDsiApiService dsiApiService,
    IHttpContextAccessor httpContextAccessor,
    ILogger<DsiUserService> logger) : IDsiUserService
{
    private readonly IDsiApiService _dsiApiService = dsiApiService ?? throw new ArgumentNullException(nameof(dsiApiService));
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    private readonly ILogger<DsiUserService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private const string OrganisationSessionKey = "CurrentOrganisationId";

    public async Task<DsiUser?> GetUserFromClaimsAsync(ClaimsPrincipal principal)
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

            // Get organisation data from claims
            var organisationClaim = principal.FindFirst("organisation")?.Value;
            var organisations = new List<DsiOrganisation>();

            if (!string.IsNullOrEmpty(organisationClaim))
            {
                try
                {
                    var orgData = JsonSerializer.Deserialize<DsiOrganisation[]>(organisationClaim);
                    if (orgData != null)
                    {
                        organisations.AddRange(orgData);
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize organisation claim for user {UserId}", userId);
                }
            }

            // If no organisations in claims, fetch from API
            if (!organisations.Any() && !string.IsNullOrEmpty(userId))
            {
                var userInfo = await _dsiApiService.GetUserAsync(userId);
                if (userInfo != null)
                {
                    organisations = userInfo.Organisations;
                }
            }

            return new DsiUser
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

    public async Task<DsiOrganisation?> GetCurrentOrganisationAsync(ClaimsPrincipal principal)
    {
        var user = await GetUserFromClaimsAsync(principal);
        if (user == null || !user.Organisations.Any())
        {
            return null;
        }

        // Check if there's a selected organisation in session
        var httpContext = _httpContextAccessor.HttpContext;
        string? selectedOrgId = null;
        if (httpContext?.Session != null)
        {
            // Fix: ISession does not have GetString, so use TryGetValue and convert to string
            if (httpContext.Session.TryGetValue(OrganisationSessionKey, out var value) && value != null)
            {
                selectedOrgId = System.Text.Encoding.UTF8.GetString(value);
            }
        }

        if (!string.IsNullOrEmpty(selectedOrgId))
        {
            var selectedOrg = user.Organisations.FirstOrDefault(o => o.Id == selectedOrgId);
            if (selectedOrg != null)
            {
                return selectedOrg;
            }
        }

        // Return the first organisation if none selected
        return user.Organisations.First();
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

            httpContext.Session.Set(OrganisationSessionKey, System.Text.Encoding.UTF8.GetBytes(organisationId));
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