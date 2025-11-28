using Microsoft.AspNetCore.Mvc;

namespace SAPSec.Web.Controllers;

[Controller]
[Route("/")]
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
}