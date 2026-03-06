using SAPSec.Core.Features.SimilarSchools;

namespace SAPSec.Core.Features.SimilarSchools.UseCases;

public class GetCharacteristicsComparison(
    ISimilarSchoolsSecondaryRepository repository)
{
    public async Task<GetCharacteristicsComparisonResponse> Execute(GetCharacteristicsComparisonRequest request)
    {
        var urns = new[] { request.CurrentSchoolUrn, request.SimilarSchoolUrn };

        var values = await repository.GetSecondaryValuesByUrnsAsync(urns);
        var national = await repository.GetSimilarSchoolsSecondaryNationalSdAsync();

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
                current.Ks2AverageScore,
                similar.Ks2AverageScore,
                national.Ks2AverageScore),
            PupilPremiumEligibilityPercentage = Build(
                current.PupilPremiumEligibilityPercentage,
                similar.PupilPremiumEligibilityPercentage,
                national.PupilPremiumEligibilityPercentage),
            PupilsWithEalPercentage = Build(
                current.PupilsWithEalPercentage,
                similar.PupilsWithEalPercentage,
                national.PupilsWithEalPercentage),
            Polar4Quintile = Build(
                RoundInt(current.Polar4Quintile),
                RoundInt(similar.Polar4Quintile),
                RoundInt(national.Polar4Quintile)),
            PupilCount = Build(
                RoundInt(current.PupilCount),
                RoundInt(similar.PupilCount),
                RoundInt(national.PupilCount)),
            PupilStabilityRate = Build(
                current.PupilStabilityRate,
                similar.PupilStabilityRate,
                national.PupilStabilityRate),
            AverageIdaciScore = Build(
                current.AverageIdaciScore,
                similar.AverageIdaciScore,
                national.AverageIdaciScore),
            PupilsWithSenSupportPercentage = Build(
                current.PupilsWithSenSupportPercentage,
                similar.PupilsWithSenSupportPercentage,
                national.PupilsWithSenSupportPercentage),
            PupilsWithEhcPlanPercentage = Build(
                current.PupilsWithEhcPlanPercentage,
                similar.PupilsWithEhcPlanPercentage,
                national.PupilsWithEhcPlanPercentage)
        };
    }

    private static SimilarSchoolCharacteristicComparison<decimal> Build(decimal current, decimal similar, decimal sdNational)
    {
        return new SimilarSchoolCharacteristicComparison<decimal>(
            current,
            similar,
            Calculate(current, similar, sdNational));
    }

    private static SimilarSchoolCharacteristicComparison<int> Build(int current, int similar, int sdNational)
    {
        return new SimilarSchoolCharacteristicComparison<int>(
            current,
            similar,
            Calculate(current, similar, sdNational));
    }

    private static SchoolSimilarity Calculate(decimal xA, decimal xB, decimal sdNational)
    {
        if (sdNational <= 0)
            return SchoolSimilarity.NotSimilar;

        var d = (xA - xB) / sdNational;
        var absD = Math.Abs(d);

        if (absD <= 0.3m) return SchoolSimilarity.Similar;
        if (absD <= 0.7m) return SchoolSimilarity.LessSimilar;
        return SchoolSimilarity.NotSimilar;
    }

    private static int RoundInt(decimal value) =>
        Convert.ToInt32(Math.Round(value, MidpointRounding.AwayFromZero));
}

public record GetCharacteristicsComparisonRequest(
    string CurrentSchoolUrn,
    string SimilarSchoolUrn);

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
    T SimilarSchoolValue,
    SchoolSimilarity Similarity);
