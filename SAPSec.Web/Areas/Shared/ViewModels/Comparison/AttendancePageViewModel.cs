using SAPSec.Web.ViewModels.Measures;

namespace SAPSec.Web.Areas.Shared.ViewModels.Comparison;

public class AttendancePageViewModel
{
    public required SchoolViewModel CurrentSchool { get; set; }
    public required SchoolViewModel ComparatorSchool { get; set; }

    public required MeasureViewModel Absence { get; set; }
}
