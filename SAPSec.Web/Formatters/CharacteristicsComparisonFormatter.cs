using System.Globalization;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Formatters;

public interface ICharacteristicsComparisonFormatter
{
    IReadOnlyList<SimilarSchoolsComparisonViewModel.CharacteristicRow> BuildRows(
        SimilarSchoolsSecondaryValues current,
        SimilarSchoolsSecondaryValues similar);
}

public sealed class CharacteristicsComparisonFormatter : ICharacteristicsComparisonFormatter
{
    public IReadOnlyList<SimilarSchoolsComparisonViewModel.CharacteristicRow> BuildRows(
        SimilarSchoolsSecondaryValues current,
        SimilarSchoolsSecondaryValues similar)
    {
        return new List<SimilarSchoolsComparisonViewModel.CharacteristicRow>(9)
        {
            new()
            {
                Characteristic = "Average KS2 reading and maths score",
                CurrentSchoolValue = Ks2Int(current),
                SimilarSchoolValue = Ks2Int(similar),
                IsNumeric = true
            },
            new()
            {
                Characteristic = "Total number of pupils",
                CurrentSchoolValue = IntN0(current.PupilCount),
                SimilarSchoolValue = IntN0(similar.PupilCount),
                IsNumeric = true
            },
            new()
            {
                Characteristic = "Pupil stability rate",
                CurrentSchoolValue = Percent1dp(current.PupilStabilityRate),
                SimilarSchoolValue = Percent1dp(similar.PupilStabilityRate),
                IsNumeric = true
            },
            new()
            {
                Characteristic = "Eligibility for pupil premium",
                CurrentSchoolValue = Percent1dp(current.PupilPremiumEligibilityPercentage),
                SimilarSchoolValue = Percent1dp(similar.PupilPremiumEligibilityPercentage),
                IsNumeric = true
            },
            new()
            {
                Characteristic = "Average IDACI score",
                CurrentSchoolValue = Dec3dp(current.AverageIdaciScore),
                SimilarSchoolValue = Dec3dp(similar.AverageIdaciScore),
                IsNumeric = true
            },
            new()
            {
                Characteristic = "Average POLAR4 quintile",
                CurrentSchoolValue = PolarText(current.Polar4Quintile),
                SimilarSchoolValue = PolarText(similar.Polar4Quintile),
                IsNumeric = false
            },
            new()
            {
                Characteristic = "Percentage of pupils with an EHC plan",
                CurrentSchoolValue = Percent1dp(current.PupilsWithEhcPlanPercentage),
                SimilarSchoolValue = Percent1dp(similar.PupilsWithEhcPlanPercentage),
                IsNumeric = true
            },
            new()
            {
                Characteristic = "Percentage of pupils with SEN support",
                CurrentSchoolValue = Percent1dp(current.PupilsWithSenSupportPercentage),
                SimilarSchoolValue = Percent1dp(similar.PupilsWithSenSupportPercentage),
                IsNumeric = true
            },
            new()
            {
                Characteristic = "Percentage of pupils with EAL",
                CurrentSchoolValue = Percent1dp(current.PupilsWithEalPercentage),
                SimilarSchoolValue = Percent1dp(similar.PupilsWithEalPercentage),
                IsNumeric = true
            },
        }.AsReadOnly();
    }

    // Formatting rules
    private static string Ks2Int(SimilarSchoolsSecondaryValues s)
    {
        return Convert.ToInt32(Math.Round(s.Ks2AverageScore, MidpointRounding.AwayFromZero))
            .ToString(CultureInfo.InvariantCulture);
    }

    private static string IntN0(int v) =>
        v.ToString("N0", CultureInfo.GetCultureInfo("en-GB"));

    private static string Percent1dp(decimal v) =>
        $"{v.ToString("0.0", CultureInfo.InvariantCulture)}%";

    private static string Dec3dp(decimal v) =>
        v.ToString("0.000", CultureInfo.InvariantCulture);

    private static string PolarText(int q) =>
        $"Quintile {q}";
}
