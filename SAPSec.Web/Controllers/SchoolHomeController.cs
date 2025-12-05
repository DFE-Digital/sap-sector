using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;

namespace SAPSec.Web.Controllers;

[Authorize]
public class SchoolHomeController(
    IUserService userService,
    ILogger<SchoolHomeController> logger) : Controller
{
    private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    private readonly ILogger<SchoolHomeController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userService.GetUserFromClaimsAsync(User);
        var currentOrg = await _userService.GetCurrentOrganisationAsync(User);

        if (!HasValidOrganisation(currentOrg))
        {
            return AccessDenied();
        }

        if (!IsEstablishment(currentOrg))
        {
            return Redirect(Routes.SchoolSearch);
        }

        SetViewBagProperties(user, currentOrg);
        return View();
    }

    private static bool HasValidOrganisation(Organisation? organisation)
    {
        return organisation?.Category != null;
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

    private void SetViewBagProperties(User user, Organisation currentOrg)
    {
        ViewBag.SchoolName = GetSchoolName(user, currentOrg);
        ViewBag.UserName = user.Name;
    }

    private static string GetSchoolName(User user, Organisation currentOrg)
    {
        return user.Organisations.FirstOrDefault(o => o.Id == currentOrg.Id)?.Name ?? "School";
    }

    private static class Routes
    {
        public const string SchoolSearch = "/search-for-a-school";
    }
}
