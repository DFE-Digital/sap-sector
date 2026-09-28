using SAPSec.Web.Areas.Shared.ViewModels;
using SAPSec.Web.ViewModels.Measures;

namespace SAPSec.Web.Areas.AllThrough.ViewModels.School;

public class AllThroughAttendancePageViewModel
{
    public required SchoolInfoViewModel School { get; set; }
    public required MeasureViewModel PrimaryAbsence { get; set; }
    public required MeasureViewModel SecondaryAbsence { get; set; }
}
