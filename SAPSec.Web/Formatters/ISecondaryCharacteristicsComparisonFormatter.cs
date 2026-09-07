using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Web.Areas.Shared.ViewModels.Comparison;

namespace SAPSec.Web.Formatters;

public interface ISecondaryCharacteristicsComparisonFormatter
{
    IReadOnlyList<SimilarityPageViewModel.CharacteristicRow> BuildRows(
        SecondaryComparisonSimilarityCharacteristics characteristics);
}
