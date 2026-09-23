using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Constants;
using SAPSec.Core.Features.Measures.Primary;
using SAPSec.Core.Features.Measures.Secondary;
using SAPSec.Core.Features.SchoolDetails;
using SAPSec.Core.Features.SchoolDetails.School;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.UseCases;
using SAPSec.Data.Repositories;
using SAPSec.Web.Areas.AllThrough.ViewModels;
using SAPSec.Web.Areas.Shared.ViewModels;
using SAPSec.Web.Areas.Shared.ViewModels.SimilarSchools;
using SAPSec.Web.Areas.Shared.ViewModels.School;
using SAPSec.Web.Constants;
using SAPSec.Web.Filters;
using SAPSec.Web.ViewModels;
using SAPSec.Web.ViewModels.Measures;

namespace SAPSec.Web.Areas.AllThrough.Controllers;

[Area("AllThrough")]
[Route("school/all-through/{urn}")]
[Authorize]
[RequireFeatureFlag(FeatureFlags.EnableAllThroughSchools)]
[RequireSchoolPhase(ExpectedSchoolPhase.AllThrough)]
public class SchoolController(
        IUseCase<GetSchoolInfoRequest, GetSchoolInfoResponse> getSchoolInfoUseCase,
        IUseCase<GetSchoolDetailsRequest, GetSchoolDetailsResponse> getSchoolDetailsUseCase,
        IUseCase<GetSchoolKs2PerformanceMeasuresRequest, GetSchoolKs2PerformanceMeasuresResponse> getKs2PerformanceMeasuresUseCase,
        IUseCase<GetSchoolKs4CoreSubjectsMeasuresRequest, GetSchoolKs4CoreSubjectsMeasuresResponse> getKs4CoreSubjectsUseCase,
        IUseCase<FindPrimarySimilarSchoolsRequest, FindPrimarySimilarSchoolsResponse> findPrimarySimilarSchoolsUseCase,
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
        var similarSchoolPhases = await GetSimilarSchoolPhasesAsync(urn);

        var model = new AllThroughOverviewViewModel(
            SchoolInfoViewModel.FromSchoolInfo(response.School),
            similarSchoolPhases.HasPrimary,
            similarSchoolPhases.HasSecondary);

        return View(model);
    }

    [HttpGet]
    [Route("ks2")]
    public async Task<IActionResult> Ks2PerformanceMeasures(string urn)
    {
        var filters = Request.Query.ToDictionary(r => r.Key, r => r.Value.ToString());
        var response = await getKs2PerformanceMeasuresUseCase.Execute(new(urn, filters));

        await PopulateViewData(response.School);

        var model = new Ks2PerformanceMeasuresPageViewModel
        {
            School = SchoolInfoViewModel.FromSchoolInfo(response.School),
            MeetingExpectedStandardRwm = MeasureViewModel.FromAllThroughPrimaryMeasure(response.MeetingExpectedStandardRwm, response.School, response.SimilarSchoolsCount > 0),
            AchievedHigherStandardRwm = MeasureViewModel.FromAllThroughPrimaryMeasure(response.AchievedHigherStandardRwm, response.School, response.SimilarSchoolsCount > 0),
            AverageScaledScoreReading = MeasureViewModel.FromAllThroughPrimaryMeasure(response.AverageScaledScoreReading, response.School, response.SimilarSchoolsCount > 0),
            AverageScaledScoreMaths = MeasureViewModel.FromAllThroughPrimaryMeasure(response.AverageScaledScoreMaths, response.School, response.SimilarSchoolsCount > 0),
            MeetingExpectedStandardGps = MeasureViewModel.FromAllThroughPrimaryMeasure(response.MeetingExpectedStandardGps, response.School, response.SimilarSchoolsCount > 0),
            AchievedHigherStandardGps = MeasureViewModel.FromAllThroughPrimaryMeasure(response.AchievedHigherStandardGps, response.School, response.SimilarSchoolsCount > 0)
        };

        return View(model);
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
        var response = await getKs4CoreSubjectsUseCase.Execute(new(urn, filters));

        await PopulateViewData(response.School);

        var hasSimilarSecondarySchools = response.SimilarSchoolsCount > 0;
        var model = new Ks4CoreSubjectsPageViewModel
        {
            School = SchoolInfoViewModel.FromSchoolInfo(response.School),
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

        return View(model);
    }

    [HttpGet]
    [Route("attendance")]
    public async Task<IActionResult> Attendance(string urn)
    {
        var response = await getSchoolInfoUseCase.Execute(new(urn));
        await PopulateViewData(response.School);

        return View(SchoolInfoViewModel.FromSchoolInfo(response.School));
    }

    [HttpGet]
    [Route("view-similar-schools")]
    public async Task<IActionResult> ViewSimilarSchools(
        string urn,
        [FromQuery] string? phase = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? page = null,
        [FromQuery] string? primaryQuery = null)
    {
        var schoolResponse = await getSchoolInfoUseCase.Execute(new(urn));
        await PopulateViewData(schoolResponse.School);

        var similarSchoolPhases = await GetSimilarSchoolPhasesAsync(urn);
        var selectedPhase = phase?.Equals("secondary", StringComparison.OrdinalIgnoreCase) == true
            ? "secondary"
            : "primary";

        SimilarSchoolsPageViewModel? primarySimilarSchools = null;
        if (selectedPhase == "primary" && similarSchoolPhases.HasPrimary)
        {
            var filterBy = SimilarSchoolsPageViewModel.BuildCoreFilters(Request.Query);
            var response = await findPrimarySimilarSchoolsUseCase.Execute(new(
                urn,
                filterBy,
                sortBy,
                page));

            primarySimilarSchools = SimilarSchoolsPageViewModel.Build(
                "primary",
                Request.Query,
                response.CurrentSchool,
                response.SortOptions,
                response.FilterOptions,
                response.ResultsPage,
                response.AllResults,
                response.ValidationErrors,
                Routes.AllThroughSchool(urn).ViewSimilarSchools,
                Routes.AllThroughSchool(urn).WhatIsASimilarSchool,
                comparatorUrn => Routes.AllThroughSchool(urn).PrimaryComparison(comparatorUrn).Similarity);
        }

        return View(AllThroughSimilarSchoolsPageViewModel.FromSchoolInfo(
            schoolResponse.School,
            selectedPhase,
            BuildPrimaryTabUrl(urn, primaryQuery),
            BuildSecondaryTabUrl(urn, selectedPhase),
            primarySimilarSchools,
            similarSchoolPhases.HasPrimary,
            similarSchoolPhases.HasSecondary));
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

    private async Task PopulateViewData(SchoolDetails currentSchool)
    {
        ViewData[ViewDataKeys.SchoolLayout] = SchoolLayoutModel.FromSchoolDetails(currentSchool);
        ViewData[ViewDataKeys.SchoolNavigation] = await CreateNavigation(currentSchool.Urn);
    }

    private async Task<SchoolSideNavigationViewModel> CreateNavigation(string urn) =>
        SchoolSideNavigationViewModel.CreateAllThrough(
            Url,
            urn,
            ControllerContext.ActionDescriptor.ActionName,
            await GetSimilarSchoolPhasesAsync(urn),
            await IsRiseResourcesEnabledAsync());

    private async Task<AllThroughSimilarSchoolPhases> GetSimilarSchoolPhasesAsync(string urn) =>
        new(
            (await similarSchoolsPrimaryRepository.GetGroupAsync(urn)).Any(),
            (await similarSchoolsSecondaryRepository.GetGroupAsync(urn)).Any());

    private async Task<bool> IsRiseResourcesEnabledAsync() =>
        featureFlagService is not null
        && await featureFlagService.IsEnabledAsync(FeatureFlags.EnableRiseResources);

    private string BuildPrimaryTabUrl(string urn, string? primaryQuery)
    {
        if (!string.IsNullOrWhiteSpace(primaryQuery) && primaryQuery.StartsWith('?'))
        {
            return Routes.AllThroughSchool(urn).ViewSimilarSchools + primaryQuery;
        }

        return Routes.AllThroughSchool(urn).ViewSimilarSchools + BuildPrimaryQueryString();
    }

    private string BuildSecondaryTabUrl(string urn, string selectedPhase)
    {
        var url = $"{Routes.AllThroughSchool(urn).ViewSimilarSchools}?phase=secondary";
        var primaryQueryString = selectedPhase == "primary"
            ? BuildPrimaryQueryString()
            : Request.Query.TryGetValue("primaryQuery", out var value)
                ? value.FirstOrDefault()
                : null;

        if (!string.IsNullOrWhiteSpace(primaryQueryString))
        {
            url += $"&primaryQuery={Uri.EscapeDataString(primaryQueryString)}";
        }

        return url;
    }

    private string BuildPrimaryQueryString()
    {
        var queryParts = new List<string>();

        foreach (var (key, values) in Request.Query)
        {
            if (key.Equals("phase", StringComparison.InvariantCultureIgnoreCase)
                || key.Equals("primaryQuery", StringComparison.InvariantCultureIgnoreCase))
            {
                continue;
            }

            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    queryParts.Add($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value!)}");
                }
            }
        }

        return queryParts.Count > 0 ? "?" + string.Join("&", queryParts) : string.Empty;
    }
}
