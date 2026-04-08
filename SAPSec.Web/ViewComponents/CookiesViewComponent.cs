using Microsoft.AspNetCore.Mvc;
using SAPSec.Web.Constants;
using SAPSec.Web.ViewModels.Components;

namespace SAPSec.Web.ViewComponents;

public class CookiesViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        var cookieValue = HttpContext.Request.Cookies[LayoutConstants.CookieSettingsName];
        var bannerState = HttpContext.Request.Query["cookie-banner"].ToString().ToLowerInvariant();

        if (!string.IsNullOrEmpty(cookieValue) &&
            bannerState is not "accepted" and not "rejected")
        {
            return new EmptyContentView();
        }

        if (string.IsNullOrEmpty(cookieValue))
        {
            bannerState = "unselected";
        }

        return View(new CookiesViewModel(LayoutConstants.CookieSettingsName, bannerState));
    }
}
