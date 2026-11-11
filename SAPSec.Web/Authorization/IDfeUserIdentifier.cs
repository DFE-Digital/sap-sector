using System.Security.Claims;

namespace SAPSec.Web.Authorization;

/// <summary>
/// Works out whether the signed-in user is acting as DfE staff.
/// </summary>
public interface IDfeUserIdentifier
{
    Task<bool> IsDfeUserAsync(ClaimsPrincipal principal);
}
