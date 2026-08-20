using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SAPSec.Core.Authentication;
using SAPSec.Core.Constants;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Web.Constants;
using SAPSec.Web.ViewModels;
using SmartBreadcrumbs.Attributes;

namespace SAPSec.Web.Controllers;

[AllowAnonymous]
public class HomeController(
    IOptions<DfeSignInSettings> configuration,
    IWebHostEnvironment environment,
    IFeatureFlagService featureFlagService) : Controller
{
    [DefaultBreadcrumb(PageTitles.ServiceHome)]
    public async Task<IActionResult> Index()
    {
        var startNowUrl = environment.IsProduction() ? configuration.Value.SignInUri : Url.Action("Index", "SchoolSearch", null);
        var enablePrimarySchools = await featureFlagService.IsEnabledAsync(FeatureFlags.EnablePrimarySchools);

        return View(new HomeViewModel
        {
            StartNowUri = startNowUrl,
            EnablePrimarySchools = enablePrimarySchools
        });
    }
}
