using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Web.Areas.Shared.ViewModels.Comparison;

namespace SAPSec.Web.Formatters;

public interface IPrimaryCharacteristicsComparisonFormatter
{
    IReadOnlyList<SimilarityPageViewModel.CharacteristicRow> BuildRows(
        PrimaryComparisonSimilarityCharacteristics characteristicts);
}
