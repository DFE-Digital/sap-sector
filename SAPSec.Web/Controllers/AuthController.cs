using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;

namespace SAPSec.Web.Controllers;

[Route("[controller]")]
public class AuthController : Controller
{
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    private static class Routes
    {
        public const string SignIn = "sign-in";
        public const string SignOut = "sign-out";
        public const string SignedOut = "signed-out";
        public const string SignOutCallback = "SignOutCallback";
        public const string SelectOrganisation = "select-organisation";
        public const string AccessDenied = "access-denied";
    }

    private static class Defaults
    {
        public const string ReturnUrl = "/search-for-a-school";
    }

    private static class LogMessages
    {
        public const string UserSigningOut = "User {UserId} signing out";
        public const string OrganisationSelected = "User {UserId} selected organisation {OrganisationId}";
        public const string OrganisationSelectionFailed = "Failed to set organisation {OrganisationId} for user {UserId}";
        public const string UserNotFound = "User not found or has no organisations";
    }

    private static class ErrorMessages
    {
        public const string OrganisationIdRequired = "Organisation ID is required";
    }

    public AuthController(
        IUserService userService,
        ILogger<AuthController> logger)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet(Routes.SignIn)]
    public IActionResult SignIn(string? returnUrl = null)
    {
        if (IsUserAuthenticated())
        {
            return RedirectToLocal(returnUrl);
        }

        return ChallengeWithRedirect(returnUrl);
    }

    [HttpGet(Routes.SelectOrganisation)]
    [Authorize]
    public async Task<IActionResult> SelectOrganisation(string? returnUrl = null)
    {
        var user = await _userService.GetUserFromClaimsAsync(User);

        if (!HasValidOrganisations(user))
        {
            _logger.LogWarning(LogMessages.UserNotFound);
            return RedirectToProblem();
        }

        ViewBag.ReturnUrl = returnUrl;
        return View(user);
    }

    [HttpPost(Routes.SelectOrganisation)]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SelectOrganisationPost(string organisationId, string? returnUrl = null)
    {
        if (string.IsNullOrEmpty(organisationId))
        {
            return BadRequest(ErrorMessages.OrganisationIdRequired);
        }

        var success = await _userService.SetCurrentOrganisationAsync(User, organisationId);

        if (!success)
        {
            LogOrganisationSelectionFailed(organisationId);
            return RedirectToProblem();
        }

        LogOrganisationSelected(organisationId);
        return RedirectToLocal(returnUrl);
    }

    [HttpGet(Routes.SignOut)]
    [HttpGet(Routes.SignOutCallback)]
    [Authorize]
    public async Task<IActionResult> SignOut()
    {
        LogUserSigningOut();

        await ClearUserSession();

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action("SignedOut", "Auth")
        };

        return SignOut(properties, OpenIdConnectDefaults.AuthenticationScheme);
    }

    [HttpGet(Routes.AccessDenied)]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpGet(Routes.SignedOut)]
    public IActionResult SignedOut()
    {
        return View();
    }

    #region Private Helper Methods

    private bool IsUserAuthenticated()
    {
        return _userService.IsAuthenticated(User);
    }

    private static bool HasValidOrganisations(User? user)
    {
        return user?.Organisations.Any() == true;
    }

    private IActionResult ChallengeWithRedirect(string? returnUrl)
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = returnUrl ?? Defaults.ReturnUrl
        };

        return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
    }

    private async Task ClearUserSession()
    {
        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (IsValidLocalUrl(returnUrl))
        {
            return Redirect(returnUrl!);
        }

        return RedirectToHome();
    }

    private bool IsValidLocalUrl(string? url)
    {
        return !string.IsNullOrEmpty(url) && Url.IsLocalUrl(url);
    }

    private IActionResult RedirectToHome()
    {
        return RedirectToAction("Index", "Home");
    }

    private IActionResult RedirectToProblem()
    {
        return RedirectToAction("StatusCodeError", "Error", new { statusCode = 500 });
    }

    #endregion

    #region Logging Methods

    private void LogUserSigningOut()
    {
        var userId = _userService.GetUserId(User);
        _logger.LogInformation(LogMessages.UserSigningOut, userId);
    }

    private void LogOrganisationSelected(string organisationId)
    {
        var userId = _userService.GetUserId(User);
        _logger.LogInformation(LogMessages.OrganisationSelected, userId, organisationId);
    }

    private void LogOrganisationSelectionFailed(string organisationId)
    {
        var userId = _userService.GetUserId(User);
        _logger.LogWarning(LogMessages.OrganisationSelectionFailed, organisationId, userId);
    }

    #endregion
}