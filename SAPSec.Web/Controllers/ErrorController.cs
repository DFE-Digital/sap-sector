using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Web.Constants;
using SAPSec.Web.Errors;

namespace SAPSec.Web.Controllers;

[Controller]
[Route("error")]
[AllowAnonymous]
public class ErrorController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly IErrorStore _errorStore;

    public ErrorController(IConfiguration configuration, IErrorStore errorStore)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _errorStore = errorStore ?? throw new ArgumentNullException(nameof(errorStore));
    }

    [HttpGet]
    [HttpPost]
    public IActionResult Problem(string? errorId = null)
    {
        if (IsDetailedErrorsEnabled(out var details, errorId))
        {
            Response.StatusCode = 500;
            return Content($"<h1>Exception details (temporary)</h1><pre>{System.Net.WebUtility.HtmlEncode(details)}</pre>", "text/html");
        }

        ViewData[ViewDataKeys.UseJsBackLink] = true;
        return View();
    }

    [HttpGet]
    [HttpPost]
    [Route("{statusCode:int}")]
    public IActionResult StatusCodeError(int statusCode, string? errorId = null)
    {
        if (IsDetailedErrorsEnabled(out var details, errorId))
        {
            Response.StatusCode = statusCode == 0 ? 500 : statusCode;
            return Content($"<h1>Exception details (temporary)</h1><pre>{System.Net.WebUtility.HtmlEncode(details)}</pre>", "text/html");
        }

        ViewData[ViewDataKeys.UseJsBackLink] = true;

        return statusCode switch
        {
            401 => View("AccessDenied"),
            404 => View("NotFound"),
            403 => View("AccessDenied"),
            _ => View("Problem")
        };
    }

    private bool IsDetailedErrorsEnabled(out string? details, string? errorId)
    {
        details = null;
        var showDetailed = true;
        if (!showDetailed) return false;

        if (!string.IsNullOrEmpty(errorId))
        {
            details = _errorStore.Get(errorId);
            return details != null;
        }

        var feature = HttpContext.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        if (feature?.Error != null)
        {
            details = _errorStore.Add(feature.Error);
            return true;
        }

        return false;
    }
}