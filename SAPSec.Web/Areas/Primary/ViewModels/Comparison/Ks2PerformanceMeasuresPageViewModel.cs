using SAPSec.Web.Areas.Shared.ViewModels;
using SAPSec.Web.ViewModels.Measures;

namespace SAPSec.Web.Areas.Primary.ViewModels.Comparison;

public class Ks2PerformanceMeasuresPageViewModel
{
    public required SchoolInfoViewModel CurrentSchool { get; set; }
    public required SchoolInfoViewModel ComparatorSchool { get; set; }

    public required MeasureViewModel MeetingExpectedStandardRwm { get; set; }
    public required MeasureViewModel AchievedHigherStandardRwm { get; set; }
    public required MeasureViewModel AverageScaledScoreReading { get; set; }
    public required MeasureViewModel AverageScaledScoreMaths { get; set; }
    public required MeasureViewModel MeetingExpectedStandardGps { get; set; }
    public required MeasureViewModel AchievedHigherStandardGps { get; set; }
}
