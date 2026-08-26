using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Web.Areas.Primary.ViewModels.Comparison;
using System.Globalization;

namespace SAPSec.Web.Formatters;

public interface IPrimaryCharacteristicsComparisonFormatter
{
    IReadOnlyList<SimilarityPageViewModel.CharacteristicRow> BuildRows(
        GetPrimaryCharacteristicsComparisonResponse response);
}

public sealed class PrimaryCharacteristicsComparisonFormatter : IPrimaryCharacteristicsComparisonFormatter
{
    public IReadOnlyList<SimilarityPageViewModel.CharacteristicRow> BuildRows(
        GetPrimaryCharacteristicsComparisonResponse response)
    {
        return new List<SimilarityPageViewModel.CharacteristicRow>(9)
        {
            new()
            {
                Characteristic = "Combined average KS1 reading, writing and maths prior attainment",
                CurrentSchoolValue = WholeNumber(response.Ks1PriorRwmAverage.CurrentSchoolValue),
                SimilarSchoolValue = WholeNumber(response.Ks1PriorRwmAverage.SimilarSchoolValue)
            },
            new()
            {
                Characteristic = "Total number of pupils",
                CurrentSchoolValue = IntN0(response.PupilCount.CurrentSchoolValue),
                SimilarSchoolValue = IntN0(response.PupilCount.SimilarSchoolValue)
            },
            new()
            {
                Characteristic = "Pupil stability rate",
                CurrentSchoolValue = Percent1dp(response.PupilStabilityRate.CurrentSchoolValue),
                SimilarSchoolValue = Percent1dp(response.PupilStabilityRate.SimilarSchoolValue)
            },
            new()
            {
                Characteristic = "Eligibility for pupil premium",
                CurrentSchoolValue = Percent1dp(response.PupilPremiumEligibilityPercentage.CurrentSchoolValue),
                SimilarSchoolValue = Percent1dp(response.PupilPremiumEligibilityPercentage.SimilarSchoolValue)
            },
            new()
            {
                Characteristic = "Average IDACI score",
                CurrentSchoolValue = Dec3dp(response.AverageIdaciScore.CurrentSchoolValue),
                SimilarSchoolValue = Dec3dp(response.AverageIdaciScore.SimilarSchoolValue)
            },
            new()
            {
                Characteristic = "Average POLAR4 quintile",
                CurrentSchoolValue = PolarText(response.Polar4Quintile.CurrentSchoolValue),
                SimilarSchoolValue = PolarText(response.Polar4Quintile.SimilarSchoolValue)
            },
            new()
            {
                Characteristic = "Percentage of pupils with an EHC plan",
                CurrentSchoolValue = Percent1dp(response.PupilsWithEhcPlanPercentage.CurrentSchoolValue),
                SimilarSchoolValue = Percent1dp(response.PupilsWithEhcPlanPercentage.SimilarSchoolValue)
            },
            new()
            {
                Characteristic = "Percentage of pupils with SEN support",
                CurrentSchoolValue = Percent1dp(response.PupilsWithSenSupportPercentage.CurrentSchoolValue),
                SimilarSchoolValue = Percent1dp(response.PupilsWithSenSupportPercentage.SimilarSchoolValue)
            },
            new()
            {
                Characteristic = "Percentage of pupils with EAL",
                CurrentSchoolValue = Percent1dp(response.PupilsWithEalPercentage.CurrentSchoolValue),
                SimilarSchoolValue = Percent1dp(response.PupilsWithEalPercentage.SimilarSchoolValue)
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
