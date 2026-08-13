using SAPSec.Data.Repositories;
using static SAPSec.Core.Features.SimilarSchools.CharacteristicValueRounding;

namespace SAPSec.Core.Features.SimilarSchools.UseCases;

public class GetPrimaryCharacteristicsComparison(ISimilarSchoolsPrimaryRepository repository)
{
    public async Task<GetPrimaryCharacteristicsComparisonResponse> Execute(GetPrimaryCharacteristicsComparisonRequest request)
    {
        var urns = new[] { request.CurrentSchoolUrn, request.SimilarSchoolUrn };

        var values = SimilarSchoolsPrimaryValues.FromData(await repository.GetValuesByUrnsAsync(urns)).ToList();

        var current = values.FirstOrDefault(v => v.Urn == request.CurrentSchoolUrn);
        if (current is null)
            throw new NotFoundException($"No characteristics found for URN {request.CurrentSchoolUrn}");

        var similar = values.FirstOrDefault(v => v.Urn == request.SimilarSchoolUrn);
        if (similar is null)
            throw new NotFoundException($"No characteristics found for URN {request.SimilarSchoolUrn}");

        return new GetPrimaryCharacteristicsComparisonResponse
        {
            CurrentSchoolUrn = request.CurrentSchoolUrn,
            SimilarSchoolUrn = request.SimilarSchoolUrn,
            Ks1PriorRwmAverage = Build(
                RoundWholeNumber(current.Ks1PriorRwmAverage),
                RoundWholeNumber(similar.Ks1PriorRwmAverage)),
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

    private static SchoolComparisonValue<decimal> Build(decimal current, decimal similar) =>
        new(current, similar);

    private static SchoolComparisonValue<int> Build(int current, int similar) =>
        new(current, similar);
}

public record GetPrimaryCharacteristicsComparisonRequest(
    string CurrentSchoolUrn,
    string SimilarSchoolUrn);

public record GetPrimaryCharacteristicsComparisonResponse
{
    public required string CurrentSchoolUrn { get; init; }
    public required string SimilarSchoolUrn { get; init; }
    public required SchoolComparisonValue<decimal> Ks1PriorRwmAverage { get; init; }
    public required SchoolComparisonValue<decimal> PupilPremiumEligibilityPercentage { get; init; }
    public required SchoolComparisonValue<decimal> PupilsWithEalPercentage { get; init; }
    public required SchoolComparisonValue<int> Polar4Quintile { get; init; }
    public required SchoolComparisonValue<int> PupilCount { get; init; }
    public required SchoolComparisonValue<decimal> PupilStabilityRate { get; init; }
    public required SchoolComparisonValue<decimal> AverageIdaciScore { get; init; }
    public required SchoolComparisonValue<decimal> PupilsWithSenSupportPercentage { get; init; }
    public required SchoolComparisonValue<decimal> PupilsWithEhcPlanPercentage { get; init; }
}
