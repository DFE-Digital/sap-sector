       using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Interfaces.Services;

namespace SAPSec.Web.Controllers;

[Authorize]  
public class SearchController : Controller
{
    private readonly IDsiUserService _userService;

    public SearchController(IDsiUserService userService)
    {
        _userService = userService;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userService.GetUserFromClaimsAsync(User);
        var organisation = await _userService.GetCurrentOrganisationAsync(User);

        ViewBag.User = user;
        ViewBag.Organisation = organisation;

        return View();
    }
}