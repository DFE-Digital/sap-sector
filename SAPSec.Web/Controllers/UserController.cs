using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Web.Constants;

namespace SAPSec.Web.Controllers;

[Authorize]
[Route("user")]
public class UserController(
    IUserService userService,
    ILogger<UserController> logger) : Controller
{
    private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    private readonly ILogger<UserController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    [HttpGet]
    [Route("redirect")]
    public async Task<IActionResult> Index()
    {
        var user = await _userService.GetUserFromClaimsAsync(User);
        if (user is null)
        {
            var userName = User.Identity?.Name ?? "(unknown)";
            throw new InvalidOperationException($"Unable to create user object from claims for user {userName}.");
        }

        var userId = user.Sub;

        var org = await _userService.GetCurrentOrganisationAsync(User);
        if (org is null)
        {
            throw new InvalidOperationException($"Current organisation is null for user {userId}.");
        }

        if (!org.IsEstablishment || org.Urn is null)
        {
            _logger.LogInformation("User Organisation is not an Establishment or has a null Urn, redirecting to school search for user {UserId}.", userId);
            return Redirect(Routes.FindASchool);
        }

        return Redirect(Routes.School(org.Urn));
    }
}
