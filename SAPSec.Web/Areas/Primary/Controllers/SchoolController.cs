using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Constants;
using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.Measures.Attendance;
using SAPSec.Core.Features.Measures.Primary;
using SAPSec.Core.Features.RiseResources;
using SAPSec.Core.Features.SchoolDetails;
using SAPSec.Core.Features.SchoolDetails.School;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.UseCases;
using SAPSec.Web.Areas.Shared.ViewModels;
using SAPSec.Web.Areas.Shared.ViewModels.School;
using SAPSec.Web.Constants;
using SAPSec.Web.Filters;
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
public class SchoolController(
        IUseCase<GetSchoolInfoRequest, GetSchoolInfoResponse> getSchoolInfoUseCase,
        IUseCase<GetSchoolDetailsRequest, GetSchoolDetailsResponse> getSchoolDetailsUseCase,
        IUseCase<GetSchoolKs2PerformanceMeasuresRequest, GetSchoolKs2PerformanceMeasuresResponse> getKs2PerformanceMeasuresUseCase,
        IUseCase<GetSchoolAttendanceMeasuresRequest, GetSchoolAttendanceMeasuresResponse> getAttendanceMeasuresUseCase,
        IUseCase<FindPrimarySimilarSchoolsRequest, FindPrimarySimilarSchoolsResponse> findPrimarySimilarSchoolsUseCase,
        IUseCase<GetRiseResourcesRequest, GetRiseResourcesResponse> getRiseResourcesUseCase,
        IFeatureFlagService featureFlagService)
    : Controller
{
    private const string SharedKs2PerformanceMeasuresView = "~/Areas/Shared/Views/School/Ks2PerformanceMeasures.cshtml";

    [HttpGet]
    public async Task<IActionResult> Index(string urn)
    {
        var response = await getSchoolInfoUseCase.Execute(new(urn));

        var hasSimilarSchoolsResponse = await GetSimilarSchoolsAsync(urn);

        await PopulateViewData(response.School);

        var model = new SchoolOverviewPageViewModel
        {
            School = SchoolInfoViewModel.FromSchoolInfo(response.School),
            HasSimilarSchools = hasSimilarSchoolsResponse.HasSimilarSchools
        };

        return View(model);
    }

    [HttpGet]
    [Route("ks2")]
    public async Task<IActionResult> Ks2PerformanceMeasures(string urn)
    {
        var filters = Request.Query.ToDictionary(r => r.Key, r => r.Value.ToString());
        var response = await getKs2PerformanceMeasuresUseCase.Execute(new(urn, filters));

        var similarSchoolsResponse = await GetSimilarSchoolsAsync(urn);

        await PopulateViewData(response.School);

        var model = new Ks2PerformanceMeasuresPageViewModel
        {
            School = SchoolInfoViewModel.FromSchoolInfo(response.School),
            WhatIsASimilarSchoolUrl = Routes.PrimarySchool(urn).WhatIsASimilarSchool,
            SimilarSchoolDefinitionLinkText = "how DfE defines what a similar school is",
            MeetingExpectedStandardRwm = MeasureViewModel.FromPrimaryMeasure(response.MeetingExpectedStandardRwm, response.School, similarSchoolsResponse.HasSimilarSchools),
            AchievedHigherStandardRwm = MeasureViewModel.FromPrimaryMeasure(response.AchievedHigherStandardRwm, response.School, similarSchoolsResponse.HasSimilarSchools),
            AverageScaledScoreReading = MeasureViewModel.FromPrimaryMeasure(response.AverageScaledScoreReading, response.School, similarSchoolsResponse.HasSimilarSchools),
            AverageScaledScoreMaths = MeasureViewModel.FromPrimaryMeasure(response.AverageScaledScoreMaths, response.School, similarSchoolsResponse.HasSimilarSchools),
            MeetingExpectedStandardGps = MeasureViewModel.FromPrimaryMeasure(response.MeetingExpectedStandardGps, response.School, similarSchoolsResponse.HasSimilarSchools),
            AchievedHigherStandardGps = MeasureViewModel.FromPrimaryMeasure(response.AchievedHigherStandardGps, response.School, similarSchoolsResponse.HasSimilarSchools)
        };

        return View(SharedKs2PerformanceMeasuresView, model);
    }

    [HttpGet]
    [Route("attendance")]
    public async Task<IActionResult> Attendance(string urn)
    {
        var filters = Request.Query.ToDictionary(r => r.Key, r => r.Value.ToString());
        var response = await getAttendanceMeasuresUseCase.Execute(new(MeasurePhase.Primary, urn, filters));

        var similarSchoolsResponse = await GetSimilarSchoolsAsync(urn);

        await PopulateViewData(response.School, similarSchoolsResponse.HasSimilarSchools);

        var model = new AttendancePageViewModel
        {
            School = SchoolInfoViewModel.FromSchoolInfo(response.School),
            Absence = MeasureViewModel.FromPrimaryMeasure(response.Absence, response.School)
        };

        return View(model);
    }

    [HttpGet]
    [Route("school-details")]
    public async Task<IActionResult> SchoolDetails(string urn)
    {
        var response = await getSchoolDetailsUseCase.Execute(new(urn));
        await PopulateViewData(response.SchoolDetails);
        return View(SchoolDetailsViewModel.FromSchoolDetails(response.SchoolDetails));
    }

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
    public async Task<IActionResult> RiseResources(string urn)
    {
        var schoolInfoResponse = await getSchoolInfoUseCase.Execute(new(urn));
        await PopulateViewData(schoolInfoResponse.School);

        var riseResourcesResponse = await getRiseResourcesUseCase.Execute(new(urn));

        return View(RiseResourcesPageViewModel.FromResponse(riseResourcesResponse));
    }

    private async Task PopulateViewData(SchoolInfo currentSchool, bool hasSimilarSchools = true)
    {
        var similarSchoolsResponse = await GetSimilarSchoolsAsync(currentSchool.Urn);

        ViewData[ViewDataKeys.SchoolLayout] = SchoolLayoutModel.FromSchoolInfo(currentSchool);

        var includeRise = featureFlagService is not null
            && await featureFlagService.IsEnabledAsync(FeatureFlags.EnableRiseResources);

        ViewData[ViewDataKeys.SchoolNavigation] = SchoolSideNavigationViewModel.CreatePrimary(
            Url,
            currentSchool.Urn,
            ControllerContext.ActionDescriptor.ActionName,
            similarSchoolsResponse.HasSimilarSchools,
            includeRise);
    }

    private async Task PopulateViewData(SchoolDetails currentSchool)
    {
        var similarSchoolsResponse = await GetSimilarSchoolsAsync(currentSchool.Urn);

        ViewData[ViewDataKeys.SchoolLayout] = SchoolLayoutModel.FromSchoolDetails(currentSchool);

        var includeRise = featureFlagService is not null
            && await featureFlagService.IsEnabledAsync(FeatureFlags.EnableRiseResources);

        ViewData[ViewDataKeys.SchoolNavigation] = SchoolSideNavigationViewModel.CreatePrimary(
            Url,
            currentSchool.Urn,
            ControllerContext.ActionDescriptor.ActionName,
            similarSchoolsResponse.HasSimilarSchools,
            includeRise);
    }

    private async Task<FindPrimarySimilarSchoolsResponse> GetSimilarSchoolsAsync(string urn)
    {
        return await findPrimarySimilarSchoolsUseCase.Execute(new FindPrimarySimilarSchoolsRequest(urn));
    }
}
