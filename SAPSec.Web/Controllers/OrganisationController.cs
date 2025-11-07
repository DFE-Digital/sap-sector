using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Interfaces.Services;

namespace SAPSec.Web.Controllers;

[Route("[controller]")]
//[Authorize]
public class OrganisationController(
    IDsiUserService userService,
    IDsiApiService apiService,
    ILogger<OrganisationController> logger) : Controller
{
    private readonly IDsiUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    private readonly IDsiApiService _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
    private readonly ILogger<OrganisationController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    [HttpGet("details")]
    public async Task<IActionResult> Details()
    {
        var organisation = await _userService.GetCurrentOrganisationAsync(User);
        if (organisation == null)
        {
            _logger.LogWarning(
                "No organisation found for user {UserId}",
                _userService.GetUserId(User));
            return RedirectToAction("Error", "Home");
        }

        return View(organisation);
    }

    [HttpGet("switch")]
    public async Task<IActionResult> Switch()
    {
        var user = await _userService.GetUserFromClaimsAsync(User);
        if (user == null || !user.Organisations.Any())
        {
            return RedirectToAction("Error", "Home");
        }

        return View(user);
    }

    [HttpPost("switch")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Switch(string organisationId, string? returnUrl = null)
    {
        if (string.IsNullOrEmpty(organisationId))
        {
            return BadRequest("Organisation ID is required");
        }

        var user = await _userService.GetUserFromClaimsAsync(User);
        if (user == null)
        {
            return RedirectToAction("Error", "Home");
        }

        var organisation = user.Organisations.FirstOrDefault(o => o.Id == organisationId);
        if (organisation == null)
        {
            _logger.LogWarning(
                "User {UserId} attempted to switch to invalid organisation {OrganisationId}",
                user.Sub,
                organisationId);
            return BadRequest("Invalid organisation");
        }

        await _userService.SetCurrentOrganisationAsync(User, organisationId);

        _logger.LogInformation(
            "User {UserId} switched to organisation {OrganisationId}",
            user.Sub,
            organisationId);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Details));
    }

    [HttpGet("api/current")]
    public async Task<IActionResult> GetCurrent()
    {
        var organisation = await _userService.GetCurrentOrganisationAsync(User);
        if (organisation == null)
        {
            return NotFound();
        }

        return Json(organisation);
    }

    [HttpGet("api/{organisationId}")]
    public async Task<IActionResult> GetOrganisation(string organisationId)
    {
        if (string.IsNullOrEmpty(organisationId))
        {
            return BadRequest("Organisation ID is required");
        }

        var organisation = await _apiService.GetOrganisationAsync(organisationId);
        if (organisation == null)
        {
            return NotFound();
        }

        return Json(organisation);
    }
}