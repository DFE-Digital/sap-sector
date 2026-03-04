using System.Globalization;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Web.Helpers;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Formatters;

public interface ICharacteristicsComparisonFormatter
{
    IReadOnlyList<SimilarSchoolsComparisonViewModel.CharacteristicRow> BuildRows(
        SimilarSchoolsSecondaryValues current,
        SimilarSchoolsSecondaryValues similar,
        SimilarSchoolsSecondaryNationalSD nationalSds);
}

public sealed class CharacteristicsComparisonFormatter : ICharacteristicsComparisonFormatter
{
 public IReadOnlyList<SimilarSchoolsComparisonViewModel.CharacteristicRow> BuildRows(
    SimilarSchoolsSecondaryValues current,
    SimilarSchoolsSecondaryValues similar,
    SimilarSchoolsSecondaryNationalSD national)
{
    return new List<SimilarSchoolsComparisonViewModel.CharacteristicRow>(9)
    {
        new()
        {
            Characteristic = "Average KS2 reading and maths score",
            CurrentSchoolValue = Ks2Int(current),
            SimilarSchoolValue = Ks2Int(similar),
            IsNumeric = true,
            Similarity = SimilarityCalculator.Calculate(current.Ks2AverageScore, similar.Ks2AverageScore, national.Ks2AverageScore)
        },
        new()
        {
            Characteristic = "Total number of pupils",
            CurrentSchoolValue = IntN0(current.PupilCount),
            SimilarSchoolValue = IntN0(similar.PupilCount),
            IsNumeric = true,
            Similarity = SimilarityCalculator.Calculate(current.PupilCount, similar.PupilCount, national.PupilCount)
        },
        new()
        {
            Characteristic = "Pupil stability rate",
            CurrentSchoolValue = Percent1dp(current.PupilStabilityRate),
            SimilarSchoolValue = Percent1dp(similar.PupilStabilityRate),
            IsNumeric = true,
            Similarity = SimilarityCalculator.Calculate(current.PupilStabilityRate, similar.PupilStabilityRate, national.PupilStabilityRate)
        },
        new()
        {
            Characteristic = "Eligibility for pupil premium",
            CurrentSchoolValue = Percent1dp(current.PupilPremiumEligibilityPercentage),
            SimilarSchoolValue = Percent1dp(similar.PupilPremiumEligibilityPercentage),
            IsNumeric = true,
            Similarity = SimilarityCalculator.Calculate(current.PupilPremiumEligibilityPercentage, similar.PupilPremiumEligibilityPercentage, national.PupilPremiumEligibilityPercentage)
        },
        new()
        {
            Characteristic = "Average IDACI score",
            CurrentSchoolValue = Dec3dp(current.AverageIdaciScore),
            SimilarSchoolValue = Dec3dp(similar.AverageIdaciScore),
            IsNumeric = true,
            Similarity = SimilarityCalculator.Calculate(current.AverageIdaciScore, similar.AverageIdaciScore, national.AverageIdaciScore)
        },
        new()
        {
            Characteristic = "Average POLAR4 quintile",
            CurrentSchoolValue = PolarText(current.Polar4Quintile),
            SimilarSchoolValue = PolarText(similar.Polar4Quintile),
            IsNumeric = false,
            Similarity = SimilarityCalculator.Calculate(current.Polar4Quintile, similar.Polar4Quintile, national.Polar4Quintile)
        },
        new()
        {
            Characteristic = "Percentage of pupils with an EHC plan",
            CurrentSchoolValue = Percent1dp(current.PupilsWithEhcPlanPercentage),
            SimilarSchoolValue = Percent1dp(similar.PupilsWithEhcPlanPercentage),
            IsNumeric = true,
            Similarity = SimilarityCalculator.Calculate(current.PupilsWithEhcPlanPercentage, similar.PupilsWithEhcPlanPercentage, national.PupilsWithEhcPlanPercentage)
        },
        new()
        {
            Characteristic = "Percentage of pupils with SEN support",
            CurrentSchoolValue = Percent1dp(current.PupilsWithSenSupportPercentage),
            SimilarSchoolValue = Percent1dp(similar.PupilsWithSenSupportPercentage),
            IsNumeric = true,
            Similarity = SimilarityCalculator.Calculate(current.PupilsWithSenSupportPercentage, similar.PupilsWithSenSupportPercentage, national.PupilsWithSenSupportPercentage)
        },
        new()
        {
            Characteristic = "Percentage of pupils with EAL",
            CurrentSchoolValue = Percent1dp(current.PupilsWithEalPercentage),
            SimilarSchoolValue = Percent1dp(similar.PupilsWithEalPercentage),
            IsNumeric = true,
            Similarity = SimilarityCalculator.Calculate(current.PupilsWithEalPercentage, similar.PupilsWithEalPercentage, national.PupilsWithEalPercentage)
        }
    }.AsReadOnly();
}

    // Formatting rules
    private static string Ks2Int(SimilarSchoolsSecondaryValues s)
    {
        return Convert.ToInt32(Math.Round(s.Ks2AverageScore, MidpointRounding.AwayFromZero))
            .ToString(CultureInfo.InvariantCulture);
    }

    private static string IntN0(decimal v) =>
        Convert.ToInt32(Math.Round(v, MidpointRounding.AwayFromZero))
            .ToString("N0", CultureInfo.GetCultureInfo("en-GB"));

    private static string Percent1dp(decimal v) =>
        $"{v.ToString("0.0", CultureInfo.InvariantCulture)}%";

    private static string Dec3dp(decimal v) =>
        v.ToString("0.000", CultureInfo.InvariantCulture);

    private static string PolarText(decimal q) =>
        $"Quintile {Convert.ToInt32(Math.Round(q, MidpointRounding.AwayFromZero))}";
    
}
