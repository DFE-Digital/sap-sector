using SAPSec.Core.Rounding;

namespace SAPSec.Core.Features.SimilarSchools.UseCases;

public record SecondaryComparisonSimilarityCharacteristics
{
    public required SchoolComparisonValue<decimal> Ks2AverageScore { get; init; }
    public required SchoolComparisonValue<decimal> PupilPremiumEligibilityPercentage { get; init; }
    public required SchoolComparisonValue<decimal> PupilsWithEalPercentage { get; init; }
    public required SchoolComparisonValue<int> Polar4Quintile { get; init; }
    public required SchoolComparisonValue<int> PupilCount { get; init; }
    public required SchoolComparisonValue<decimal> PupilStabilityRate { get; init; }
    public required SchoolComparisonValue<decimal> AverageIdaciScore { get; init; }
    public required SchoolComparisonValue<decimal> PupilsWithSenSupportPercentage { get; init; }
    public required SchoolComparisonValue<decimal> PupilsWithEhcPlanPercentage { get; init; }

    internal static SecondaryComparisonSimilarityCharacteristics Build(
        SimilarSchoolsSecondaryValues currentSchoolValues,
        SimilarSchoolsSecondaryValues comparatorSchoolValues) => new()
        {
            Ks2AverageScore = new(
                Round.ToWholeNumber(currentSchoolValues.Ks2AverageScore),
                Round.ToWholeNumber(comparatorSchoolValues.Ks2AverageScore)),
            PupilPremiumEligibilityPercentage = new(
                Round.ToOneDecimalPlace(currentSchoolValues.PupilPremiumEligibilityPercentage),
                Round.ToOneDecimalPlace(comparatorSchoolValues.PupilPremiumEligibilityPercentage)),
            PupilsWithEalPercentage = new(
                Round.ToOneDecimalPlace(currentSchoolValues.PupilsWithEalPercentage),
                Round.ToOneDecimalPlace(comparatorSchoolValues.PupilsWithEalPercentage)),
            Polar4Quintile = new(
                Round.ToInt(currentSchoolValues.Polar4Quintile),
                Round.ToInt(comparatorSchoolValues.Polar4Quintile)),
            PupilCount = new(
                Round.ToInt(currentSchoolValues.PupilCount),
                Round.ToInt(comparatorSchoolValues.PupilCount)),
            PupilStabilityRate = new(
                Round.ToOneDecimalPlace(currentSchoolValues.PupilStabilityRate),
                Round.ToOneDecimalPlace(comparatorSchoolValues.PupilStabilityRate)),
            AverageIdaciScore = new(
                Round.ToThreeDecimalPlaces(currentSchoolValues.AverageIdaciScore),
                Round.ToThreeDecimalPlaces(comparatorSchoolValues.AverageIdaciScore)),
            PupilsWithSenSupportPercentage = new(
                Round.ToOneDecimalPlace(currentSchoolValues.PupilsWithSenSupportPercentage),
                Round.ToOneDecimalPlace(comparatorSchoolValues.PupilsWithSenSupportPercentage)),
            PupilsWithEhcPlanPercentage = new(
                Round.ToOneDecimalPlace(currentSchoolValues.PupilsWithEhcPlanPercentage),
                Round.ToOneDecimalPlace(comparatorSchoolValues.PupilsWithEhcPlanPercentage))
        };
}