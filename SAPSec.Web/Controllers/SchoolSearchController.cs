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
    public async Task<IActionResult> Search([FromQuery] string? query)
    {
        using (logger.BeginScope(new { query }))
        {
            var results = await _searchService.SearchAsync(query ?? string.Empty);

            if (results.Count == 1)
            {
                return RedirectToAction("Index", "School", new { results[0].Establishment.URN });
            }

            return View(new SchoolSearchResultsViewModel
                {
                    Query = query ?? string.Empty,
                    Results = results.Select(s => new SchoolSearchResultViewModel
                    {
                        SchoolName = s.Establishment.EstablishmentName,
                        URN = s.Establishment.URN,
                        LocalAuthority = s.Establishment.LANAme
                    }).ToArray()
                }
            );
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