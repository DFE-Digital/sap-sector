using SAPSec.Web.Areas.Shared.ViewModels;
using SAPSec.Web.ViewModels.Measures;

namespace SAPSec.Web.Areas.Secondary.ViewModels.Comparison;

public class Ks4CoreSubjectsPageViewModel
{
    public required SchoolViewModel CurrentSchool { get; set; }
    public required SchoolViewModel ComparatorSchool { get; set; }

    public required MeasureViewModel[] Measures { get; set; }
}
