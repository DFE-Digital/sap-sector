using Dfe.Analytics.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SAPSec.Web.Controllers;

[AllowAnonymous]
//[Route("tracking")]
public class TrackingController() : Controller
{
    // [DefaultBreadcrumb(PageTitles.ServiceHome)]
    public IActionResult LogToBigQuery(string? externalLink)
    {
        //var startNowUrl = environment.IsProduction() ? configuration.Value.SignInUri : Url.Action("Index", "SchoolSearch", null);

        HttpContext.GetWebRequestEvent()?.AddData("External link click", externalLink);


        return Redirect(externalLink);
    }
}
