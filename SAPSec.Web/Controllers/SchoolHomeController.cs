using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SAPSec.Core.Configuration;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;

namespace SAPSec.Web.Controllers;

[Authorize]
public class SchoolHomeController(
    IUserService userService,
    ILogger<SchoolHomeController> logger,
    IOptions<PrivateBetaRestrictedAccess> privateBetaOptions) : Controller
{
    private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    private readonly ILogger<SchoolHomeController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly PrivateBetaRestrictedAccess _privateBeta = privateBetaOptions.Value ?? throw new ArgumentNullException(nameof(privateBetaOptions));

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userService.GetUserFromClaimsAsync(User);
        if (user is null)
        {
            _logger.LogWarning("User claim was null.");
            return AccessDenied();
        }

        var org = await _userService.GetCurrentOrganisationAsync(User);
        if (org is null)
        {
            _logger.LogWarning("User Organisation claim was null.");
            return AccessDenied();
        }

        if (!IsEstablishment(org) || org.Urn is null)
        {
            _logger.LogInformation("User Organisation is not an Establishment or has a null Urn, redirecting to school search.");
            return Redirect(Routes.SchoolSearch);
        }

        if (!_privateBeta.UrnWhitelist.Contains(org.Urn))
        {
            _logger.LogWarning($"User Organisation {org.Name} (URN: {org.Urn}) was not part of the private beta whitelist: [{string.Join(", ", _privateBeta.UrnWhitelist)}]");
            return AccessDenied();
        }

        return Redirect(Routes.School(org.Urn));
    }

    private static bool IsEstablishment(Organisation organisation)
    {
        return string.Equals(
            organisation?.Category?.Name,
            "Establishment",
            StringComparison.OrdinalIgnoreCase);
    }

    private IActionResult AccessDenied()
    {
        return RedirectToAction("StatusCodeError", "Error", new { statusCode = 403 });
    }

    private static class Routes
    {
        public const string SchoolSearch = "/find-a-school";
        public static string School(string urn) => $"/school/{urn}";
    }
}
