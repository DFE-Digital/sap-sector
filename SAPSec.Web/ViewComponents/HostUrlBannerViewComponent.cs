using Microsoft.AspNetCore.Mvc;

namespace SAPSec.Web.ViewComponents;

public class HostUrlBannerViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View();
    }
}