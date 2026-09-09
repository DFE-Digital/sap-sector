using SAPSec.Web.ViewModels.Measures;

namespace SAPSec.Web.Areas.Shared.ViewModels.School;

public class AttendancePageViewModel
{
    public required SchoolInfoViewModel School { get; set; }
    public required MeasureViewModel Absence { get; set; }

}
