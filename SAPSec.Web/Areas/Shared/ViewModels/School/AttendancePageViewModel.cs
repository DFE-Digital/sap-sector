using SAPSec.Web.ViewModels.Measures;

namespace SAPSec.Web.Areas.Shared.ViewModels.School;

public class AttendancePageViewModel
{
    public required SchoolViewModel School { get; set; }
    public required MeasureViewModel Absence { get; set; }

}
