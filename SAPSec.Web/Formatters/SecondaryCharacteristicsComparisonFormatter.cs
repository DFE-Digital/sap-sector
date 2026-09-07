using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Web.Areas.Shared.ViewModels.Comparison;
using System.Globalization;

namespace SAPSec.Web.Formatters;

public sealed class SecondaryCharacteristicsComparisonFormatter : ISecondaryCharacteristicsComparisonFormatter
{
    public IReadOnlyList<SimilarityPageViewModel.CharacteristicRow> BuildRows(
       SecondaryComparisonSimilarityCharacteristics characteristics)
    {
        return new List<SimilarityPageViewModel.CharacteristicRow>(9)
    {
        new()
        {
            Characteristic = "Average KS2 reading and maths score",
            CurrentSchoolValue = Ks2Int(characteristics.Ks2AverageScore.CurrentSchoolValue),
            SimilarSchoolValue = Ks2Int(characteristics.Ks2AverageScore.ComparatorSchoolValue),
            IsNumeric = true,
            Similarity = characteristics.Ks2AverageScore.Similarity
        },
        new()
        {
            Characteristic = "Total number of pupils",
            CurrentSchoolValue = IntN0(characteristics.PupilCount.CurrentSchoolValue),
            SimilarSchoolValue = IntN0(characteristics.PupilCount.ComparatorSchoolValue),
            IsNumeric = true,
            Similarity = characteristics.PupilCount.Similarity
        },
        new()
        {
            Characteristic = "Pupil stability rate",
            CurrentSchoolValue = Percent1dp(characteristics.PupilStabilityRate.CurrentSchoolValue),
            SimilarSchoolValue = Percent1dp(characteristics.PupilStabilityRate.ComparatorSchoolValue),
            IsNumeric = true,
            Similarity = characteristics.PupilStabilityRate.Similarity
        },
        new()
        {
            Characteristic = "Eligibility for pupil premium",
            CurrentSchoolValue = Percent1dp(characteristics.PupilPremiumEligibilityPercentage.CurrentSchoolValue),
            SimilarSchoolValue = Percent1dp(characteristics.PupilPremiumEligibilityPercentage.ComparatorSchoolValue),
            IsNumeric = true,
            Similarity = characteristics.PupilPremiumEligibilityPercentage.Similarity
        },
        new()
        {
            Characteristic = "Average IDACI score",
            CurrentSchoolValue = Dec3dp(characteristics.AverageIdaciScore.CurrentSchoolValue),
            SimilarSchoolValue = Dec3dp(characteristics.AverageIdaciScore.ComparatorSchoolValue),
            IsNumeric = true,
            Similarity = characteristics.AverageIdaciScore.Similarity
        },
        new()
        {
            Characteristic = "Average POLAR4 quintile",
            CurrentSchoolValue = PolarText(characteristics.Polar4Quintile.CurrentSchoolValue),
            SimilarSchoolValue = PolarText(characteristics.Polar4Quintile.ComparatorSchoolValue),
            IsNumeric = false,
            Similarity = characteristics.Polar4Quintile.Similarity
        },
        new()
        {
            Characteristic = "Percentage of pupils with an EHC plan",
            CurrentSchoolValue = Percent1dp(characteristics.PupilsWithEhcPlanPercentage.CurrentSchoolValue),
            SimilarSchoolValue = Percent1dp(characteristics.PupilsWithEhcPlanPercentage.ComparatorSchoolValue),
            IsNumeric = true,
            Similarity = characteristics.PupilsWithEhcPlanPercentage.Similarity
        },
        new()
        {
            Characteristic = "Percentage of pupils with SEN support",
            CurrentSchoolValue = Percent1dp(characteristics.PupilsWithSenSupportPercentage.CurrentSchoolValue),
            SimilarSchoolValue = Percent1dp(characteristics.PupilsWithSenSupportPercentage.ComparatorSchoolValue),
            IsNumeric = true,
            Similarity = characteristics.PupilsWithSenSupportPercentage.Similarity
        },
        new()
        {
            Characteristic = "Percentage of pupils with EAL",
            CurrentSchoolValue = Percent1dp(characteristics.PupilsWithEalPercentage.CurrentSchoolValue),
            SimilarSchoolValue = Percent1dp(characteristics.PupilsWithEalPercentage.ComparatorSchoolValue),
            IsNumeric = true,
            Similarity = characteristics.PupilsWithEalPercentage.Similarity
        }
    }.AsReadOnly();
    }

    // Formatting rules
    private static string Ks2Int(decimal v) =>
        Convert.ToInt32(Math.Round(v, MidpointRounding.AwayFromZero))
            .ToString(CultureInfo.InvariantCulture);

    private static string IntN0(decimal v) =>
        Convert.ToInt32(Math.Round(v, MidpointRounding.AwayFromZero))
            .ToString("N0", CultureInfo.GetCultureInfo("en-GB"));

    private static string Percent1dp(decimal v) =>
        $"{v.ToString("0.0", CultureInfo.InvariantCulture)}%";

    private static string Dec3dp(decimal v) =>
        v.ToString("0.000", CultureInfo.InvariantCulture);

    private static string PolarText(int q) =>
        $"Quintile {q}";

}
