using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Interfaces.Services;

namespace SAPSec.Web.Controllers;

[Authorize]
public class SchoolHomeController : Controller
{
    private readonly IDsiUserService _userService;
    private readonly ILogger<SchoolHomeController> _logger;

    public SchoolHomeController(
        IDsiUserService userService,
        ILogger<SchoolHomeController> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userService.GetUserFromClaimsAsync(User);

        if (user == null)
        {
            _logger.LogWarning("User authenticated but could not load user data");
            return RedirectToAction("Error", "Home");
        }

        var currentOrg = await _userService.GetCurrentOrganisationAsync(User);

        if (currentOrg == null && user.Organisations.Count > 1)
        {
            _logger.LogInformation("User needs to select organisation");
            return RedirectToAction("Index", "SchoolSearch");
        }

        ViewBag.SchoolName = currentOrg?.Name ?? "Your School";
        ViewBag.UserName = user.Name;

        return View();
    }
}
