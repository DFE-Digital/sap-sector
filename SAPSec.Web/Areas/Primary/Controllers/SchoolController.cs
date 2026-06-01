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
        return await RenderPrimarySchoolViewAsync(urn);
    }

    [HttpGet]
    [Route("ks2")]
    public async Task<IActionResult> Ks2(string urn)
    {
        return await RenderPrimarySchoolViewAsync(urn);
    }

    [HttpGet]
    [Route("attendance")]
    public async Task<IActionResult> Attendance(string urn)
    {
        return await RenderPrimarySchoolViewAsync(urn);
    }

    [HttpGet]
    [Route("view-similar-schools")]
    public async Task<IActionResult> ViewSimilarSchools(string urn)
    {
        return await RenderPrimarySchoolViewAsync(urn);
    }

    [HttpGet]
    [Route("school-details")]
    public async Task<IActionResult> SchoolDetails(string urn)
    {
        return await RenderPrimarySchoolViewAsync(urn);
    }

    [HttpGet]
    [Route("what-is-a-similar-school")]
    public async Task<IActionResult> WhatIsASimilarSchool(string urn)
    {
        return await RenderPrimarySchoolViewAsync(urn);
    }

    private async Task<IActionResult> RenderPrimarySchoolViewAsync(string urn)
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
