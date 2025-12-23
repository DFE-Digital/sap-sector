using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SAPSec.Web.Controllers;

[Authorize]
public class ConnectSchoolController(ILogger<ConnectSchoolController> logger) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        logger.LogInformation("User accessing Change School page");
        return View();
    }
}