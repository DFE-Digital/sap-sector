using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Constants;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.UseCases;
using SAPSec.Web.Constants;
using SAPSec.Web.Filters;
using SAPSec.Web.Helpers;
using SAPSec.Web.Services;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Areas.Secondary.Controllers;

[Area("Secondary")]
[Route("school/secondary/{urn}")]
[Authorize]
[RequireSchoolPhase(ExpectedSchoolPhase.Secondary)]
public class SimilarSchoolsController(
        IRequestSchoolAccessor requestSchoolAccessor,
        IUseCase<FindSecondarySimilarSchoolsRequest, FindSecondarySimilarSchoolsResponse> findSimilarSchoolsUseCase,
        IFeatureFlagService featureFlagService) : Controller
{
    [HttpGet]
    [Route("view-similar-schools")]
    public async Task<IActionResult> ViewSimilarSchools(
        string urn,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? page = null)
    {
        //var school = await requestSchoolAccessor.GetAsync(HttpContext, urn);

        if (Url is not null)
        {
            ViewData[ViewDataKeys.SchoolNavigation] = SchoolSideNavigationViewModel.CreateSecondary(
                Url,
                urn,
                nameof(ViewSimilarSchools),
                await IsRiseResourcesEnabledAsync());
        }

        var filterBy = BuildCoreFilters(Request.Query);
        var currentFilters = ExtractCurrentFilters(Request.Query);

        var response = await findSimilarSchoolsUseCase.Execute(new(
            urn,
            filterBy,
            sortBy,
            page));

        var schools = response.ResultsPage
            .Select(result => MapToViewModel(result, urn))
            .ToList();

        var allSchools = response.AllResults
            .Select(result => MapToViewModel(result, urn))
            .ToList();

        var responseSortBy = response.SortOptions.First(o => o.Selected).Key;
        var baseUrl = Routes.SecondarySchool(urn).ViewSimilarSchools;

        var viewModel = new SimilarSchoolsPageViewModel
        {
            Urn = response.CurrentSchool.Urn,
            SchoolName = response.CurrentSchool.Name,
            NoResultsMessage = "There are no schools that match your search.",
            PhaseLabel = "secondary",
            ResultsBaseUrl = baseUrl,
            FilterFormUrl = baseUrl,
            SimilarSchools = schools,
            //PhaseOfEducation = school.PhaseOfEducation.Display(),
            //Urn = int.TryParse(urn, out var urnValue) ? urnValue : 0,
            //Schools = schools,
            MapSchools = allSchools,
            //FilterOptions = response.FilterOptions,
            SortOptions = response.SortOptions,
            CurrentFilters = currentFilters,
            FilterGroups = SimilarSchoolsViewModelHelpers.BuildFilterGroups(response.FilterOptions),
            SelectedFilterTags = SimilarSchoolsViewModelHelpers.BuildSelectedFilterTags(
                response.FilterOptions,
                currentFilters,
                responseSortBy,
                baseUrl),
            SortBy = responseSortBy,
            CurrentPage = response.ResultsPage.CurrentPage,
            PageSize = response.ResultsPage.ItemsPerPage,
            TotalResults = response.AllResults.Count,
            ValidationErrors = response.ValidationErrors,
            WhatIsASimilarSchoolUrl = Routes.SecondarySchool(urn).WhatIsASimilarSchool
        };

        return View(viewModel);
    }

    private static Dictionary<string, IEnumerable<string>> BuildCoreFilters(IQueryCollection query)
    {
        return query
            .Where(kvp => kvp.Key != "sortBy" && kvp.Key != "page")
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Where(v => !string.IsNullOrWhiteSpace(v))!.Select(v => v!),
                StringComparer.InvariantCultureIgnoreCase);
    }

    private static Dictionary<string, List<string>> ExtractCurrentFilters(IQueryCollection query)
    {
        var result = new Dictionary<string, List<string>>(StringComparer.InvariantCultureIgnoreCase);
        foreach (var (key, values) in query)
        {
            if (key == "sortBy" || key == "page") continue;
            result[key] = values.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v!).ToList();
        }

        return result;
    }

    private SimilarSchoolViewModel MapToViewModel(SimilarSchoolResult result, string currentSchoolUrn)
    {
        return new SimilarSchoolViewModel
        {
            Urn = result.URN,
            Name = result.Name,
            LocalAuthorityName = result.LocalAuthority.Name,
            FullAddress = result.Address.ToString(),
            Latitude = result.Coordinates?.Latitude.ToString(),
            Longitude = result.Coordinates?.Longitude.ToString(),
            SortMetricName = result.SortValue.Name,
            SortMetricDisplayValue = result.SortValue.Value.Display(),
            ComparisonUrl = Routes.SecondarySchool(currentSchoolUrn).Comparison(result.URN).Similarity
        };
    }

    private async Task<bool> IsRiseResourcesEnabledAsync() =>
        featureFlagService is not null
        && await featureFlagService.IsEnabledAsync(FeatureFlags.EnableRiseResources);
}
