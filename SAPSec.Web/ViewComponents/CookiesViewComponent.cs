using Microsoft.AspNetCore.Mvc;
using SAPSec.Web.Constants;
using SAPSec.Web.ViewModels.Components;

namespace SAPSec.Web.ViewComponents;

public class CookiesViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        if (HttpContext.Request.Cookies.ContainsKey(LayoutConstants.CookieSettingsName))
        {
            return new EmptyContentView();
        }

        return View(new CookiesViewModel(LayoutConstants.CookieSettingsName));
    }
}