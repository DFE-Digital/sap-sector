using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.FeatureFlags;
using SAPSec.Web.Filters;
using SAPSec.Web.ViewModels.ChartExamples;

namespace SAPSec.Web.Controllers;

/// <summary>
/// Example chart pages built from made-up data, so product and design can review how charts behave
/// for edge cases before real data is wired in. Feature flagged and off in production.
/// </summary>
[Authorize]
[RequireFeatureFlag(Flags.EnableChartExamples)]
[Route("chart-examples")]
public class ChartExamplesController : Controller
{
    [HttpGet("ks2-progress")]
    public IActionResult Ks2Progress() => View(Ks2ProgressChartExamplesViewModel.Create());
}
