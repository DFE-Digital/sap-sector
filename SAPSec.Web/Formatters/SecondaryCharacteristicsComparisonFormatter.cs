using SAPSec.Core.Features.SimilarSchools;
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
            ComparatorSchoolValue = Ks2Int(characteristics.Ks2AverageScore.ComparatorSchoolValue),
        },
        new()
        {
            Characteristic = "Total number of pupils",
            CurrentSchoolValue = IntN0(characteristics.PupilCount.CurrentSchoolValue),
            ComparatorSchoolValue = IntN0(characteristics.PupilCount.ComparatorSchoolValue),
        },
        new()
        {
            Characteristic = "Pupil stability rate",
            CurrentSchoolValue = Percent1dp(characteristics.PupilStabilityRate.CurrentSchoolValue),
            ComparatorSchoolValue = Percent1dp(characteristics.PupilStabilityRate.ComparatorSchoolValue),
        },
        new()
        {
            Characteristic = "Eligibility for pupil premium",
            CurrentSchoolValue = Percent1dp(characteristics.PupilPremiumEligibilityPercentage.CurrentSchoolValue),
            ComparatorSchoolValue = Percent1dp(characteristics.PupilPremiumEligibilityPercentage.ComparatorSchoolValue),
        },
        new()
        {
            Characteristic = "Average IDACI score",
            CurrentSchoolValue = Dec3dp(characteristics.AverageIdaciScore.CurrentSchoolValue),
            ComparatorSchoolValue = Dec3dp(characteristics.AverageIdaciScore.ComparatorSchoolValue),
        },
        new()
        {
            Characteristic = "Average POLAR4 quintile",
            CurrentSchoolValue = PolarText(characteristics.Polar4Quintile.CurrentSchoolValue),
            ComparatorSchoolValue = PolarText(characteristics.Polar4Quintile.ComparatorSchoolValue),
        },
        new()
        {
            Characteristic = "Percentage of pupils with an EHC plan",
            CurrentSchoolValue = Percent1dp(characteristics.PupilsWithEhcPlanPercentage.CurrentSchoolValue),
            ComparatorSchoolValue = Percent1dp(characteristics.PupilsWithEhcPlanPercentage.ComparatorSchoolValue),
        },
        new()
        {
            Characteristic = "Percentage of pupils with SEN support",
            CurrentSchoolValue = Percent1dp(characteristics.PupilsWithSenSupportPercentage.CurrentSchoolValue),
            ComparatorSchoolValue = Percent1dp(characteristics.PupilsWithSenSupportPercentage.ComparatorSchoolValue),
        },
        new()
        {
            Characteristic = "Percentage of pupils with EAL",
            CurrentSchoolValue = Percent1dp(characteristics.PupilsWithEalPercentage.CurrentSchoolValue),
            ComparatorSchoolValue = Percent1dp(characteristics.PupilsWithEalPercentage.ComparatorSchoolValue),
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
