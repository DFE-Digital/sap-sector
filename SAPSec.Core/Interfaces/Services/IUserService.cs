using SAPSec.Core.Model;
using System.Security.Claims;

namespace SAPSec.Core.Interfaces.Services;

public interface IUserService
{
    Task<User> GetUserFromClaimsAsync(ClaimsPrincipal principal);
    Task<Organisation> GetCurrentOrganisationAsync(ClaimsPrincipal principal);
    Task<bool> SetCurrentOrganisationAsync(ClaimsPrincipal principal, string organisationId);
    string? GetUserId(ClaimsPrincipal principal);
    string? GetUserEmail(ClaimsPrincipal principal);
    bool IsAuthenticated(ClaimsPrincipal principal);
    bool HasRole(ClaimsPrincipal principal, string role);
}