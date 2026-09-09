using SAPSec.Web.Areas.Shared.ViewModels;
using SAPSec.Web.ViewModels.Measures;

namespace SAPSec.Web.Areas.Secondary.ViewModels.School;

public class Ks4CoreSubjectsPageViewModel
{
    public required SchoolInfoViewModel School { get; set; }

    public required MeasureViewModel[] Measures { get; set; }
}
