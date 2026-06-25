using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Constants;
using SAPSec.Core.Features.Primary;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.UseCases;
using SAPSec.Web.Areas.Primary.ViewModels;
using SAPSec.Web.Constants;
using SAPSec.Web.ViewModels;
using SAPSec.Web.ViewModels.Measures;

namespace SAPSec.Web.Areas.Primary.Controllers;

/// <summary>
/// Controller for primary school details pages.
/// Single Responsibility: HTTP handling and view selection only.
/// </summary>
[Area("Primary")]
[Route("school/primary/{urn}")]
[Authorize]
public class SchoolController(
    IUseCase<GetSchoolInfoRequest, GetSchoolInfoResponse> getSchoolInfoUseCase,
    IUseCase<GetSchoolKs2PerformanceMeasuresRequest, GetSchoolKs2PerformanceMeasuresResponse> ks2PerformanceMeasuresUseCase,
    IFeatureFlagService featureFlagService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string urn)
    {
        return await RenderPrimarySchoolViewAsync(urn);
    }

    [HttpGet]
    [Route("ks2")]
    public async Task<IActionResult> Ks2PerformanceMeasures(string urn)
    {
        if (!await featureFlagService.IsEnabledAsync(FeatureFlags.EnablePrimarySchools))
        {
            return NotFound();
        }

        var response = await ks2PerformanceMeasuresUseCase.Execute(new(urn));

        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
        ViewData[ViewDataKeys.SchoolLayout] = SchoolLayoutModel.FromSchoolInfo(response.School);
        ViewData[ViewDataKeys.SchoolNavigation] = SchoolSideNavigationViewModel.CreatePrimary(
            Url,
            response.School.Urn,
            ControllerContext.ActionDescriptor.ActionName);

        var model = new Ks2MeasuresPageViewModel
        {
            School = SchoolInfoViewModel.FromSchoolInfo(response.School),
            MeetingExpectedStandardRwm = MeasureViewModel.FromMeasure(response.MeetingExpectedStandardRwm, response.School)
        };

        return View(model);
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
    [Route("view-similar-schools/{similarSchoolUrn}")]
    public async Task<IActionResult> SimilarSchoolComparison(string urn, string similarSchoolUrn)
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

        var response = await getSchoolInfoUseCase.Execute(new(urn));

        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
        ViewData[ViewDataKeys.SchoolLayout] = SchoolLayoutModel.FromSchoolInfo(response.School);
        ViewData[ViewDataKeys.SchoolNavigation] = SchoolSideNavigationViewModel.CreatePrimary(
            Url,
            response.School.Urn,
            ControllerContext.ActionDescriptor.ActionName);

        return View(SchoolInfoViewModel.FromSchoolInfo(response.School));
    }
}
