using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Constants;
using SAPSec.Core.Features.Availability;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.UseCases;
using SAPSec.Web.Areas.Shared.ViewModels.School;
using SAPSec.Web.Constants;
using SAPSec.Web.Filters;
using SAPSec.Web.ViewModels;
using System.Globalization;

namespace SAPSec.Web.Areas.Primary.Controllers;

[Area("Primary")]
[Route("school/primary/{urn}")]
[Authorize]
[RequireSchoolPhase(ExpectedSchoolPhase.Primary)]
[RequireFeatureFlag(FeatureFlags.EnablePrimarySchools)]
public class SimilarSchoolsController(
        IUseCase<GetSchoolInfoRequest, GetSchoolInfoResponse> getSchoolInfoUseCase,
        IUseCase<FindPrimarySimilarSchoolsRequest, FindPrimarySimilarSchoolsResponse> findSimilarSchoolsUseCase,
        IFeatureFlagService featureFlagService)
    : Controller
{
    [HttpGet]
    [Route("view-similar-schools")]
    public async Task<IActionResult> ViewSimilarSchools(
        string urn,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? page = null)
    {
        //var schoolInfoResponse = await getSchoolInfoUseCase.Execute(new(urn));
        var filterBy = ExtractCurrentFilters(Request.Query)
            .ToDictionary(kvp => kvp.Key, kvp => (IEnumerable<string>)kvp.Value, StringComparer.InvariantCultureIgnoreCase);
        var response = await findSimilarSchoolsUseCase.Execute(new(
            urn,
            filterBy,
            sortBy,
            page));

        await PopulateViewData(response.CurrentSchool);

        var currentFilters = ExtractCurrentFilters(Request.Query);
        var filterOptions = response.FilterOptions;

        var viewModel = new SimilarSchoolsPageViewModel
        {
            Urn = response.CurrentSchool.Urn,
            SchoolName = response.CurrentSchool.Name,
            FilterFormUrl = Routes.PrimarySchool(response.CurrentSchool.Urn).ViewSimilarSchools,
            ResultsBaseUrl = Routes.PrimarySchool(response.CurrentSchool.Urn).ViewSimilarSchools,
            NoResultsMessage = "There are no similar schools available for this school.",
            PhaseLabel = "primary",
            WhatIsASimilarSchoolUrl = Routes.PrimarySchool(response.CurrentSchool.Urn).WhatIsASimilarSchool,
            //CurrentSchool = new SchoolInfoViewModel(
            //    response.CurrentSchool.Urn,
            //    response.CurrentSchool.Name,
            //    string.Empty),
            SimilarSchools = MapRows(response.ResultsPage, response.CurrentSchool.Urn),
            MapSchools = MapRows(response.AllResults, response.CurrentSchool.Urn),
            //Urn = int.TryParse(response.CurrentSchool.Urn, out var urn) ? urn : 0,
            CurrentFilters = currentFilters,
            FilterGroups = SimilarSchoolsViewModelHelpers.BuildFilterGroups(filterOptions),
            SelectedFilterTags = SimilarSchoolsViewModelHelpers.BuildSelectedFilterTags(
                filterOptions,
                currentFilters,
                response.SortOptions.FirstOrDefault(o => o.Selected)?.Key ?? "RwmExpected",
                Routes.PrimarySchool(response.CurrentSchool.Urn).ViewSimilarSchools),
            SortOptions = response.SortOptions,
            SortBy = response.SortOptions.FirstOrDefault(o => o.Selected)?.Key ?? "RwmExpected",
            ValidationErrors = response.ValidationErrors,
            CurrentPage = response.ResultsPage.CurrentPage,
            PageSize = response.ResultsPage.ItemsPerPage,
            TotalResults = response.AllResults.Count
        };

        return View(viewModel);
    }

    private async Task PopulateViewData(SchoolInfo currentSchool)
    {
        ViewData[ViewDataKeys.SchoolLayout] = SchoolLayoutModel.FromSchoolInfo(currentSchool);

        var includeRise = featureFlagService is not null
            && await featureFlagService.IsEnabledAsync(FeatureFlags.EnableRiseResources);

        ViewData[ViewDataKeys.SchoolNavigation] = SchoolSideNavigationViewModel.CreatePrimary(
            Url,
            currentSchool.Urn,
            ControllerContext.ActionDescriptor.ActionName,
            includeRise);
    }

    public static Dictionary<string, List<string>> ExtractCurrentFilters(IQueryCollection query)
    {
        var result = new Dictionary<string, List<string>>(StringComparer.InvariantCultureIgnoreCase);
        foreach (var (key, values) in query)
        {
            if (key == "sortBy" || key == "page")
            {
                continue;
            }

            result[key] = values.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v!).ToList();
        }

        return result;
    }

    private static IReadOnlyCollection<SimilarSchoolViewModel> MapRows(
        IEnumerable<SimilarSchoolResult> rows,
        string currentSchoolUrn) =>
        rows
            .Select(row => new SimilarSchoolViewModel
            {
                Urn = row.URN,
                Name = row.Name,
                LocalAuthorityName = row.LocalAuthority.Name,
                FullAddress = row.Address.ToString(),
                Latitude = row.Coordinates?.Latitude.ToString(CultureInfo.InvariantCulture),
                Longitude = row.Coordinates?.Longitude.ToString(CultureInfo.InvariantCulture),
                SortMetricName = row.SortValue.Name,
                SortMetricDisplayValue = DisplaySortValue(row.SortValue.Value),
                ComparisonUrl = Routes.PrimarySchool(currentSchoolUrn).Comparison(row.URN).Similarity
            })
            .ToList()
            .AsReadOnly();

    private static string DisplaySortValue(DataWithAvailability<string> value) =>
        value.HasValue && value.Value is not null
            ? value.Value
            : "No data available";
}