using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SAPSec.Web.Controllers;

[Controller]
[Route("/")]
[AllowAnonymous]
public class StaticContentController : Controller
{
    [HttpGet]
    [Route("accessibility")]
    public IActionResult Accessibility()
    {
        return View();
    }

    [Route("terms-of-use")]
    public IActionResult TermsOfUse()
    {
        return View();
    }

    [Route("cookies")]
    public IActionResult Cookies()
    {
        return View();
    }
}