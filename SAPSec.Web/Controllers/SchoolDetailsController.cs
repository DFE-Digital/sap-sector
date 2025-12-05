using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SAPSec.Web.Controllers;

[Authorize]
public class SchoolDetailsController(ILogger<SchoolDetailsController> logger) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        logger.LogInformation("User accessing School Details page");
        return View();
    }
}