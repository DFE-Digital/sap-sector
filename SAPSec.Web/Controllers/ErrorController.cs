using Microsoft.AspNetCore.Mvc;
using SAPSec.Web.Constants;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Controllers;

[Controller]
[Route("error")]
public class ErrorController : Controller
{
    [HttpGet]
    [HttpPost]
    public IActionResult Problem()
    {
        ViewData[ViewDataKeys.UseJsBackLink] = true;

        return View(ErrorModel);
    }

    [HttpGet]
    [HttpPost]
    [Route("{statusCode:int}")]
    public IActionResult StatusCodeError(int statusCode)
    {
        ViewData[ViewDataKeys.UseJsBackLink] = true;

        return statusCode switch
        {
            401 => View("AccessDenied"),
            404 => View("NotFound", ErrorModel),
            403 => View("AccessDenied"),
            _ => View("Problem", ErrorModel)
        };
    }

    private ErrorViewModel ErrorModel => new()
    {
        ErrorCode = HttpContext.TraceIdentifier,
        ErrorMessage = HttpContext.Items.TryGetValue("ErrorMessage", out object? o) && o is string message ? message : null
    };
}
