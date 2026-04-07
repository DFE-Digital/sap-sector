using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Web.Constants;
using SAPSec.Web.Helpers;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Controllers;

[Route("school/{urn}")]
[Authorize]
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
        [FromQuery] string? sortBy = null,
        [FromQuery] string? page = null)
    {
        var school = await _schoolDetailsService.GetByUrnAsync(urn);

        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
        ViewData["SchoolDetails"] = school;

        var filterBy = BuildCoreFilters(Request.Query);
        var currentFilters = ExtractCurrentFilters(Request.Query);

        var response = await _findSimilarSchools.Execute(new FindSimilarSchoolsRequest(
            urn,
            filterBy,
            sortBy,
            page));

        var schools = response.ResultsPage
            .Select(MapToViewModel)
            .ToList();

        var allSchools = response.AllResults
            .Select(MapToViewModel)
            .ToList();

        var responseSortBy = response.SortOptions.First(o => o.Selected).Key;

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
            FilterGroups = BuildFilterGroups(response.FilterOptions),
            SelectedFilterTags = BuildSelectedFilterTags(response.FilterOptions, currentFilters, responseSortBy, urn),
            SortBy = responseSortBy,
            CurrentPage = response.ResultsPage.CurrentPage,
            PageSize = response.ResultsPage.ItemsPerPage,
            TotalResults = response.AllResults.Count,
            ValidationErrors = response.ValidationErrors
        };

        return View(viewModel);
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
            ("Location", new List<string> { "dist", "reg", "ur" }),
            ("School characteristics", new List<string> { "poe", "sciu", "np", "sf", "ap", "sp", "goe" }),
            ("Attendance", new List<string> { "oar", "par" })
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
        var baseUrl = Url is null
            ? $"/school/{urn}/view-similar-schools"
            : Url.Action(nameof(ViewSimilarSchools), "SimilarSchools", new { urn }) ?? $"/school/{urn}/view-similar-schools";

        foreach (var filter in filterOptions)
        {
            if (filter is SimilarSchoolsSingleValueAvailableFilter single)
            {
                foreach (var option in single.Options.Where(o => o.Selected))
                {
                    var queryString = BuildQueryStringWithout(currentFilters, sortBy, [(filter.Key, option.Key)]);
                    tags.Add(new SimilarSchoolsSelectedFilterTagViewModel(option.Name, baseUrl + queryString));
                }
            }
            if (filter is SimilarSchoolsMultiValueAvailableFilter multi)
            {
                foreach (var option in multi.Options.Where(o => o.Selected))
                {
                    var queryString = BuildQueryStringWithout(currentFilters, sortBy, [(filter.Key, option.Key)]);
                    tags.Add(new SimilarSchoolsSelectedFilterTagViewModel(option.Name, baseUrl + queryString));
                }
            }
            if (filter is SimilarSchoolsNumericRangeAvailableFilter range
                && !range.ValidationErrors.Any()
                && (!string.IsNullOrWhiteSpace(range.From.Value) || !string.IsNullOrWhiteSpace(range.To.Value)))
            {
                IEnumerable<(string, string)> exclude = [
                    (range.From.Key, range.From.Value),
                    (range.To.Key, range.To.Value)
                ];
                var queryString = BuildQueryStringWithout(currentFilters, sortBy, exclude);
                var rangeText = (string.IsNullOrWhiteSpace(range.From.Value), string.IsNullOrWhiteSpace(range.To.Value)) switch
                {
                    (false, false) => $"from {range.From.Value}% to {range.To.Value}%",
                    (false, true) => $"over {range.From.Value}%",
                    (true, false) => $"up to {range.To.Value}%",
                    _ => ""
                };
                tags.Add(new SimilarSchoolsSelectedFilterTagViewModel($"{range.Name} {rangeText}", baseUrl + queryString));
            }
        }

        return tags;
    }

    private static string BuildQueryStringWithout(
        Dictionary<string, List<string>> currentFilters,
        string sortBy,
        IEnumerable<(string Key, string Value)> exclude)
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
                if (exclude.Any(e => key.Equals(e.Key, StringComparison.InvariantCultureIgnoreCase)
                    && value.Equals(e.Value, StringComparison.InvariantCultureIgnoreCase)))
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
            UrbanOrRural = school.UrbanRural.Name,
            SortMetricName = result.SortValue.Name,
            SortMetricDisplayValue = result.SortValue.Value.Display()
        };
    }
}
