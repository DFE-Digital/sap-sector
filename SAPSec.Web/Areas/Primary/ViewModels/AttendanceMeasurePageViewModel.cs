using SAPSec.Web.ViewModels;
using SAPSec.Web.ViewModels.Measures;

namespace SAPSec.Web.Areas.Primary.ViewModels;

public class AttendanceMeasurePageViewModel
{
    public required SchoolInfoViewModel School { get; set; }
    public required MeasureViewModel TotalAbsence { get; set; }
    public required MeasureViewModel PersistentAbsence { get; set; }
}
