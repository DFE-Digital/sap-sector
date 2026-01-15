using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Web.Constants;
using SAPSec.Web.Extensions;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Controllers;

[Route("school/{urn}")]
public class SchoolController(
    IEstablishmentService _establishmentService,
    ILogger<SchoolController> _logger) : Controller
{
    [HttpGet]
    public IActionResult Index(string urn)
    {
        var school = _establishmentService.GetEstablishment(urn);
        if (school != null)
        {
            var viewModel = new SchoolViewModel(school);
            return View(viewModel);
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
        var school = _establishmentService.GetEstablishment(urn);
        if (school != null)
        {
            var viewModel = new SchoolViewModel(school);
            return View(viewModel);
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
        var school = _establishmentService.GetEstablishment(urn);
        if (school != null)
        {
            var viewModel = new SchoolViewModel(school);
            return View(viewModel);
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
        var school = _establishmentService.GetEstablishment(urn);
        if (school != null)
        {
            var viewModel = new SchoolViewModel(school);
            return View(viewModel);
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
        var school = _establishmentService.GetEstablishment(urn);
        if (school != null)
        {
            var viewModel = new SchoolViewModel(school);
            return View(viewModel);
        }
        else
        {
            _logger.LogInformation($"{urn} was not found on School Controller");
            return RedirectToAction("Error");
        }
    }

    [HttpGet]
    [Route("school-details")]
    public IActionResult SchoolDetails(string urn)
    {
        var school = _establishmentService.GetEstablishment(urn);
        if (school != null)
        {
            var viewModel = new SchoolViewModel(school);
            return View(viewModel);
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
        var school = _establishmentService.GetEstablishment(urn);
        if (school != null)
        {
            var viewModel = new SchoolViewModel(school);
            return View(viewModel);
        }
        else
        {
            _logger.LogInformation($"{urn} was not found on School Controller");
            return RedirectToAction("Error");
        }
    }
}