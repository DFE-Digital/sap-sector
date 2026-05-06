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
            throw new InvalidOperationException("User claim was null.");
        }

        var org = await _userService.GetCurrentOrganisationAsync(User);
        if (org is null)
        {
            throw new InvalidOperationException("User Organisation claim was null.");
        }

        if (!org.IsEstablishment || org.Urn is null)
        {
            _logger.LogInformation("User Organisation is not an Establishment or has a null Urn, redirecting to school search.");
            return Redirect(Routes.FindASchool);
        }

        return Redirect(Routes.School(org.Urn));
    }
}
