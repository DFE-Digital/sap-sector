using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SAPSec.Core.Configuration;
using SAPSec.Core.Features.Home.UseCases;
using SAPSec.Web.Constants;
using SAPSec.Web.ViewModels;
using SmartBreadcrumbs.Attributes;

namespace SAPSec.Web.Controllers;

[AllowAnonymous]
public class HomeController(
    IOptions<DfeSignInSettings> configuration,
    IWebHostEnvironment environment,
    GetEnablePrimarySchools getEnablePrimarySchools) : Controller
{
    [DefaultBreadcrumb(PageTitles.ServiceHome)]
    public async Task<IActionResult> Index()
    {
        var startNowUrl = environment.IsProduction() ? configuration.Value.SignInUri : Url.Action("Index", "SchoolSearch", null);
        var featureFlags = await getEnablePrimarySchools.Execute();

        return View(new HomeViewModel
        {
            StartNowUri = startNowUrl,
            EnablePrimarySchools = featureFlags.EnablePrimarySchools
        });
    }
}
