using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SAPSec.Core.Features.SchoolSearch;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Web.Constants;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Controllers;

[Authorize]
[Route("find-a-school")]
public class SchoolSearchController(
    ILogger<SchoolSearchController> logger,
    ISchoolSearchService _searchService,
    ISimilarSchoolsSecondaryRepository? _similarSchoolsSecondaryRepository = null) : Controller
{
    private const int PageSize = 10;
    public const string Hint = "Search by name or school ID";
    public const string NoResultsErrorMessage = "We could not find any schools matching your search criteria";

    [HttpGet]
    public IActionResult Index() => View(new SchoolSearchQueryViewModel());

    [HttpPost]
    public async Task<IActionResult> Index(SchoolSearchQueryViewModel searchQueryViewModel)
    {
        using (logger.BeginScope(new { searchQueryViewModel }))
        {
            ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(searchQueryViewModel.Urn);

            if (!ModelState.IsValid)
            {
                return View(searchQueryViewModel);
            }

            if (string.IsNullOrWhiteSpace(searchQueryViewModel.Urn))
            {
                var routeValues = BuildSearchRouteValues(
                    searchQueryViewModel.Query,
                    searchQueryViewModel.SecondaryOnly,
                    searchQueryViewModel.SimilarSchoolsOnly);
                return RedirectToAction("Search", routeValues);
            }

            var school = await _searchService.SearchByNumberAsync(searchQueryViewModel.Urn);
            if (!string.IsNullOrWhiteSpace(school?.URN))
            {
                return RedirectToAction("Index", "School", new
                {
                    school?.URN
                });
            }

            ModelState.AddModelError("Query", "We could not find any schools matching your search criteria");
            return View(searchQueryViewModel);
        }
    }

    [HttpGet]
    [Route("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? query,
        [FromQuery] string[]? localAuthorities,
        [FromQuery] int page = 1,
        [FromQuery] bool? secondaryOnly = null,
        [FromQuery] bool? similarSchoolsOnly = null)
    {
        var applySecondaryOnly = secondaryOnly ?? false;
        var applySimilarSchoolsOnly = similarSchoolsOnly ?? false;

        using (logger.BeginScope(new { query, secondaryOnly = applySecondaryOnly, similarSchoolsOnly = applySimilarSchoolsOnly, page }))
        {
            if (page < 1) page = 1;

            var results = await _searchService.SearchAsync(query ?? string.Empty);

            var allLocalAuthorities = results
                .Select(s => s.LANAme)
                .Where(la => !string.IsNullOrWhiteSpace(la))
                .Distinct()
                .OrderBy(la => la)
                .ToArray();

            if (localAuthorities != null && localAuthorities.Length > 0)
            {
                results = results
                    .Where(s => localAuthorities.Contains(s.LANAme))
                    .ToList();
            }

            if (applySecondaryOnly)
            {
                results = results
                    .Where(s => IsSecondaryPhase(s.PhaseOfEducationName))
                    .ToList();
            }

            if (applySimilarSchoolsOnly)
            {
                var withSimilarSchools = await Task.WhenAll(results.Select(async s => new
                {
                    School = s,
                    HasSimilarSchools = await HasSimilarSchoolsAsync(s.URN)
                }));

                results = withSimilarSchools
                    .Where(x => x.HasSimilarSchools)
                    .Select(x => x.School)
                    .ToList();
            }

            if (results.Count == 1 && (localAuthorities == null || localAuthorities.Length == 0))
            {
                return RedirectToAction("Index", "School", new { results[0].URN });
            }

            var totalResults = results.Count;

            var pagedResults = results
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            var totalPages = (int)Math.Ceiling((double)totalResults / PageSize);
            if (page > totalPages && totalPages > 0)
            {
                var routeValues = BuildSearchRouteValues(query, applySecondaryOnly, applySimilarSchoolsOnly);
                routeValues["localAuthorities"] = localAuthorities;
                routeValues["page"] = totalPages;
                return RedirectToAction("Search", routeValues);
            }

            // Map all results for display
            var allSchoolsForMap = results.Select(s => new SchoolSearchResultViewModel
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
                LocalAuthorities = allLocalAuthorities,
                SelectedLocalAuthorities = localAuthorities ?? Array.Empty<string>(),
                SecondaryOnly = applySecondaryOnly,
                SimilarSchoolsOnly = applySimilarSchoolsOnly,
                CurrentPage = page,
                PageSize = PageSize,
                TotalResults = totalResults,
                Results = pagedResults.Select(s => new SchoolSearchResultViewModel
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
    [Route("search")]
    public async Task<IActionResult> Search(SchoolSearchQueryViewModel searchQueryViewModel)
    {
        using (logger.BeginScope(new { searchQueryViewModel }))
        {
            if (!ModelState.IsValid)
            {
                return View(new SchoolSearchResultsViewModel
                {
                    Query = searchQueryViewModel.Query,
                    SecondaryOnly = searchQueryViewModel.SecondaryOnly,
                    SimilarSchoolsOnly = searchQueryViewModel.SimilarSchoolsOnly
                });
            }

            if (string.IsNullOrWhiteSpace(searchQueryViewModel.Urn))
            {
                var routeValues = BuildSearchRouteValues(
                    searchQueryViewModel.Query,
                    searchQueryViewModel.SecondaryOnly,
                    searchQueryViewModel.SimilarSchoolsOnly);
                return RedirectToAction("Search", routeValues);
            }

            var school = await _searchService.SearchByNumberAsync(searchQueryViewModel.Urn);
            if (!string.IsNullOrWhiteSpace(school?.URN))
            {
                return RedirectToAction("Index", "School", new
                {
                    school.URN
                });
            }

            ModelState.AddModelError("Query", "We could not find any schools matching your search criteria");
            var fallbackRouteValues = BuildSearchRouteValues(
                searchQueryViewModel.Query,
                searchQueryViewModel.SecondaryOnly,
                searchQueryViewModel.SimilarSchoolsOnly);
            return RedirectToAction("Search", fallbackRouteValues);
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

    private static bool IsSecondaryPhase(string? phaseOfEducationName) =>
        !string.IsNullOrWhiteSpace(phaseOfEducationName) &&
        phaseOfEducationName.Contains("secondary", StringComparison.InvariantCultureIgnoreCase);

    private static RouteValueDictionary BuildSearchRouteValues(
        string? query,
        bool secondaryOnly,
        bool similarSchoolsOnly)
    {
        var routeValues = new RouteValueDictionary
        {
            ["query"] = query
        };

        if (secondaryOnly)
        {
            routeValues["secondaryOnly"] = true;
        }

        if (similarSchoolsOnly)
        {
            routeValues["similarSchoolsOnly"] = true;
        }

        return routeValues;
    }

    private async Task<bool> HasSimilarSchoolsAsync(string urn)
    {
        if (_similarSchoolsSecondaryRepository is null)
        {
            return true;
        }

        try
        {
            var similarSchoolUrns = await _similarSchoolsSecondaryRepository.GetSimilarSchoolUrnsAsync(urn);
            return similarSchoolUrns.Any();
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Unable to fetch similar school URNs for search result URN {Urn}", urn);
            return false;
        }
    }
}

