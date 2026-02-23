using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Web.Constants;
using SAPSec.Web.Helpers;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Controllers;

[Route("school/{urn}")]
public class SimilarSchoolsController : Controller
{
    private readonly ISchoolDetailsService _schoolDetailsService;
    private readonly FindSimilarSchools _findSimilarSchools;
    private readonly ILogger<SimilarSchoolsController> _logger;

    public SimilarSchoolsController(
        ISchoolDetailsService schoolDetailsService,
        FindSimilarSchools findSimilarSchools,
        ILogger<SimilarSchoolsController> logger)
    {
        _schoolDetailsService = schoolDetailsService;
        _findSimilarSchools = findSimilarSchools;
        _logger = logger;
    }

    [HttpGet]
    [Route("view-similar-schools")]
    public async Task<IActionResult> ViewSimilarSchools(
        string urn,
        [FromQuery] string? sortBy,
        [FromQuery] int page = 1)
    {
        var school = await _schoolDetailsService.TryGetByUrnAsync(urn);
        if (school is null)
        {
            _logger.LogInformation("{Urn} was not found on SimilarSchools Controller", urn);
            return RedirectToAction("Error", "School");
        }

        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
        ViewData["SchoolDetails"] = school;

        var coreSortBy = string.IsNullOrWhiteSpace(sortBy) ? "Att8" : sortBy;
        var filterBy = BuildCoreFilters(Request.Query);
        var currentFilters = ExtractCurrentFilters(Request.Query);

        var response = await _findSimilarSchools.Execute(new FindSimilarSchoolsRequest(
            urn,
            filterBy,
            coreSortBy,
            page));

        var schools = response.ResultsPage
            .Select(MapToViewModel)
            .ToList();

        var allSchools = response.AllResults
            .Select(MapToViewModel)
            .ToList();

        var viewModel = new SimilarSchoolsPageViewModel
        {
            EstablishmentName = school.Name.Display(),
            PhaseOfEducation = school.PhaseOfEducation.Display(),
            Urn = int.TryParse(urn, out var urnValue) ? urnValue : 0,
            Schools = schools,
            MapSchools = allSchools,
            FilterOptions = response.FilterOptions,
            SortOptions = response.SortOptions,
            CurrentFilters = currentFilters,
            FilterGroups = BuildFilterGroups(response.FilterOptions),
            SelectedFilterTags = BuildSelectedFilterTags(response.FilterOptions, currentFilters, coreSortBy, urn),
            SortBy = coreSortBy,
            CurrentPage = response.ResultsPage.CurrentPage,
            PageSize = response.ResultsPage.ItemsPerPage,
            TotalResults = response.AllResults.Count
        };

        return View("~/Views/School/ViewSimilarSchools.cshtml", viewModel);
    }

    [HttpGet]
    [Route("similar-schools")]
    public IActionResult Index(string urn)
    {
        var url = Url.Action(nameof(ViewSimilarSchools), "SimilarSchools", new { urn });
        if (string.IsNullOrEmpty(url))
        {
            return RedirectToAction(nameof(ViewSimilarSchools), new { urn });
        }

        return Redirect(url + Request.QueryString);
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

    private static List<SimilarSchoolsFilterGroupViewModel> BuildFilterGroups(
        IReadOnlyCollection<SimilarSchoolsAvailableFilter> filterOptions)
    {
        var categoryKeys = new List<(string Heading, List<string> Keys)>
        {
            ("Location", new List<string> { "dist", "ur" }),
            ("School characteristics", new List<string> { "gender", "admissions", "phase", "type", "size", "sixthform", "nursery", "sen", "resourced" }),
            ("Attendance", new List<string> { "overallabsence", "persistentabsence" })
        };

        var grouped = new List<SimilarSchoolsFilterGroupViewModel>();

        foreach (var (heading, keys) in categoryKeys)
        {
            var filters = keys
                .Select(key => filterOptions.FirstOrDefault(f => f.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase)))
                .Where(f => f != null)
                .Cast<SimilarSchoolsAvailableFilter>()
                .ToList();

            if (filters.Any())
            {
                grouped.Add(new SimilarSchoolsFilterGroupViewModel(heading, filters));
            }
        }

        var knownKeys = categoryKeys.SelectMany(kvp => kvp.Keys).ToHashSet(StringComparer.InvariantCultureIgnoreCase);
        var remaining = filterOptions.Where(f => !knownKeys.Contains(f.Key)).ToList();
        if (remaining.Any())
        {
            grouped.Add(new SimilarSchoolsFilterGroupViewModel("Other filters", remaining));
        }

        return grouped;
    }

    private List<SimilarSchoolsSelectedFilterTagViewModel> BuildSelectedFilterTags(
        IReadOnlyCollection<SimilarSchoolsAvailableFilter> filterOptions,
        Dictionary<string, List<string>> currentFilters,
        string sortBy,
        string urn)
    {
        var tags = new List<SimilarSchoolsSelectedFilterTagViewModel>();
        var baseUrl = Url.Action(nameof(ViewSimilarSchools), "SimilarSchools", new { urn }) ?? $"/school/{urn}/view-similar-schools";

        foreach (var filter in filterOptions)
        {
            foreach (var option in filter.Options.Where(o => o.Selected))
            {
                var queryString = BuildQueryStringWithout(currentFilters, sortBy, filter.Key, option.Key);
                tags.Add(new SimilarSchoolsSelectedFilterTagViewModel(option.Name, baseUrl + queryString));
            }
        }

        return tags;
    }

    private static string BuildQueryStringWithout(
        Dictionary<string, List<string>> currentFilters,
        string sortBy,
        string excludeParam,
        string excludeValue)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            parts.Add($"sortBy={Uri.EscapeDataString(sortBy)}");
        }

        foreach (var (key, values) in currentFilters)
        {
            foreach (var value in values)
            {
                if (key.Equals(excludeParam, StringComparison.InvariantCultureIgnoreCase) &&
                    value.Equals(excludeValue, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                parts.Add($"{key}={Uri.EscapeDataString(value)}");
            }
        }

        return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
    }

    private static SimilarSchoolViewModel MapToViewModel(SimilarSchoolResult result)
    {
        var school = result.SimilarSchool;
        var address = school.Address;

        return new SimilarSchoolViewModel
        {
            UrnRaw = school.URN,
            Urn = int.TryParse(school.URN, out var urn) ? urn : 0,
            EstablishmentName = school.Name,
            Street = address.Street,
            Town = address.Town,
            Postcode = address.Postcode,
            Latitude = result.Coordinates?.Latitude.ToString(),
            Longitude = result.Coordinates?.Longitude.ToString(),
            UrbanOrRural = school.UrbanRuralName,
            Att8Scr = school.Attainment8Score.HasValue ? (double?)school.Attainment8Score.Value : null,
            SortMetricName = result.SortValue.Name,
            SortMetricDisplayValue = result.SortValue.Value.HasValue
                ? result.SortValue.Value.Value.ToString("0.#")
                : "N/A"
        };
    }
}
