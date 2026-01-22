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
    public IActionResult Index(string urn)
    {
        var school = _schoolDetailsService.TryGetByUrn(urn);

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
    public IActionResult SchoolDetails(string urn)
    {
        var school = _schoolDetailsService.TryGetByUrn(urn);
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
    public IActionResult WhatIsASimilarSchool(string urn)
    {
        var school = _schoolDetailsService.TryGetByUrn(urn);
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
    public IActionResult ViewSimilarSchools(string urn)
    {
        var school = _schoolDetailsService.TryGetByUrn(urn);
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
    public IActionResult Attendance(string urn)
    {
        var school = _schoolDetailsService.TryGetByUrn(urn);
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
    public IActionResult Ks4HeadlineMeasures(string urn)
    {
        var school = _schoolDetailsService.TryGetByUrn(urn);
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
    public IActionResult Ks4CoreSubjects(string urn)
    {
        var school = _schoolDetailsService.TryGetByUrn(urn);
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