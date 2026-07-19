using SAPSec.Web.ViewModels;
using SAPSec.Web.ViewModels.Measures;

namespace SAPSec.Web.Areas.Primary.ViewModels;

public class AttendanceMeasuresPageViewModel
{
    public required SchoolInfoViewModel School { get; set; }
    public required MeasureViewModel Attendance { get; set; }
   // public required MeasureViewModel PersistentAbsence { get; set; }
}
