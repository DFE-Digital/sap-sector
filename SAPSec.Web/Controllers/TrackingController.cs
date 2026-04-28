using Dfe.Analytics.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SAPSec.Web.Controllers;

[AllowAnonymous]
[Route("tracking")]
public class TrackingController() : Controller
{
    [HttpGet]
    [Route("tracked-linked-click")]
    public IActionResult TrackedLinkClick(string? externalLink)
    {
        if (externalLink is not null)
        {
            HttpContext.GetWebRequestEvent()?.AddData("External link click", externalLink);

            return Redirect(externalLink);
        }

        return NoContent();
    }
}
