using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Web.Constants;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Controllers;

[Authorize]
public class SchoolSearchController(
    ILogger<SchoolSearchController> logger,
    ISearchService _searchService) : Controller
{
    private const int PageSize = 10;

    [HttpGet]
    [Route("search-for-a-school")]
    public IActionResult Index() => View(new SchoolSearchQueryViewModel());

    [HttpPost]
    [Route("search-for-a-school")]
    public IActionResult Index(SchoolSearchQueryViewModel searchQueryViewModel)
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
                return RedirectToAction("Search", new
                {
                    query = searchQueryViewModel.Query
                });
            }

            var school = _searchService.SearchByNumber(searchQueryViewModel.Urn);
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
    [Route("school/search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? query,
        [FromQuery] string[]? localAuthorities,
        [FromQuery] int page = 1)
    {
        using (logger.BeginScope(new { query, page }))
        {
            if (page < 1) page = 1;

            var results = await _searchService.SearchAsync(query ?? string.Empty);

            var allLocalAuthorities = results
                .Select(s => s.Establishment.LANAme)
                .Where(la => !string.IsNullOrWhiteSpace(la))
                .Distinct()
                .OrderBy(la => la)
                .ToArray();

            if (localAuthorities != null && localAuthorities.Length > 0)
            {
                results = results
                    .Where(s => localAuthorities.Contains(s.Establishment.LANAme))
                    .ToList();
            }

            if (results.Count == 1 && (localAuthorities == null || localAuthorities.Length == 0))
            {
                return RedirectToAction("Index", "School", new { results[0].Establishment.URN });
            }

            var totalResults = results.Count;

            var pagedResults = results
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            var totalPages = (int)Math.Ceiling((double)totalResults / PageSize);
            if (page > totalPages && totalPages > 0)
            {
                return RedirectToAction("Search", new { query, localAuthorities, page = totalPages });
            }

            return View(new SchoolSearchResultsViewModel
            {
                Query = query ?? string.Empty,
                LocalAuthorities = allLocalAuthorities,
                SelectedLocalAuthorities = localAuthorities ?? Array.Empty<string>(),
                CurrentPage = page,
                PageSize = PageSize,
                TotalResults = totalResults,
                Results = pagedResults.Select(s => new SchoolSearchResultViewModel
                {
                    SchoolName = s.Establishment.EstablishmentName,
                    URN = s.Establishment.URN,
                    LocalAuthority = s.Establishment.LANAme,
                    Latitude = s.Establishment.Latitude,
                    Longitude = s.Establishment.Longitude,
                    Address = string.Join(", ", new[]
                    {
                        s.Establishment.AddressStreet,
                        s.Establishment.AddressLocality,
                        s.Establishment.LANAme,
                        s.Establishment.AddressPostcode
                    }.Where(x => !string.IsNullOrWhiteSpace(x)))
                }).ToArray()
            });
        }
    }

    [HttpPost]
    [Route("school/search")]
    public IActionResult Search(SchoolSearchQueryViewModel searchQueryViewModel)
    {
        using (logger.BeginScope(new { searchQueryViewModel }))
        {
            if (!ModelState.IsValid)
            {
                return View(new SchoolSearchResultsViewModel
                {
                    Query = searchQueryViewModel.Query,
                });
            }

            if (string.IsNullOrWhiteSpace(searchQueryViewModel.Urn))
            {
                return RedirectToAction("Search", searchQueryViewModel);
            }

            var school = _searchService.SearchByNumber(searchQueryViewModel.Urn);
            if (!string.IsNullOrWhiteSpace(school?.URN))
            {
                return RedirectToAction("Index", "School", new
                {
                    school.URN
                });
            }

            ModelState.AddModelError("Query", "We could not find any schools matching your search criteria");
            return RedirectToAction("Search", searchQueryViewModel);
        }
    }

    [HttpGet("school/suggest")]
    public async Task<IActionResult> Suggest([FromQuery] string queryPart)
    {
        using (logger.BeginScope(new { queryPart }))
        {
            var suggestions = await _searchService.SuggestAsync(queryPart);

            return Ok(suggestions);
        }
    }
}