using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SAPSec.Web.Controllers;

[Authorize]
public class ChangeSchoolController(ILogger<ChangeSchoolController> logger) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        logger.LogInformation("User accessing Change School page");
        return View();
    }
}