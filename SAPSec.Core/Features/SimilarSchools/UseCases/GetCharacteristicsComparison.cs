using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.SimilarSchools.UseCases;

public class GetCharacteristicsComparison(ISimilarSchoolsSecondaryRepository repository)
{
    public async Task<GetCharacteristicsComparisonResponse> Execute(GetCharacteristicsComparisonRequest request)
    {
        var urns = new[] { request.CurrentSchoolUrn, request.SimilarSchoolUrn };

        var values = SimilarSchoolsSecondaryValues.FromData(await repository.GetValuesByUrnsAsync(urns));

        var current = values.FirstOrDefault(v => v.Urn == request.CurrentSchoolUrn);
        if (current is null)
            throw new NotFoundException($"No characteristics found for URN {request.CurrentSchoolUrn}");

        var similar = values.FirstOrDefault(v => v.Urn == request.SimilarSchoolUrn);
        if (similar is null)
            throw new NotFoundException($"No characteristics found for URN {request.SimilarSchoolUrn}");

        return new GetCharacteristicsComparisonResponse
        {
            CurrentSchoolUrn = request.CurrentSchoolUrn,
            SimilarSchoolUrn = request.SimilarSchoolUrn,
            Ks2AverageScore = Build(
                RoundWholeNumber(current.Ks2AverageScore),
                RoundWholeNumber(similar.Ks2AverageScore)),
            PupilPremiumEligibilityPercentage = Build(
                RoundToOneDecimalPlace(current.PupilPremiumEligibilityPercentage),
                RoundToOneDecimalPlace(similar.PupilPremiumEligibilityPercentage)),
            PupilsWithEalPercentage = Build(
                RoundToOneDecimalPlace(current.PupilsWithEalPercentage),
                RoundToOneDecimalPlace(similar.PupilsWithEalPercentage)),
            Polar4Quintile = Build(
                RoundInt(current.Polar4Quintile),
                RoundInt(similar.Polar4Quintile)),
            PupilCount = Build(
                RoundInt(current.PupilCount),
                RoundInt(similar.PupilCount)),
            PupilStabilityRate = Build(
                RoundToOneDecimalPlace(current.PupilStabilityRate),
                RoundToOneDecimalPlace(similar.PupilStabilityRate)),
            AverageIdaciScore = Build(
                RoundToThreeDecimalPlaces(current.AverageIdaciScore),
                RoundToThreeDecimalPlaces(similar.AverageIdaciScore)),
            PupilsWithSenSupportPercentage = Build(
                RoundToOneDecimalPlace(current.PupilsWithSenSupportPercentage),
                RoundToOneDecimalPlace(similar.PupilsWithSenSupportPercentage)),
            PupilsWithEhcPlanPercentage = Build(
                RoundToOneDecimalPlace(current.PupilsWithEhcPlanPercentage),
                RoundToOneDecimalPlace(similar.PupilsWithEhcPlanPercentage))
        };
    }

    private static SimilarSchoolCharacteristicComparison<decimal> Build(decimal current, decimal similar) =>
        new(current, similar);

    private static SimilarSchoolCharacteristicComparison<int> Build(int current, int similar) =>
        new(current, similar);

    private static int RoundInt(decimal value) =>
        Convert.ToInt32(Math.Round(value, MidpointRounding.AwayFromZero));

    private static decimal RoundWholeNumber(decimal value) =>
        decimal.Round(value, 0, MidpointRounding.AwayFromZero);

    private static decimal RoundToOneDecimalPlace(decimal value) =>
        decimal.Round(value, 1, MidpointRounding.AwayFromZero);

    private static decimal RoundToThreeDecimalPlaces(decimal value) =>
        decimal.Round(value, 3, MidpointRounding.AwayFromZero);

}

public record GetCharacteristicsComparisonRequest(string CurrentSchoolUrn, string SimilarSchoolUrn);

public record GetCharacteristicsComparisonResponse
{
    public required string CurrentSchoolUrn { get; init; }
    public required string SimilarSchoolUrn { get; init; }
    public required SimilarSchoolCharacteristicComparison<decimal> Ks2AverageScore { get; init; }
    public required SimilarSchoolCharacteristicComparison<decimal> PupilPremiumEligibilityPercentage { get; init; }
    public required SimilarSchoolCharacteristicComparison<decimal> PupilsWithEalPercentage { get; init; }
    public required SimilarSchoolCharacteristicComparison<int> Polar4Quintile { get; init; }
    public required SimilarSchoolCharacteristicComparison<int> PupilCount { get; init; }
    public required SimilarSchoolCharacteristicComparison<decimal> PupilStabilityRate { get; init; }
    public required SimilarSchoolCharacteristicComparison<decimal> AverageIdaciScore { get; init; }
    public required SimilarSchoolCharacteristicComparison<decimal> PupilsWithSenSupportPercentage { get; init; }
    public required SimilarSchoolCharacteristicComparison<decimal> PupilsWithEhcPlanPercentage { get; init; }
}

public record SimilarSchoolCharacteristicComparison<T>(
    T CurrentSchoolValue,
    T SimilarSchoolValue);
