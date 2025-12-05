using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SAPSec.Web.Controllers;

[Authorize]
public class SchoolDetailsController : Controller
{
    private readonly ILogger<SchoolDetailsController> _logger;

    public SchoolDetailsController(ILogger<SchoolDetailsController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        _logger.LogInformation("User accessing School Details page");
        return View();
    }
}