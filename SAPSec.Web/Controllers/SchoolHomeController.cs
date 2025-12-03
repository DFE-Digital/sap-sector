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

        var currentOrg = await _userService.GetCurrentOrganisationAsync(User);

        if (currentOrg?.Category?.Name != "Establishment")
        {
            return RedirectToAction("Index", "SchoolSearch");
        }

        ViewBag.SchoolName = user.Organisations.FirstOrDefault(o => o.Id == currentOrg?.Id)?.Name ?? "School";
        ViewBag.UserName = user.Name;

        return View();
    }
}
