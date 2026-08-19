using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Constants;
using SAPSec.Core.Features.Primary;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.UseCases;
using SAPSec.Web.Areas.Primary.ViewModels;
using SAPSec.Web.Areas.Shared.ViewModels;
using SAPSec.Web.Constants;
using SAPSec.Web.Filters;
using SAPSec.Web.Services;
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
[RequireSchoolPhase(ExpectedSchoolPhase.Primary)]
[RequireFeatureFlag(FeatureFlags.EnablePrimarySchools)]
public class SchoolController(
    IUseCase<GetSchoolInfoRequest, GetSchoolInfoResponse> getSchoolInfoUseCase,
    IUseCase<GetSchoolKs2PerformanceMeasuresRequest, GetSchoolKs2PerformanceMeasuresResponse> ks2PerformanceMeasuresUseCase,
    IUseCase<GetSchoolAttendanceMeasuresRequest, GetSchoolAttendanceMeasuresResponse> getAttendanceMeasuresUseCase,
    IUseCase<FindPrimarySimilarSchoolsRequest, FindPrimarySimilarSchoolsResponse> findPrimarySimilarSchoolsUseCase,
    IRequestSchoolAccessor requestSchoolAccessor)
    : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string urn)
    {
        var response = await getSchoolInfoUseCase.Execute(new(urn));

        PopulateViewData(response.School);

        return View(SchoolInfoViewModel.FromSchoolInfo(response.School));
    }

    [HttpGet]
    [Route("ks2")]
    public async Task<IActionResult> Ks2PerformanceMeasures(string urn)
    {
        var filters = Request.Query.ToDictionary(r => r.Key, r => r.Value.ToString());
        var response = await ks2PerformanceMeasuresUseCase.Execute(new(urn, filters));

        PopulateViewData(response.School);

        var model = new Ks2PerformanceMeasuresPageViewModel
        {
            School = SchoolInfoViewModel.FromSchoolInfo(response.School),
            MeetingExpectedStandardRwm = MeasureViewModel.FromPrimaryMeasure(response.MeetingExpectedStandardRwm, response.School),
            AchievedHigherStandardRwm = MeasureViewModel.FromPrimaryMeasure(response.AchievedHigherStandardRwm, response.School),
            AverageScaledScoreReading = MeasureViewModel.FromPrimaryMeasure(response.AverageScaledScoreReading, response.School),
            AverageScaledScoreMaths = MeasureViewModel.FromPrimaryMeasure(response.AverageScaledScoreMaths, response.School),
            MeetingExpectedStandardGps = MeasureViewModel.FromPrimaryMeasure(response.MeetingExpectedStandardGps, response.School),
            AchievedHigherStandardGps = MeasureViewModel.FromPrimaryMeasure(response.AchievedHigherStandardGps, response.School)
        };

        return View(model);
    }

    [HttpGet]
    [Route("attendance")]
    public async Task<IActionResult> Attendance(string urn)
    {
        var filters = Request.Query.ToDictionary(r => r.Key, r => r.Value.ToString());
        var response = await getAttendanceMeasuresUseCase.Execute(new(urn, filters));

        PopulateViewData(response.School);

        var model = new AttendanceMeasuresPageViewModel
        {
            School = SchoolInfoViewModel.FromSchoolInfo(response.School),
            Absence = MeasureViewModel.FromPrimaryMeasure(response.Absence, response.School)
        };

        return View(model);
    }

    [HttpGet]
    [Route("view-similar-schools")]
    public async Task<IActionResult> ViewSimilarSchools(
        string urn,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? page = null)
    {
        var schoolInfoResponse = await getSchoolInfoUseCase.Execute(new(urn));
        var filterBy = PrimarySimilarSchoolsPageViewModel.ExtractCurrentFilters(Request.Query)
            .ToDictionary(kvp => kvp.Key, kvp => (IEnumerable<string>)kvp.Value, StringComparer.InvariantCultureIgnoreCase);
        var response = await findPrimarySimilarSchoolsUseCase.Execute(new(
            urn,
            filterBy,
            sortBy,
            page));

        PopulateViewData(schoolInfoResponse.School);

        return View(PrimarySimilarSchoolsPageViewModel.FromResponse(response, Request.Query));
    }

    [HttpGet]
    [Route("school-details")]
    public async Task<IActionResult> SchoolDetails(string urn)
    {
        var response = await getSchoolInfoUseCase.Execute(new(urn));

        PopulateViewData(response.School);

        var schoolDetails = await requestSchoolAccessor.GetAsync(HttpContext, urn);

        return View(schoolDetails);
    }

    [HttpGet]
    [Route("what-is-a-similar-school")]
    public async Task<IActionResult> WhatIsASimilarSchool(string urn)
    {
        var response = await getSchoolInfoUseCase.Execute(new(urn));

        PopulateViewData(response.School);

        return View(SchoolInfoViewModel.FromSchoolInfo(response.School));
    }

    private void PopulateViewData(SchoolInfo currentSchool)
    {
        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(currentSchool.Urn);
        ViewData[ViewDataKeys.SchoolLayout] = SchoolLayoutModel.FromSchoolInfo(currentSchool);
        ViewData[ViewDataKeys.SchoolNavigation] = SchoolSideNavigationViewModel.CreatePrimary(
            Url,
            currentSchool.Urn,
            ControllerContext.ActionDescriptor.ActionName);
    }
}
