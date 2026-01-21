using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Web.Constants;

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
}