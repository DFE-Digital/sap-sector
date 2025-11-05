using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Configuration;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Controllers;

public class HomeController(IConfiguration configuration, IWebHostEnvironment environment) : Controller
{
    public IActionResult Index()
    {
        var settings = configuration.GetSection("DFESignInSettings").Get<DfeSignInSettings>();

        var startNowUrl = environment.IsProduction() ? settings?.SignInUri : Url.Action("Index", "school", null);

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
}