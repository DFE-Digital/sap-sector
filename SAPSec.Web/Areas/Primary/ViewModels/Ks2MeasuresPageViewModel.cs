using SAPSec.Web.ViewModels;
using SAPSec.Web.ViewModels.Measures;

namespace SAPSec.Web.Areas.Primary.ViewModels;

public class Ks2MeasuresPageViewModel
{
    public required SchoolInfoViewModel School { get; set; }

    public required MeasureViewModel MeetingExpectedStandardRwm { get; set; }
}
