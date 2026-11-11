using Microsoft.Extensions.Options;
using SAPSec.Core.Authentication;
using System.Security.Claims;

namespace SAPSec.Web.Authorization;

/// <summary>
/// Recognises DfE staff by the domain of the email address they signed in with. This does not look at the
/// organisation they signed in as, so DfE staff count as DfE users even when signed in for a school.
/// </summary>
public class DfeUserIdentifier(
    IUserService userService,
    IOptions<DfeUserOptions> options) : IDfeUserIdentifier
{
    public Task<bool> IsDfeUserAsync(ClaimsPrincipal principal)
    {
        if (!userService.IsAuthenticated(principal))
        {
            return Task.FromResult(false);
        }

        var domain = EmailDomain(userService.GetUserEmail(principal));
        if (domain is null)
        {
            return Task.FromResult(false);
        }

        var isDfeUser = options.Value.EmailDomains
            .Where(configured => !string.IsNullOrWhiteSpace(configured))
            .Any(configured => string.Equals(configured.Trim().TrimStart('@'), domain, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(isDfeUser);
    }

    private static string? EmailDomain(string? email)
    {
        var trimmed = email?.Trim();
        var at = trimmed?.LastIndexOf('@') ?? -1;

        return at > 0 && at < trimmed!.Length - 1 ? trimmed[(at + 1)..] : null;
    }
}
