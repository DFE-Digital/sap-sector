using SAPSec.Core.Features.SimilarSchools;

namespace SAPSec.Web.Areas.Primary.ViewModels.Comparison;

public class SimilarityPageViewModel
{
    public required string Urn { get; set; }
    public required string Name { get; set; }
    public required string SimilarSchoolUrn { get; set; }
    public required string SimilarSchoolName { get; set; }

    public required IReadOnlyList<CharacteristicRow> CharacteristicsRows { get; set; }

    public sealed class CharacteristicRow
    {
        public required string Characteristic { get; init; }
        public required string CurrentSchoolValue { get; init; }
        public required string SimilarSchoolValue { get; init; }
        public bool IsNumeric { get; init; }

        public SchoolSimilarity? Similarity { get; init; }
    }
}