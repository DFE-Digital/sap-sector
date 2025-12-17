using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Web.Constants;
using SAPSec.Web.Extensions;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Controllers;

[Route("school/{urn}")]
public class SchoolController(IEstablishmentService _establishmentService, ILogger<SchoolController> _logger) : Controller
{
    [HttpGet]
    public IActionResult Index(string urn)
    {
        ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);

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