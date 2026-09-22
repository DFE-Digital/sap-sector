using SAPSec.Web.Areas.Shared.ViewModels;
using SAPSec.Web.ViewModels.Measures;

namespace SAPSec.Web.Areas.AllThrough.ViewModels;

public class Ks4CoreSubjectsPageViewModel
{
    public required SchoolInfoViewModel School { get; set; }

    public required MeasureViewModel[] Measures { get; set; }
}
