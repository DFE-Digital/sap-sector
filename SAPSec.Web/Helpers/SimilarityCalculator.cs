using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Helpers;

public static class SimilarityCalculator
{
    public static SimilarSchoolsComparisonViewModel.SimilarityLabel Calculate(decimal xA, decimal xB, decimal sdNational)
    {
        if (sdNational <= 0)
            return SimilarSchoolsComparisonViewModel.SimilarityLabel.NotSimilar;

        var d = (xA - xB) / sdNational;
        var absD = Math.Abs(d);

        if (absD <= 0.3m) return SimilarSchoolsComparisonViewModel.SimilarityLabel.Similar;
        if (absD <= 0.7m) return SimilarSchoolsComparisonViewModel.SimilarityLabel.LessSimilar;
        return SimilarSchoolsComparisonViewModel.SimilarityLabel.NotSimilar;
    }
}