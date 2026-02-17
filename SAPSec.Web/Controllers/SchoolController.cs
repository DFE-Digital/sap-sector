using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Web.Constants;
using SAPSec.Web.Helpers;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Controllers;

/// <summary>
/// Controller for school details pages.
/// Single Responsibility: HTTP handling and view selection only.
/// </summary>
[Route("school/{urn}")]
public class SchoolController : Controller
{
    private readonly ISchoolDetailsService _schoolDetailsService;
    private readonly FindSimilarSchools _findSimilarSchools;
    private readonly ILogger<SchoolController> _logger;

    public SchoolController(
        ISchoolDetailsService schoolDetailsService,
        FindSimilarSchools findSimilarSchools,
        ILogger<SchoolController> logger)
    {
        _schoolDetailsService = schoolDetailsService;
        _findSimilarSchools = findSimilarSchools;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string urn)
    {
        var school = await _schoolDetailsService.TryGetByUrnAsync(urn);

        if (school is null)
        {
            _logger.LogInformation("School with URN {Urn} was not found", urn);
            return NotFound();
        }

        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
        SetSchoolViewData(school);
        return View(school);
    }

    [HttpGet]
    [Route("school-details")]
    public async Task<IActionResult> SchoolDetails(string urn)
    {
        var school = await _schoolDetailsService.TryGetByUrnAsync(urn);
        if (school != null)
        {
            ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
            SetSchoolViewData(school);
            return View(school);
        }

        _logger.LogInformation("{Urn} was not found on School Controller", urn);
        return RedirectToAction("Error");
    }

    [HttpGet]
    [Route("what-is-a-similar-school")]
    public async Task<IActionResult> WhatIsASimilarSchool(string urn)
    {
        var school = await _schoolDetailsService.TryGetByUrnAsync(urn);
        if (school != null)
        {
            ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
            SetSchoolViewData(school);
            return View(school);
        }

        _logger.LogInformation("{Urn} was not found on School Controller", urn);
        return RedirectToAction("Error");
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
            _logger.LogInformation("{Urn} was not found on School Controller", urn);
            return RedirectToAction("Error");
        }

        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
        SetSchoolViewData(school);

        var coreSortBy = string.IsNullOrWhiteSpace(sortBy) ? "Att8" : sortBy;
        var filterBy = BuildCoreFilters(Request.Query);

        var response = await _findSimilarSchools.Execute(new FindSimilarSchoolsRequest(
            urn,
            filterBy,
            coreSortBy,
            page));

        var schools = response.ResultsPage
            .Select(r => MapToViewModel(r))
            .ToList();

        var allSchools = response.AllResults
            .Select(r => MapToViewModel(r))
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
            CurrentFilters = ExtractCurrentFilters(Request.Query),
            SortBy = coreSortBy,
            CurrentPage = response.ResultsPage.CurrentPage,
            PageSize = response.ResultsPage.PageSize,
            TotalResults = response.AllResults.Count
        };

        return View(viewModel);
    }

    [HttpGet]
    [Route("attendance")]
    public async Task<IActionResult> Attendance(string urn)
    {
        var school = await _schoolDetailsService.TryGetByUrnAsync(urn);
        if (school != null)
        {
            ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
            SetSchoolViewData(school);
            return View(school);
        }

        _logger.LogInformation("{Urn} was not found on School Controller", urn);
        return RedirectToAction("Error");
    }

    [HttpGet]
    [Route("ks4-headline-measures")]
    public async Task<IActionResult> Ks4HeadlineMeasures(string urn)
    {
        var school = await _schoolDetailsService.TryGetByUrnAsync(urn);
        if (school != null)
        {
            ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
            SetSchoolViewData(school);
            return View(school);
        }

        _logger.LogInformation("{Urn} was not found on School Controller", urn);
        return RedirectToAction("Error");
    }

    [HttpGet]
    [Route("ks4-core-subjects")]
    public async Task<IActionResult> Ks4CoreSubjects(string urn)
    {
        var school = await _schoolDetailsService.TryGetByUrnAsync(urn);
        if (school != null)
        {
            ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
            SetSchoolViewData(school);
            return View(school);
        }

        _logger.LogInformation("{Urn} was not found on School Controller", urn);
        return RedirectToAction("Error");
    }

    private static Dictionary<string, IEnumerable<string>> BuildCoreFilters(IQueryCollection query)
    {
        return query
            .Where(kvp => kvp.Key != "sortBy" && kvp.Key != "page")
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.AsEnumerable(),
                StringComparer.InvariantCultureIgnoreCase);
    }
    private static Dictionary<string, List<string>> ExtractCurrentFilters(IQueryCollection query)
    {
        var result = new Dictionary<string, List<string>>(StringComparer.InvariantCultureIgnoreCase);
        foreach (var (key, values) in query)
        {
            if (key == "sortBy" || key == "page") continue;
            result[key] = values.ToList();
        }

        return result;
    }

    private static SimilarSchoolViewModel MapToViewModel(SimilarSchoolResult result)
    {
        var school = result.SimilarSchool;
        var address = school.Address;

        return new SimilarSchoolViewModel
        {
            Urn = int.TryParse(school.URN, out var urn) ? urn : 0,
            EstablishmentName = school.Name,
            Street = address.Street,
            Town = address.Town,
            Postcode = address.Postcode,
            Latitude = result.Coordinates?.Latitude.ToString(),
            Longitude = result.Coordinates?.Longitude.ToString(),
            UrbanOrRural = school.UrbanRuralName,
            Att8Scr = school.Attainment8Score.HasValue ? (double?)school.Attainment8Score.Value : null
        };
    }

    private void SetSchoolViewData(Core.Model.SchoolDetails school)
    {
        ViewData["SchoolDetails"] = school;
    }
}
