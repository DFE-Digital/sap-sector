using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SAPSec.Web.Controllers;

[Authorize]
public class ChangeSchoolController : Controller
{
    private readonly ILogger<ChangeSchoolController> _logger;

    public ChangeSchoolController(ILogger<ChangeSchoolController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        _logger.LogInformation("User accessing Change School page");
        return View();
    }
}