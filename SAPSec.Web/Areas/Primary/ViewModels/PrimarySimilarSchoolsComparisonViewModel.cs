using SAPSec.Web.ViewModels.Measures;

namespace SAPSec.Web.Areas.Primary.ViewModels;

public class PrimarySimilarSchoolsComparisonViewModel
{
    public required string Urn { get; set; }
    public required string SimilarSchoolUrn { get; set; }
    public required string Name { get; set; }
    public required string SimilarSchoolName { get; set; }
    public MeasureViewModel? MeetingExpectedStandardRwm { get; set; }
    public MeasureViewModel? AchievedHigherStandardRwm { get; set; }
    public MeasureViewModel? AverageScaledScoreReading { get; set; }
}
