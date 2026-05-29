using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Web.Constants;
using SAPSec.Web.Services;

namespace SAPSec.Web.Areas.Primary.Controllers;

/// <summary>
/// Controller for primary school details pages.
/// Single Responsibility: HTTP handling and view selection only.
/// </summary>
[Area("Primary")]
[Route("school/primary/{urn}")]
[Authorize]
public class SchoolController(
    ISchoolDetailsService schoolDetailsService,
    IFeatureFlagService featureFlagService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string urn)
    {
        if (!await featureFlagService.IsEnabledAsync(FeatureFlags.EnablePrimarySchools))
        {
            return NotFound();
        }

        var school = await schoolDetailsService.GetByUrnAsync(urn);
        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
        ViewData["SchoolDetails"] = school;

        return View(school);
    }

    [HttpGet]
    [Route("what-is-a-similar-school")]
    public async Task<IActionResult> WhatIsASimilarSchool(string urn)
    {
        if (!await featureFlagService.IsEnabledAsync(FeatureFlags.EnablePrimarySchools))
        {
            return NotFound();
        }

        var school = await schoolDetailsService.GetByUrnAsync(urn);
        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
        ViewData["SchoolDetails"] = school;

        return View(school);
    }
}
