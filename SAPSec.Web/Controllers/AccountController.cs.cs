namespace SAPSec.Web.Controllers;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;

    public AccountController(ILogger<AccountController> logger)
    {
        _logger = logger;
    }

    [HttpGet("/login")]
    public IActionResult Login(string returnUrl = "/")
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return LocalRedirect(returnUrl);
        }

        return Challenge(new AuthenticationProperties
        {
            RedirectUri = returnUrl
        }, OpenIdConnectDefaults.AuthenticationScheme);
    }

    [HttpPost("/logout")]
    [HttpGet("/logout")]
    [Authorize]
    public IActionResult Logout()
    {
        return SignOut(
            new AuthenticationProperties { RedirectUri = "/" },
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme);
    }

    [HttpGet("/signout-callback-oidc")]
    public IActionResult SignoutCallback()
    {
        return RedirectToAction("Index", "Home");
    }

    [HttpGet("/access-denied")]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpGet("/error")]
    public IActionResult Error(string? message = null)
    {
        ViewBag.ErrorMessage = message ?? "An error occurred";
        return View();
    }
}
