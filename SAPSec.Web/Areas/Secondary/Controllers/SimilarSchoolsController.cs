using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Features.SimilarSchools.UseCases;
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
public class SimilarSchoolsController : Controller
{
    private readonly IRequestSchoolAccessor _requestSchoolAccessor;
    private readonly FindSimilarSchools _findSimilarSchools;
    private readonly ILogger<SimilarSchoolsController> _logger;

    public SimilarSchoolsController(
        IRequestSchoolAccessor requestSchoolAccessor,
        FindSimilarSchools findSimilarSchools,
        ILogger<SimilarSchoolsController> logger)
    {
        _requestSchoolAccessor = requestSchoolAccessor;
        _findSimilarSchools = findSimilarSchools;
        _logger = logger;
    }

    [HttpGet]
    [Route("view-similar-schools")]
    public async Task<IActionResult> ViewSimilarSchools(
        string urn,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? page = null)
    {
        var school = await _requestSchoolAccessor.GetAsync(HttpContext, urn);

        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
        ViewData[ViewDataKeys.SchoolDetails] = school;
        if (Url is not null)
        {
            ViewData[ViewDataKeys.SchoolNavigation] = SchoolSideNavigationViewModel.CreateSecondary(
                Url,
                school?.Urn ?? urn,
                nameof(ViewSimilarSchools));
        }

        var filterBy = BuildCoreFilters(Request.Query);
        var currentFilters = ExtractCurrentFilters(Request.Query);

        var response = await _findSimilarSchools.Execute(new FindSimilarSchoolsRequest(
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
            EstablishmentName = school.Name,
            PhaseOfEducation = school.PhaseOfEducation.Display(),
            Urn = int.TryParse(urn, out var urnValue) ? urnValue : 0,
            Schools = schools,
            MapSchools = allSchools,
            FilterOptions = response.FilterOptions,
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
            FilterFormUrl = baseUrl,
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
        var school = result.SimilarSchool;
        var address = school.Address;

        return new SimilarSchoolViewModel
        {
            UrnRaw = school.URN,
            Urn = int.TryParse(school.URN, out var urn) ? urn : 0,
            EstablishmentName = school.Name,
            LocalAuthorityName = school.LocalAuthority.Name,
            Street = address.Street,
            Town = address.Town,
            Postcode = address.Postcode,
            Latitude = result.Coordinates?.Latitude.ToString(),
            Longitude = result.Coordinates?.Longitude.ToString(),
            UrbanOrRural = school.UrbanRural.Name,
            SortMetricName = result.SortValue.Name,
            SortMetricDisplayValue = result.SortValue.Value.Display(),
            ComparisonUrl = Routes.SecondarySchool(currentSchoolUrn).Comparison(school.URN).Overview
        };
    }
}
