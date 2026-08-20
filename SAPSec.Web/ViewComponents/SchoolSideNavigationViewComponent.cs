using Microsoft.AspNetCore.Mvc;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.ViewComponents;

public class SchoolSideNavigationViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(SchoolSideNavigationViewModel model)
    {
        return View(model);
    }
}
