using SAPSec.Web.ViewModels.Measures;

namespace SAPSec.Web.Areas.Primary.ViewModels.Comparison;

public class Ks2PerformanceMeasuresPageViewModel
{
    public required string Urn { get; set; }
    public required string Name { get; set; }
    public required string SimilarSchoolUrn { get; set; }
    public required string SimilarSchoolName { get; set; }

    public required MeasureViewModel MeetingExpectedStandardRwm { get; set; }
    public required MeasureViewModel AchievedHigherStandardRwm { get; set; }
    public required MeasureViewModel AverageScaledScoreReading { get; set; }
    public required MeasureViewModel AverageScaledScoreMaths { get; set; }
    public required MeasureViewModel MeetingExpectedStandardGps { get; set; }
    public required MeasureViewModel AchievedHigherStandardGps { get; set; }
}
