using SAPSec.Web.ViewModels.Measures;

namespace SAPSec.Web.Areas.Shared.ViewModels.Comparison;

public class AttendancePageViewModel
{
    public required SchoolInfoViewModel CurrentSchool { get; set; }
    public required SchoolInfoViewModel ComparatorSchool { get; set; }

    public required MeasureViewModel Absence { get; set; }
}
