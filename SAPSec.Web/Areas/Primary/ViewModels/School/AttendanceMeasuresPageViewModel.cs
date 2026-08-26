using SAPSec.Web.Areas.Shared.ViewModels;
using SAPSec.Web.ViewModels.Measures;

namespace SAPSec.Web.Areas.Primary.ViewModels.School;

public class AttendanceMeasuresPageViewModel
{
    public required SchoolInfoViewModel School { get; set; }
    public required MeasureViewModel Absence { get; set; }

}
