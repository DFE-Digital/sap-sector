using Microsoft.AspNetCore.Mvc;
using SAPSec.Web.ViewModels.Components;

namespace SAPSec.Web.ViewComponents;

public class PhaseBannerViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        var vm = new PhaseBannerViewModel();

        // if (UserClaimsPrincipal.Claims.Any())
        // {
        //     vm.Organisation = UserClaimsPrincipal.Organisation().Name;
        // }

        return View(vm);
    }
}