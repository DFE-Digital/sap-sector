using SAPSec.Core.Features.SimilarSchools;
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
    public MeasureViewModel? AverageScaledScoreMaths { get; set; }
    public MeasureViewModel? MeetingExpectedStandardGps { get; set; }
    public MeasureViewModel? AchievedHigherStandardGps { get; set; }

    public MeasureViewModel? Absence { get; set; }

    public IReadOnlyList<CharacteristicRow> CharacteristicsRows { get; set; }
        = Array.Empty<CharacteristicRow>();

    public sealed class CharacteristicRow
    {
        public required string Characteristic { get; init; }
        public required string CurrentSchoolValue { get; init; }
        public required string SimilarSchoolValue { get; init; }
        public bool IsNumeric { get; init; }

        public SchoolSimilarity? Similarity { get; init; }
    }
}
