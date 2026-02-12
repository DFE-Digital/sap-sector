using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Services;
using SAPSec.Web.Constants;
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
    private readonly ILogger<SchoolController> _logger;

    public SchoolController(
        ISchoolDetailsService schoolDetailsService,
        ILogger<SchoolController> logger)
    {
        _schoolDetailsService = schoolDetailsService;
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
            return View(school);
        }
        else
        {
            _logger.LogInformation($"{urn} was not found on School Controller");
            return RedirectToAction("Error");
        }
    }

    [HttpGet]
    [Route("what-is-a-similar-school")]
    public async Task<IActionResult> WhatIsASimilarSchool(string urn)
    {
        var school = await _schoolDetailsService.TryGetByUrnAsync(urn);
        if (school != null)
        {
            ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
            return View(school);
        }
        else
        {
            _logger.LogInformation($"{urn} was not found on School Controller");
            return RedirectToAction("Error");
        }
    }

    [HttpGet]
    [Route("view-similar-schools")]
    public async Task<IActionResult> ViewSimilarSchools(string urn)
    {
        var school = await _schoolDetailsService.TryGetByUrnAsync(urn);
        if (school != null)
        {
            ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
            return View(school);
        }
        else
        {
            _logger.LogInformation($"{urn} was not found on School Controller");
            return RedirectToAction("Error");
        }
    }

    [HttpGet]
    [Route("attendance")]
    public async Task<IActionResult> Attendance(string urn)
    {
        var school = await _schoolDetailsService.TryGetByUrnAsync(urn);
        if (school != null)
        {
            ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
            return View(school);
        }
        else
        {
            _logger.LogInformation($"{urn} was not found on School Controller");
            return RedirectToAction("Error");
        }
    }

    [HttpGet]
    [Route("ks4-headline-measures")]
    public async Task<IActionResult> Ks4HeadlineMeasures(string urn)
    {
        var school = await _schoolDetailsService.TryGetByUrnAsync(urn);
        if (school != null)
        {
            ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
            return View(school);
        }
        else
        {
            _logger.LogInformation($"{urn} was not found on School Controller");
            return RedirectToAction("Error");
        }
    }

    [HttpGet]
    [Route("ks4-core-subjects")]
    public async Task<IActionResult> Ks4CoreSubjects(string urn)
    {
        var school = await _schoolDetailsService.TryGetByUrnAsync(urn);
        if (school != null)
        {
            ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);
            return View(school);
        }
        else
        {
            _logger.LogInformation($"{urn} was not found on School Controller");
            return RedirectToAction("Error");
        }
    }

}