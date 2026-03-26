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
}