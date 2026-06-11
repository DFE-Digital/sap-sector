using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.School.Search;
using SAPSec.Web.ViewModels;
using System.Text.RegularExpressions;

namespace SAPSec.Web.Controllers;

[Authorize]
[Route("find-a-school")]
public class SchoolSearchController(
    ILogger<SchoolSearchController> logger,
    ISchoolSearchService _searchService,
    GetSearchResultsUseCase useCase) : Controller
{
    private const int PageSize = 10;
    public const string Hint = "Search by name or school ID";
    public const string NoResultsErrorMessage = "We could not find any schools matching your search criteria";

    [HttpGet]
    public async Task<IActionResult> Index(
        [FromQuery] string? query,
        [FromQuery] string[]? localAuthorities,
        [FromQuery] int page = 1)
    {
        using (logger.BeginScope(new { query, page }))
        {
            var filters = new Dictionary<string, IEnumerable<string>>();
            filters["la"] = localAuthorities ?? [];
            var response = await useCase.Execute(new(query ?? "", filters, page));

            if (response.AllResults.Count() == 1 && (localAuthorities == null || localAuthorities.Length == 0))
            {
                return RedirectToAction("Index", "School", new { urn = response.AllResults.First().URN });
            }

            // Map all results for display
            var allSchoolsForMap = response.AllResults.Select(s => new SchoolSearchResultViewModel
            {
                SchoolName = s.EstablishmentName,
                URN = s.URN,
                LocalAuthority = s.LANAme,
                Latitude = s.Latitude,
                Longitude = s.Longitude,
                Address = string.Join(", ", new[]
                {
                    s.AddressStreet,
                    s.AddressLocality,
                    s.LANAme,
                    s.AddressPostcode
                }.Where(x => !string.IsNullOrWhiteSpace(x)))
            }).ToArray();

            return View(new SchoolSearchResultsViewModel
            {
                Query = query ?? string.Empty,
                LocalAuthorities = response.FilterOptions.FirstOrDefault(f => f.Key == "la")?.Options?.Select(o => o.Name).ToArray() ?? [],
                SelectedLocalAuthorities = response.FilterOptions.FirstOrDefault(f => f.Key == "la")?.Options?.Where(o => o.Selected).Select(o => o.Name).ToArray() ?? [],
                CurrentPage = page,
                PageSize = PageSize,
                TotalResults = response.ResultsPage.TotalCount,
                Results = response.ResultsPage.Select(s => new SchoolSearchResultViewModel
                {
                    SchoolName = s.EstablishmentName,
                    URN = s.URN,
                    LocalAuthority = s.LANAme,
                    Latitude = s.Latitude,
                    Longitude = s.Longitude,
                    Address = string.Join(", ", new[]
                    {
                        s.AddressStreet,
                        s.AddressLocality,
                        s.LANAme,
                        s.AddressPostcode
                    }.Where(x => !string.IsNullOrWhiteSpace(x)))
                }).ToArray(),
                AllResults = allSchoolsForMap
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Index(SchoolSearchQueryViewModel searchQueryViewModel)
    {
        using (logger.BeginScope(new { searchQueryViewModel }))
        {
            if (!ModelState.IsValid)
            {
                return View(new SchoolSearchResultsViewModel
                {
                    Query = searchQueryViewModel.Query
                });
            }

            var schoolNumber = SchoolNumberFrom(searchQueryViewModel);
            if (string.IsNullOrWhiteSpace(schoolNumber))
            {
                var routeValues = BuildSearchRouteValues(
                    searchQueryViewModel.Query);
                return RedirectToAction("Index", routeValues);
            }

            var school = await _searchService.SearchByNumberAsync(schoolNumber);
            if (!string.IsNullOrWhiteSpace(school?.URN))
            {
                return RedirectToAction("Index", "School", new
                {
                    school.URN
                });
            }

            ModelState.AddModelError("Query", "We could not find any schools matching your search criteria");
            var fallbackRouteValues = BuildSearchRouteValues(
                searchQueryViewModel.Query);
            return RedirectToAction("Index", fallbackRouteValues);
        }
    }

    [HttpGet("suggest")]
    public async Task<IActionResult> Suggest([FromQuery] string queryPart)
    {
        using (logger.BeginScope(new { queryPart }))
        {
            var suggestions = await _searchService.SuggestAsync(queryPart);

            return Ok(suggestions);
        }
    }

    private static RouteValueDictionary BuildSearchRouteValues(
        string? query)
    {
        var routeValues = new RouteValueDictionary
        {
            ["query"] = query
        };

        return routeValues;
    }

    private static string? SchoolNumberFrom(SchoolSearchQueryViewModel searchQueryViewModel) =>
        !string.IsNullOrWhiteSpace(searchQueryViewModel.Urn)
            ? searchQueryViewModel.Urn.Trim()
            : IsSchoolNumberCandidate(searchQueryViewModel.Query)
                ? searchQueryViewModel.Query.Trim()
                : null;

    private static bool IsSchoolNumberCandidate(string? value) =>
        !string.IsNullOrWhiteSpace(value) &&
        Regex.IsMatch(value.Trim(), @"^(\d+|\d+[\\/]\d+)$");
}
