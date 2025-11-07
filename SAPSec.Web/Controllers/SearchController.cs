using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Interfaces.Services;

namespace SAPSec.Web.Controllers;

public class SearchController(IDsiUserService userService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var user = await userService.GetUserFromClaimsAsync(User);
        var organisation = await userService.GetCurrentOrganisationAsync(User);

        ViewBag.User = user;
        ViewBag.Organisation = organisation;

        return View();
    }
}