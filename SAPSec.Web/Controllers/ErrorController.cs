using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Web.Constants;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Controllers;

[Controller]
[Route("error")]
[AllowAnonymous]
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
        ErrorCode = HttpContext.Items.TryGetValue("ErrorCode", out object? oc) && oc is string code ? code : null,
        ErrorMessage = HttpContext.Items.TryGetValue("ErrorMessage", out object? om) && om is string message ? message : null
    };
}
