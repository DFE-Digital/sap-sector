using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Interfaces.Services;

namespace SAPSec.Web.Controllers;

[Route("auth")]
public class AuthController(
    IUserService userService,
    ILogger<AuthController> logger) : Controller
{
    private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    private readonly ILogger<AuthController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private static class Routes
    {
        public const string SignIn = "signin";
        public const string SignOut = "signout";
        public const string SignedOut = "signed-out";
    }

    private static class LogMessages
    {
        public const string UserSigningOut = "User {UserId} signing out";
    }

    [HttpGet(Routes.SignIn)]
    [AllowAnonymous]
    public IActionResult SignIn(string? returnUrl = null)
    {
        if (IsUserAuthenticated())
        {
            return RedirectToLocal(returnUrl);
        }

        return ChallengeWithRedirect(returnUrl);
    }

    [HttpGet(Routes.SignOut)]
    [AllowAnonymous]
    public new async Task<IActionResult> SignOut()
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

    [HttpGet(Routes.SignedOut)]
    [AllowAnonymous]
    public IActionResult SignedOut()
    {
        return View();
    }

    #region Private Helper Methods

    private bool IsUserAuthenticated()
    {
        return _userService.IsAuthenticated(User);
    }

    private IActionResult ChallengeWithRedirect(string? returnUrl)
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = returnUrl ?? Constants.Routes.FindASchool
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

    #endregion

    #region Logging Methods

    private void LogUserSigningOut()
    {
        var userId = _userService.GetUserId(User);
        _logger.LogInformation(LogMessages.UserSigningOut, userId);
    }

    #endregion
}
