using Microsoft.Extensions.Options;
using SAPSec.Core.Authentication;
using System.Security.Claims;

namespace SAPSec.Web.Authorization;

/// <summary>
/// Recognises DfE staff by the organisation they signed in as, not by their email address: someone with a
/// DfE email who signs in for a school is treated as a school user.
/// </summary>
public class DfeUserIdentifier(
    IUserService userService,
    IOptions<DfeUserOptions> options) : IDfeUserIdentifier
{
    public async Task<bool> IsDfeUserAsync(ClaimsPrincipal principal)
    {
        if (!userService.IsAuthenticated(principal))
        {
            return false;
        }

        var organisationName = (await userService.GetCurrentOrganisationAsync(principal))?.Name?.Trim();
        if (string.IsNullOrEmpty(organisationName))
        {
            return false;
        }

        return options.Value.OrganisationNames
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Any(name => string.Equals(name.Trim(), organisationName, StringComparison.OrdinalIgnoreCase));
    }
}
