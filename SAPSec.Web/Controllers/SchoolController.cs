using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Infrastructure.Interfaces;
using SAPSec.Web.Constants;
using SAPSec.Web.Extensions;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Controllers;

[Route("school/{urn}")]
public class SchoolController(
    ISchoolRepository repository,
    ILogger<SchoolController> logger) : Controller
{
    [HttpGet]
    public IActionResult Index(string urn)
    {
        using (logger.BeginScope(new { urn }))
        {
            try
            {
                ViewData[ViewDataKeys.BreadcrumbNode] = BreadcrumbNodes.SchoolHome(urn);

                var school = repository.GetSchoolByUrn(int.Parse(urn));
                var viewModel = new SchoolViewModel(school);
                return View(viewModel);
            }
            catch (Exception e)
            {
                logger.LogError(e, "An error displaying school details: {DisplayUrl}", Request.GetDisplayUrl());
                return e is StatusCodeException s ? StatusCode((int)s.Status) : StatusCode(500);
            }
        }
    }
}