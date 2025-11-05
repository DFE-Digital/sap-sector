using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Interfaces.Services;

namespace SAPSec.Web.Controllers;

[Route("[controller]")]
public class AuthController(
    IDsiUserService userService,
    ILogger<AuthController> logger) : Controller
{
    private readonly IDsiUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    private readonly ILogger<AuthController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    [HttpGet("sign-in")]
    public IActionResult SignIn(string? returnUrl = null)
    {
        if (_userService.IsAuthenticated(User))
        {
            return RedirectToAction("Index");
        }

        var properties = new AuthenticationProperties
        {
            RedirectUri = returnUrl,
            Items = { { "returnUrl", returnUrl ?? "/" } }
        };

        return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
    }

    [HttpGet("sign-in-callback")]
    public async Task<IActionResult> SignInCallback(string? returnUrl = null)
    {
        try
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded)
            {
                _logger.LogWarning("Authentication failed");
                return RedirectToAction("Error", "Home");
            }

            var user = await _userService.GetUserFromClaimsAsync(User);
            if (user == null)
            {
                _logger.LogWarning("Failed to get user from claims");
                return RedirectToAction("Error", "Home");
            }

            _logger.LogInformation(
                "User {Email} signed in successfully",
                user.Email);

            // If user has multiple organisations, redirect to organisation selection
            if (user.Organisations.Count > 1)
            {
                return RedirectToAction(
                    nameof(SelectOrganisation),
                    new { returnUrl });
            }

            // If user has one organisation, set it as current
            if (user.Organisations.Count == 1)
            {
                await _userService.SetCurrentOrganisationAsync(
                    User,
                    user.Organisations[0].Id);
            }

            return RedirectToLocal(returnUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during sign-in callback");
            return RedirectToAction("Error", "Home");
        }
    }

    [HttpGet("select-organisation")]
    [Authorize]
    public async Task<IActionResult> SelectOrganisation(string? returnUrl = null)
    {
        var user = await _userService.GetUserFromClaimsAsync(User);
        if (user == null || !user.Organisations.Any())
        {
            return RedirectToAction("Error", "Home");
        }

        ViewBag.ReturnUrl = returnUrl;
        return View(user);
    }

    [HttpPost("select-organisation")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SelectOrganisation(
        string organisationId,
        string? returnUrl = null)
    {
        if (string.IsNullOrEmpty(organisationId))
        {
            return BadRequest("Organisation ID is required");
        }

        var success = await _userService.SetCurrentOrganisationAsync(User, organisationId);
        if (!success)
        {
            _logger.LogWarning(
                "Failed to set organisation {OrganisationId} for user {UserId}",
                organisationId,
                _userService.GetUserId(User));
            return RedirectToAction("Error", "Home");
        }

        _logger.LogInformation(
            "User {UserId} selected organisation {OrganisationId}",
            _userService.GetUserId(User),
            organisationId);

        return RedirectToLocal(returnUrl);
    }

    [HttpGet("sign-out")]
    [Authorize]
    public async Task<IActionResult> SignOut()
    {
        var userId = _userService.GetUserId(User);

        _logger.LogInformation("User {UserId} signing out", userId);

        // Clear session
        HttpContext.Session.Clear();

        // Sign out of both the cookie scheme and OIDC
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet("access-denied")]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpGet("signed-out")]
    public IActionResult SignedOut()
    {
        return View();
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }
}