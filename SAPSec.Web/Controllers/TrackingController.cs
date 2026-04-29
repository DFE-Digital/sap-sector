using Dfe.Analytics.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SAPSec.Web.Controllers;

[AllowAnonymous]
[Route("tracking")]
public class TrackingController() : Controller
{
    //[HttpGet]
    //[Route("tracked-link-clicked")]
    //public IActionResult TrackedLinkClicked(string? externalLink)
    //{
    //    if (externalLink is not null)
    //    {
    //        HttpContext.GetWebRequestEvent()?.AddData("External link click", externalLink);

    //        return Redirect(externalLink);
    //    }

    //    return NoContent();
    //}
}
