using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Web.Constants;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Controllers;

[Route("school")]
public class SchoolSearchController(
    ILogger<SchoolSearchController> logger,
    ISearchService searchService) : Controller
{
    [HttpGet]
    public IActionResult Index() => View(new SchoolSearchQueryViewModel());

    [HttpPost]
    public IActionResult Index(SchoolSearchQueryViewModel searchQueryViewModel)
    {
        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome("123");

        if (!ModelState.IsValid)
        {
            return View(searchQueryViewModel);
        }

        //TODO: how do we redirect to exaxt school?
        // if (!string.IsNullOrWhiteSpace(searchQueryViewModel.EstablishmentId))
        // {
        //     return RedirectToAction("Index", "School", new
        //     {
        //         query = searchQueryViewModel.EstablishmentId
        //     });
        // }

        return RedirectToAction("Search", new
        {
            query = searchQueryViewModel.Query
        });
    }

    [HttpGet]
    [Route("search")]
    public async Task<IActionResult> Search([FromQuery] string? query)
    {
        using (logger.BeginScope(new { query }))
        {
            var results = await searchService.SearchAsync(query ?? string.Empty, 50);

            return View(new SchoolSearchResultsViewModel
                {
                    Query = query ?? string.Empty,
                    Results = results.Select(s => new SchoolSearchResultViewModel
                    {
                        SchoolName = s.Establishment.EstablishmentName,
                        URN = s.Establishment.EstablishmentNumber.ToString()
                    }).ToArray()
                }
            );
        }
    }

    [HttpPost]
    [Route("search")]
    public IActionResult Search(SchoolSearchQueryViewModel searchQueryViewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(new SchoolSearchResultsViewModel
            {
                Query = searchQueryViewModel.Query,
            });
        }

        if (!string.IsNullOrWhiteSpace(searchQueryViewModel.EstablishmentId))
        {
            return RedirectToAction("Index", "School", new
            {
                urn = searchQueryViewModel.EstablishmentId
            });
        }

        return RedirectToAction("Search", searchQueryViewModel);
    }

    [HttpGet("suggest")]
    public async Task<IActionResult> Suggest([FromQuery] string queryPart)
    {
        //TODO: do we need to configure this?
        var take = 10;

        var suggestions = await searchService.SuggestAsync(queryPart, take);

        return Ok(suggestions);
    }
}