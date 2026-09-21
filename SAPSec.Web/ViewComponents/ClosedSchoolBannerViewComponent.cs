using Microsoft.AspNetCore.Mvc;
using SAPSec.Web.ViewModels.Components;

namespace SAPSec.Web.ViewComponents;

public class ClosedSchoolBannerViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(
        ClosedSchoolBannerVariant variant = ClosedSchoolBannerVariant.ThisSchool,
        IReadOnlyList<SuccessorLinkViewModel>? successors = null)
    {
        return View(new ClosedSchoolBannerViewModel(variant, successors ?? []));
    }
}
