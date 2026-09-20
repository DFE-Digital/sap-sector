namespace SAPSec.Web.Areas.Shared.ViewModels.Comparison;

public class SimilarityPageViewModel
{
    public required SchoolInfoViewModel CurrentSchool { get; set; }
    public required SchoolInfoViewModel ComparatorSchool { get; set; }

    public required IReadOnlyList<CharacteristicRow> CharacteristicsRows { get; set; }

    public sealed class CharacteristicRow
    {
        public required string Characteristic { get; init; }
        public required string CurrentSchoolValue { get; init; }
        public required string ComparatorSchoolValue { get; init; }
    }
}