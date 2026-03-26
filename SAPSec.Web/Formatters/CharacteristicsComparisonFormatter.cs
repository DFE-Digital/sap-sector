using System.Globalization;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Formatters;

public interface ICharacteristicsComparisonFormatter
{
    IReadOnlyList<SimilarSchoolsComparisonViewModel.CharacteristicRow> BuildRows(
        GetCharacteristicsComparisonResponse response);
}

public sealed class CharacteristicsComparisonFormatter : ICharacteristicsComparisonFormatter
{
 public IReadOnlyList<SimilarSchoolsComparisonViewModel.CharacteristicRow> BuildRows(
    GetCharacteristicsComparisonResponse response)
{
    return new List<SimilarSchoolsComparisonViewModel.CharacteristicRow>(9)
    {
        new()
        {
            Characteristic = "Average KS2 reading and maths score",
            CurrentSchoolValue = Ks2Int(response.Ks2AverageScore.CurrentSchoolValue),
            SimilarSchoolValue = Ks2Int(response.Ks2AverageScore.SimilarSchoolValue),
            IsNumeric = true,
            Similarity = response.Ks2AverageScore.Similarity
        },
        new()
        {
            Characteristic = "Total number of pupils",
            CurrentSchoolValue = IntN0(response.PupilCount.CurrentSchoolValue),
            SimilarSchoolValue = IntN0(response.PupilCount.SimilarSchoolValue),
            IsNumeric = true,
            Similarity = response.PupilCount.Similarity
        },
        new()
        {
            Characteristic = "Pupil stability rate",
            CurrentSchoolValue = Percent1dp(response.PupilStabilityRate.CurrentSchoolValue),
            SimilarSchoolValue = Percent1dp(response.PupilStabilityRate.SimilarSchoolValue),
            IsNumeric = true,
            Similarity = response.PupilStabilityRate.Similarity
        },
        new()
        {
            Characteristic = "Eligibility for pupil premium",
            CurrentSchoolValue = Percent1dp(response.PupilPremiumEligibilityPercentage.CurrentSchoolValue),
            SimilarSchoolValue = Percent1dp(response.PupilPremiumEligibilityPercentage.SimilarSchoolValue),
            IsNumeric = true,
            Similarity = response.PupilPremiumEligibilityPercentage.Similarity
        },
        new()
        {
            Characteristic = "Average IDACI score",
            CurrentSchoolValue = Dec3dp(response.AverageIdaciScore.CurrentSchoolValue),
            SimilarSchoolValue = Dec3dp(response.AverageIdaciScore.SimilarSchoolValue),
            IsNumeric = true,
            Similarity = response.AverageIdaciScore.Similarity
        },
        new()
        {
            Characteristic = "Average POLAR4 quintile",
            CurrentSchoolValue = PolarText(response.Polar4Quintile.CurrentSchoolValue),
            SimilarSchoolValue = PolarText(response.Polar4Quintile.SimilarSchoolValue),
            IsNumeric = false,
            Similarity = response.Polar4Quintile.Similarity
        },
        new()
        {
            Characteristic = "Percentage of pupils with an EHC plan",
            CurrentSchoolValue = Percent1dp(response.PupilsWithEhcPlanPercentage.CurrentSchoolValue),
            SimilarSchoolValue = Percent1dp(response.PupilsWithEhcPlanPercentage.SimilarSchoolValue),
            IsNumeric = true,
            Similarity = response.PupilsWithEhcPlanPercentage.Similarity
        },
        new()
        {
            Characteristic = "Percentage of pupils with SEN support",
            CurrentSchoolValue = Percent1dp(response.PupilsWithSenSupportPercentage.CurrentSchoolValue),
            SimilarSchoolValue = Percent1dp(response.PupilsWithSenSupportPercentage.SimilarSchoolValue),
            IsNumeric = true,
            Similarity = response.PupilsWithSenSupportPercentage.Similarity
        },
        new()
        {
            Characteristic = "Percentage of pupils with EAL",
            CurrentSchoolValue = Percent1dp(response.PupilsWithEalPercentage.CurrentSchoolValue),
            SimilarSchoolValue = Percent1dp(response.PupilsWithEalPercentage.SimilarSchoolValue),
            IsNumeric = true,
            Similarity = response.PupilsWithEalPercentage.Similarity
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
