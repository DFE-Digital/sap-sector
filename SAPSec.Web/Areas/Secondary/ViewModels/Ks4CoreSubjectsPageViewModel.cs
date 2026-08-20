using SAPSec.Web.ViewModels;
using SAPSec.Web.ViewModels.Measures;

namespace SAPSec.Web.Areas.Secondary.ViewModels;

public class Ks4CoreSubjectsPageViewModel
{
    public required SchoolInfoViewModel School { get; set; }

    public required MeasureViewModel[] Measures { get; set; }
}
