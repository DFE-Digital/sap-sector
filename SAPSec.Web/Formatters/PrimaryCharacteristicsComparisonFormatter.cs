using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Web.Areas.Shared.ViewModels.Comparison;
using System.Globalization;

namespace SAPSec.Web.Formatters;

public sealed class PrimaryCharacteristicsComparisonFormatter : IPrimaryCharacteristicsComparisonFormatter
{
    public IReadOnlyList<SimilarityPageViewModel.CharacteristicRow> BuildRows(
        PrimaryComparisonSimilarityCharacteristics characteristicts)
    {
        return new List<SimilarityPageViewModel.CharacteristicRow>(9)
        {
            new()
            {
                Characteristic = "Combined average KS1 reading, writing and maths prior attainment",
                CurrentSchoolValue = WholeNumber(characteristicts.Ks1PriorRwmAverage.CurrentSchoolValue),
                SimilarSchoolValue = WholeNumber(characteristicts.Ks1PriorRwmAverage.ComparatorSchoolValue)
            },
            new()
            {
                Characteristic = "Total number of pupils",
                CurrentSchoolValue = IntN0(characteristicts.PupilCount.CurrentSchoolValue),
                SimilarSchoolValue = IntN0(characteristicts.PupilCount.ComparatorSchoolValue)
            },
            new()
            {
                Characteristic = "Pupil stability rate",
                CurrentSchoolValue = Percent1dp(characteristicts.PupilStabilityRate.CurrentSchoolValue),
                SimilarSchoolValue = Percent1dp(characteristicts.PupilStabilityRate.ComparatorSchoolValue)
            },
            new()
            {
                Characteristic = "Eligibility for pupil premium",
                CurrentSchoolValue = Percent1dp(characteristicts.PupilPremiumEligibilityPercentage.CurrentSchoolValue),
                SimilarSchoolValue = Percent1dp(characteristicts.PupilPremiumEligibilityPercentage.ComparatorSchoolValue)
            },
            new()
            {
                Characteristic = "Average IDACI score",
                CurrentSchoolValue = Dec3dp(characteristicts.AverageIdaciScore.CurrentSchoolValue),
                SimilarSchoolValue = Dec3dp(characteristicts.AverageIdaciScore.ComparatorSchoolValue)
            },
            new()
            {
                Characteristic = "Average POLAR4 quintile",
                CurrentSchoolValue = PolarText(characteristicts.Polar4Quintile.CurrentSchoolValue),
                SimilarSchoolValue = PolarText(characteristicts.Polar4Quintile.ComparatorSchoolValue)
            },
            new()
            {
                Characteristic = "Percentage of pupils with an EHC plan",
                CurrentSchoolValue = Percent1dp(characteristicts.PupilsWithEhcPlanPercentage.CurrentSchoolValue),
                SimilarSchoolValue = Percent1dp(characteristicts.PupilsWithEhcPlanPercentage.ComparatorSchoolValue)
            },
            new()
            {
                Characteristic = "Percentage of pupils with SEN support",
                CurrentSchoolValue = Percent1dp(characteristicts.PupilsWithSenSupportPercentage.CurrentSchoolValue),
                SimilarSchoolValue = Percent1dp(characteristicts.PupilsWithSenSupportPercentage.ComparatorSchoolValue)
            },
            new()
            {
                Characteristic = "Percentage of pupils with EAL",
                CurrentSchoolValue = Percent1dp(characteristicts.PupilsWithEalPercentage.CurrentSchoolValue),
                SimilarSchoolValue = Percent1dp(characteristicts.PupilsWithEalPercentage.ComparatorSchoolValue)
            }
        }.AsReadOnly();
    }

    private static string WholeNumber(decimal v) =>
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
