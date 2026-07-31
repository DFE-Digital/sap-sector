using SAPSec.Core.Features.Measures;

namespace SAPSec.Web.Areas.Primary.ViewModels;

public class PrimarySimilarSchoolsComparisonViewModel
{
    public required string Urn { get; set; }
    public required string SimilarSchoolUrn { get; set; }
    public required string Name { get; set; }
    public required string SimilarSchoolName { get; set; }
    public Measure? MeetingExpectedStandardRwm { get; set; }
    public Measure? AchievedHigherStandardRwm { get; set; }
}
