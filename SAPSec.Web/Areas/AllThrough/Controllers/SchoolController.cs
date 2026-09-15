using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Constants;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.UseCases;
using SAPSec.Data.Repositories;
using SAPSec.Web.Areas.Shared.ViewModels;
using SAPSec.Web.Areas.Shared.ViewModels.School;
using SAPSec.Web.Constants;
using SAPSec.Web.Filters;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Areas.AllThrough.Controllers;

[Area("AllThrough")]
[Route("school/all-through/{urn}")]
[Authorize]
[RequireFeatureFlag(FeatureFlags.EnableAllThroughSchools)]
[RequireSchoolPhase(ExpectedSchoolPhase.AllThrough)]
public class SchoolController(
        IUseCase<GetSchoolInfoRequest, GetSchoolInfoResponse> getSchoolInfoUseCase,
        ISimilarSchoolsPrimaryRepository similarSchoolsPrimaryRepository,
        ISimilarSchoolsSecondaryRepository similarSchoolsSecondaryRepository,
        IFeatureFlagService featureFlagService)
    : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string urn)
    {
        var response = await getSchoolInfoUseCase.Execute(new(urn));
        await PopulateViewData(response.School);
        return View(SchoolInfoViewModel.FromSchoolInfo(response.School));
    }

    [HttpGet]
    [Route("ks2")]
    public Task<IActionResult> Ks2PerformanceMeasures(string urn) =>
        HeadingPage(urn, "KS2 performance measures");

    [HttpGet]
    [Route("ks4-headline-measures")]
    public Task<IActionResult> Ks4HeadlineMeasures(string urn) =>
        HeadingPage(urn, "KS4 headline performance measures");

    [HttpGet]
    [Route("ks4-core-subjects")]
    public Task<IActionResult> Ks4CoreSubjects(string urn) =>
        HeadingPage(urn, "KS4 core subject GCSE results");

    [HttpGet]
    [Route("attendance")]
    public Task<IActionResult> Attendance(string urn) =>
        HeadingPage(urn, "Attendance measures");

    [HttpGet]
    [Route("view-similar-schools")]
    public Task<IActionResult> ViewSimilarSchools(string urn) =>
        HeadingPage(urn, "View similar schools");

    [HttpGet]
    [Route("school-details")]
    public Task<IActionResult> SchoolDetails(string urn) =>
        HeadingPage(urn, "School details");

    [HttpGet]
    [Route("what-is-a-similar-school")]
    public async Task<IActionResult> WhatIsASimilarSchool(string urn)
    {
        var response = await getSchoolInfoUseCase.Execute(new(urn));
        await PopulateViewData(response.School);
        return View(SchoolInfoViewModel.FromSchoolInfo(response.School));
    }

    [HttpGet]
    [RequireFeatureFlag(FeatureFlags.EnableRiseResources)]
    [Route("rise-resources")]
    public Task<IActionResult> RiseResources(string urn) =>
        HeadingPage(urn, PageTitles.RiseResources);

    private async Task<IActionResult> HeadingPage(string urn, string title)
    {
        var response = await getSchoolInfoUseCase.Execute(new(urn));
        await PopulateViewData(response.School);
        ViewData[ViewDataKeys.Title] = title;

        return View("Page", SchoolInfoViewModel.FromSchoolInfo(response.School));
    }

    private async Task PopulateViewData(SchoolInfo currentSchool)
    {
        ViewData[ViewDataKeys.SchoolLayout] = SchoolLayoutModel.FromSchoolInfo(currentSchool);
        ViewData[ViewDataKeys.SchoolNavigation] = await CreateNavigation(currentSchool.Urn);
    }

    private async Task<SchoolSideNavigationViewModel> CreateNavigation(string urn) =>
        SchoolSideNavigationViewModel.CreateAllThrough(
            Url,
            urn,
            ControllerContext.ActionDescriptor.ActionName,
            await HasSimilarSchoolsAsync(urn),
            await IsRiseResourcesEnabledAsync());

    private async Task<bool> HasSimilarSchoolsAsync(string urn) =>
        (await similarSchoolsPrimaryRepository.GetGroupAsync(urn)).Any()
        || (await similarSchoolsSecondaryRepository.GetGroupAsync(urn)).Any();

    private async Task<bool> IsRiseResourcesEnabledAsync() =>
        featureFlagService is not null
        && await featureFlagService.IsEnabledAsync(FeatureFlags.EnableRiseResources);
}
