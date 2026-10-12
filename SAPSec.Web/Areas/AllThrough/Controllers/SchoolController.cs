using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.FeatureFlags;
using SAPSec.Core.Features.Measures.Primary;
using SAPSec.Core.Features.Measures.Secondary;
using SAPSec.Core.Features.SchoolDetails.School;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.UseCases;
using SAPSec.Web.Areas.AllThrough.ViewModels;
using SAPSec.Web.Areas.Shared.ViewModels;
using SAPSec.Web.Areas.Shared.ViewModels.School;
using SAPSec.Web.Constants;
using SAPSec.Web.Filters;
using SAPSec.Web.Services;
using SAPSec.Web.ViewModels;
using SAPSec.Web.ViewModels.Components;
using SAPSec.Web.ViewModels.Measures;

namespace SAPSec.Web.Areas.AllThrough.Controllers;

[Area("AllThrough")]
[Route("school/all-through/{urn}")]
[Authorize]
[RequireFeatureFlag(Flags.EnableAllThroughSchools)]
[RequireSchoolPhase(ExpectedSchoolPhase.AllThrough)]
public class SchoolController(
        IUseCase<GetSchoolRequest, GetSchoolResponse> getSchoolInfoUseCase,
        IUseCase<GetSchoolDetailsRequest, GetSchoolDetailsResponse> getSchoolDetailsUseCase,
        IRequestSchoolAccessor requestSchoolAccessor,
        IUseCase<GetSchoolKs2PerformanceMeasuresRequest, GetSchoolKs2PerformanceMeasuresResponse> getKs2PerformanceMeasuresUseCase,
        IUseCase<GetSchoolKs4CoreSubjectsMeasuresRequest, GetSchoolKs4CoreSubjectsMeasuresResponse> getKs4CoreSubjectsUseCase,
        IUseCase<GetAllThroughSimilarSchoolPhasesRequest, GetAllThroughSimilarSchoolPhasesResponse> getAllThroughSimilarSchoolPhasesUseCase,
        IFeatureFlagService featureFlagService)
    : Controller
{
    private const string SharedKs2PerformanceMeasuresView = "~/Areas/Shared/Views/School/Ks2PerformanceMeasures.cshtml";
    private const string SharedKs4CoreSubjectsView = "~/Areas/Shared/Views/School/Ks4CoreSubjects.cshtml";

    [HttpGet]
    public async Task<IActionResult> Index(string urn)
    {
        var school = await requestSchoolAccessor.GetAsync(HttpContext, urn);
        await PopulateViewData(school);
        var similarSchoolPhases = await GetSimilarSchoolPhasesAsync(urn);

        var model = new AllThroughOverviewViewModel(
            SchoolViewModel.FromSchool(school),
            similarSchoolPhases.HasPrimary,
            similarSchoolPhases.HasSecondary);

        return View(model);
    }

    [HttpGet]
    [Route("ks2")]
    public async Task<IActionResult> Ks2PerformanceMeasures(string urn)
    {
        var filters = Request.Query.ToDictionary(r => r.Key, r => r.Value.ToString());
        var school = await requestSchoolAccessor.GetAsync(HttpContext, urn);
        var response = await getKs2PerformanceMeasuresUseCase.Execute(new(urn, filters));

        await PopulateViewData(school);

        var model = new Ks2PerformanceMeasuresPageViewModel
        {
            School = SchoolViewModel.FromSchool(response.School),
            WhatIsASimilarSchoolUrl = Routes.AllThroughSchool(urn).WhatIsASimilarSchool,
            SimilarSchoolDefinitionLinkText = "how DfE identifies what a similar school is",
            MeetingExpectedStandardRwm = MeasureViewModel.FromAllThroughPrimaryMeasure(response.MeetingExpectedStandardRwm, response.School, response.SimilarSchoolsCount > 0),
            AchievedHigherStandardRwm = MeasureViewModel.FromAllThroughPrimaryMeasure(response.AchievedHigherStandardRwm, response.School, response.SimilarSchoolsCount > 0),
            AverageScaledScoreReading = MeasureViewModel.FromAllThroughPrimaryMeasure(response.AverageScaledScoreReading, response.School, response.SimilarSchoolsCount > 0),
            AverageScaledScoreMaths = MeasureViewModel.FromAllThroughPrimaryMeasure(response.AverageScaledScoreMaths, response.School, response.SimilarSchoolsCount > 0),
            MeetingExpectedStandardGps = MeasureViewModel.FromAllThroughPrimaryMeasure(response.MeetingExpectedStandardGps, response.School, response.SimilarSchoolsCount > 0),
            AchievedHigherStandardGps = MeasureViewModel.FromAllThroughPrimaryMeasure(response.AchievedHigherStandardGps, response.School, response.SimilarSchoolsCount > 0)
        };

        return View(SharedKs2PerformanceMeasuresView, model);
    }

    [HttpGet]
    [Route("ks4-headline-measures")]
    public Task<IActionResult> Ks4HeadlineMeasures(string urn) =>
        HeadingPage(urn, "KS4 headline performance measures");

    [HttpGet]
    [Route("ks4-core-subjects")]
    public async Task<IActionResult> Ks4CoreSubjects(string urn)
    {
        var filters = Request.Query.ToDictionary(r => r.Key, r => r.Value.ToString());
        var school = await requestSchoolAccessor.GetAsync(HttpContext, urn);
        var response = await getKs4CoreSubjectsUseCase.Execute(new(urn, filters));

        await PopulateViewData(school);

        var hasSimilarSecondarySchools = response.SimilarSchoolsCount > 0;
        var model = new Ks4CoreSubjectsPageViewModel
        {
            School = SchoolViewModel.FromSchool(response.School),
            WhatIsASimilarSchoolUrl = Routes.AllThroughSchool(urn).WhatIsASimilarSchool,
            SimilarSchoolDefinitionLinkText = "how DfE identifies what a similar school is",
            Measures = [
                MeasureViewModel.FromAllThroughSecondaryMeasure(response.EnglishLanguage, response.School, hasSimilarSecondarySchools),
                MeasureViewModel.FromAllThroughSecondaryMeasure(response.EnglishLiterature, response.School, hasSimilarSecondarySchools),
                MeasureViewModel.FromAllThroughSecondaryMeasure(response.Maths, response.School, hasSimilarSecondarySchools),
                MeasureViewModel.FromAllThroughSecondaryMeasure(response.CombinedScience, response.School, hasSimilarSecondarySchools),
                MeasureViewModel.FromAllThroughSecondaryMeasure(response.Biology, response.School, hasSimilarSecondarySchools),
                MeasureViewModel.FromAllThroughSecondaryMeasure(response.Chemistry, response.School, hasSimilarSecondarySchools),
                MeasureViewModel.FromAllThroughSecondaryMeasure(response.Physics, response.School, hasSimilarSecondarySchools)
            ]
        };

        return View(SharedKs4CoreSubjectsView, model);
    }

    [HttpGet]
    [Route("attendance")]
    public async Task<IActionResult> Attendance(string urn)
    {
        var school = await requestSchoolAccessor.GetAsync(HttpContext, urn);
        await PopulateViewData(school);

        return View(SchoolViewModel.FromSchool(school));
    }

    [HttpGet]
    [Route("school-details")]
    public async Task<IActionResult> SchoolDetails(string urn)
    {
        var school = await requestSchoolAccessor.GetAsync(HttpContext, urn);
        await PopulateViewData(school);

        return View(SchoolViewModel.FromSchool(school));
    }

    [HttpGet]
    [Route("what-is-a-similar-school")]
    public async Task<IActionResult> WhatIsASimilarSchool(string urn)
    {
        var school = await requestSchoolAccessor.GetAsync(HttpContext, urn);
        var similarSchoolPhases = await GetSimilarSchoolPhasesAsync(urn);
        await PopulateViewData(school);

        var model = new AllThroughWhatIsASimilarSchoolViewModel(
            SchoolViewModel.FromSchool(school),
            similarSchoolPhases.HasPrimary,
            similarSchoolPhases.HasSecondary);

        return View(model);
    }

    [HttpGet]
    [RequireFeatureFlag(Flags.EnableRiseResources)]
    [Route("rise-resources")]
    public Task<IActionResult> RiseResources(string urn) =>
        HeadingPage(urn, PageTitles.RiseResources);

    private async Task<IActionResult> HeadingPage(string urn, string title)
    {
        var school = await requestSchoolAccessor.GetAsync(HttpContext, urn);
        await PopulateViewData(school);
        ViewData[ViewDataKeys.Title] = title;

        return View("Page", SchoolViewModel.FromSchool(school));
    }

    private async Task PopulateViewData(LinkedSchool currentSchool)
    {
        ViewData[ViewDataKeys.SchoolLayout] = SchoolLayoutModel.FromSchool(currentSchool);
        ViewData[ViewDataKeys.SchoolNavigation] = await CreateNavigation(currentSchool.Urn);

        ViewData[ViewDataKeys.ShowClosedSchoolBanner] = currentSchool.ShowClosedSchoolBanner;
        ViewData[ViewDataKeys.ClosedSchoolSuccessors] = SuccessorLinkViewModel.FromSuccessors(currentSchool.Successors);
        ViewData[ViewDataKeys.SchoolPredecessors] = SuccessorLinkViewModel.FromSuccessors(currentSchool.Predecessors);
    }

    private async Task<SchoolSideNavigationViewModel> CreateNavigation(string urn) =>
        SchoolSideNavigationViewModel.CreateAllThrough(
            Url,
            urn,
            ControllerContext.ActionDescriptor.ActionName,
            await GetSimilarSchoolPhasesAsync(urn),
            await IsRiseResourcesEnabledAsync());

    private async Task<AllThroughSimilarSchoolPhases> GetSimilarSchoolPhasesAsync(string urn)
    {
        var response = await getAllThroughSimilarSchoolPhasesUseCase.Execute(new(urn));

        return new(response.HasPrimary, response.HasSecondary);
    }

    private async Task<bool> IsRiseResourcesEnabledAsync() =>
        featureFlagService is not null
        && await featureFlagService.IsEnabledAsync(Flags.EnableRiseResources);
}
