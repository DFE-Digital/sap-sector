using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SAPSec.Web.Controllers;

[Controller]
[Route("/")]
[AllowAnonymous]
public class StaticContentController : Controller
{
    [HttpGet]
    [Route("accessibility")]
    public IActionResult Accessibility()
    {
        return View();
    }

    [HttpGet]
    [Route("terms-and-conditions")]
    public IActionResult TermsAndConditions()
    {
        return View();
    }

    [HttpGet]
    [Route("cookies")]
    public IActionResult Cookies([FromQuery] string? returnUrl = null)
    {
        ViewData["CookieReturnUrl"] = GetSafeReturnUrl(returnUrl)
            ?? GetSafeReturnUrl(Request.Headers.Referer.ToString())
            ?? Url.Content("~/");

        return View();
    }

    private string? GetSafeReturnUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        if (Url.IsLocalUrl(url))
        {
            return IsCookiesUrl(url) ? null : url;
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var absoluteUrl))
        {
            return null;
        }

        if (!string.Equals(absoluteUrl.Host, Request.Host.Host, StringComparison.OrdinalIgnoreCase)
            || (Request.Host.Port.HasValue && absoluteUrl.Port != Request.Host.Port.Value))
        {
            return null;
        }

        var localUrl = absoluteUrl.PathAndQuery;
        return Url.IsLocalUrl(localUrl) && !IsCookiesUrl(localUrl) ? localUrl : null;
    }

    private static bool IsCookiesUrl(string url)
    {
        var path = url.Split('?', '#')[0].TrimEnd('/');
        return string.Equals(path, "/cookies", StringComparison.OrdinalIgnoreCase);
    }
}
