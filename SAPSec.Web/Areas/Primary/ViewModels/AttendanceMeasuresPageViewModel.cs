using SAPSec.Web.ViewModels;
using SAPSec.Web.ViewModels.Measures;

namespace SAPSec.Web.Areas.Primary.ViewModels;

public class AttendanceMeasuresPageViewModel
{
    public required SchoolInfoViewModel School { get; set; }
    public required MeasureViewModel Absence { get; set; }

}
