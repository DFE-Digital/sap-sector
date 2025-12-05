using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SAPSec.Core.Configuration;
using SAPSec.Web.Constants;
using SAPSec.Web.ViewModels;
using SmartBreadcrumbs.Attributes;

namespace SAPSec.Web.Controllers;

public class HomeController(IOptions<DfeSignInSettings> configuration, IWebHostEnvironment environment) : Controller
{
    [DefaultBreadcrumb(PageTitles.ServiceHome)]
    public IActionResult Index()
    {
        var startNowUrl = environment.IsProduction() ? configuration.Value.SignInUri : Url.Action("Index", "SchoolSearch", null);

        return View(new HomeViewModel { StartNowUri = startNowUrl });
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpGet]
    [Route("/Home/StatusCode")]
    public IActionResult StatusCode(int code)
    {
        if (code == 403)
        {
            return RedirectToAction("AccessDenied", "Auth");
        }

        if (code == 404)
        {
            return View("NotFound");
        }

        return View("Error");
    }
}