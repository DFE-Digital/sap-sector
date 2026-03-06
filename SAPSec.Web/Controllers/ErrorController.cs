using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Web.Constants;

namespace SAPSec.Web.Controllers;

[Controller]
[Route("error")]
public class ErrorController(IWebHostEnvironment environment) : Controller
{
    [HttpGet]
    [HttpPost]
    public IActionResult Problem()
    {
        ViewData[ViewDataKeys.UseJsBackLink] = true;
        if (!environment.IsProduction())
        {
            var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            if (exceptionFeature?.Error is Exception ex)
            {
                ViewData["ErrorDetail"] = $"{ex.GetType().Name}: {ex.Message}";
            }
        }

        return View();
    }

    [HttpGet]
    [HttpPost]
    [Route("{statusCode:int}")]
    public IActionResult StatusCodeError(int statusCode)
    {
        ViewData[ViewDataKeys.UseJsBackLink] = true;
        if (!environment.IsProduction() && HttpContext.Items.TryGetValue("ErrorDetail", out var detail))
        {
            ViewData["ErrorDetail"] = detail?.ToString();
        }

        return statusCode switch
        {
            401 => View("AccessDenied"),
            404 => View("NotFound"),
            403 => View("AccessDenied"),
            _ => View("Problem")
        };
    }
}
