using SAPSec.Core.Model.DsiUser;
using System.Security.Claims;

namespace SAPSec.Core.Interfaces.Services;

public interface IDsiUserService
{
    Task<DsiUser?> GetUserFromClaimsAsync(ClaimsPrincipal principal);
    Task<DsiOrganisation?> GetCurrentOrganisationAsync(ClaimsPrincipal principal);
    Task<bool> SetCurrentOrganisationAsync(ClaimsPrincipal principal, string organisationId);
    string? GetUserId(ClaimsPrincipal principal);
    string? GetUserEmail(ClaimsPrincipal principal);
    bool IsAuthenticated(ClaimsPrincipal principal);
    bool HasRole(ClaimsPrincipal principal, string role);
}