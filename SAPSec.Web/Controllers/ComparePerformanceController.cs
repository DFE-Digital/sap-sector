using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SAPSec.Web.Controllers;

[Authorize]
public class ComparePerformanceController : Controller
{
    private readonly ILogger<ComparePerformanceController> _logger;

    public ComparePerformanceController(ILogger<ComparePerformanceController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        _logger.LogInformation("User accessing Compare Performance page");
        return View();
    }
}