using SAPSec.Core.Features.SimilarSchools;

namespace SAPSec.Core.Features.SimilarSchools.UseCases;

public class GetCharacteristicsComparison(
    ISimilarSchoolsSecondaryRepository repository)
{
    public async Task<GetCharacteristicsComparisonResponse> Execute(GetCharacteristicsComparisonRequest request)
    {
        var urns = new[] { request.CurrentSchoolUrn, request.SimilarSchoolUrn };

        var values = await repository.GetSecondaryValuesByUrnsAsync(urns);
        var standardDeviations = request.SimilarityCalculationMethod == SimilarityCalculationMethod.Group
            ? await BuildGroupStandardDeviationsAsync(request.CurrentSchoolUrn)
            : await repository.GetSimilarSchoolsSecondaryStandardDeviationsAsync();

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
                standardDeviations.Ks2AverageScore),
            PupilPremiumEligibilityPercentage = Build(
                current.PupilPremiumEligibilityPercentage,
                similar.PupilPremiumEligibilityPercentage,
                standardDeviations.PupilPremiumEligibilityPercentage),
            PupilsWithEalPercentage = Build(
                current.PupilsWithEalPercentage,
                similar.PupilsWithEalPercentage,
                standardDeviations.PupilsWithEalPercentage),
            Polar4Quintile = Build(
                RoundInt(current.Polar4Quintile),
                RoundInt(similar.Polar4Quintile),
                RoundInt(standardDeviations.Polar4Quintile)),
            PupilCount = Build(
                RoundInt(current.PupilCount),
                RoundInt(similar.PupilCount),
                RoundInt(standardDeviations.PupilCount)),
            PupilStabilityRate = Build(
                current.PupilStabilityRate,
                similar.PupilStabilityRate,
                standardDeviations.PupilStabilityRate),
            AverageIdaciScore = Build(
                current.AverageIdaciScore,
                similar.AverageIdaciScore,
                standardDeviations.AverageIdaciScore),
            PupilsWithSenSupportPercentage = Build(
                current.PupilsWithSenSupportPercentage,
                similar.PupilsWithSenSupportPercentage,
                standardDeviations.PupilsWithSenSupportPercentage),
            PupilsWithEhcPlanPercentage = Build(
                current.PupilsWithEhcPlanPercentage,
                similar.PupilsWithEhcPlanPercentage,
                standardDeviations.PupilsWithEhcPlanPercentage)
        };
    }

    private async Task<SimilarSchoolsSecondaryStandardDeviations> BuildGroupStandardDeviationsAsync(string currentSchoolUrn)
    {
        var groupUrns = await repository.GetSimilarSchoolUrnsAsync(currentSchoolUrn);
        var groupValues = await repository.GetSecondaryValuesByUrnsAsync(groupUrns);

        return new SimilarSchoolsSecondaryStandardDeviations
        {
            Ks2AverageScore = PopulationStandardDeviation(groupValues.Select(v => v.Ks2AverageScore)),
            PupilPremiumEligibilityPercentage = PopulationStandardDeviation(groupValues.Select(v => v.PupilPremiumEligibilityPercentage)),
            PupilsWithEalPercentage = PopulationStandardDeviation(groupValues.Select(v => v.PupilsWithEalPercentage)),
            Polar4Quintile = PopulationStandardDeviation(groupValues.Select(v => v.Polar4Quintile)),
            PupilStabilityRate = PopulationStandardDeviation(groupValues.Select(v => v.PupilStabilityRate)),
            AverageIdaciScore = PopulationStandardDeviation(groupValues.Select(v => v.AverageIdaciScore)),
            PupilsWithSenSupportPercentage = PopulationStandardDeviation(groupValues.Select(v => v.PupilsWithSenSupportPercentage)),
            PupilCount = PopulationStandardDeviation(groupValues.Select(v => v.PupilCount)),
            PupilsWithEhcPlanPercentage = PopulationStandardDeviation(groupValues.Select(v => v.PupilsWithEhcPlanPercentage))
        };
    }

    private static SimilarSchoolCharacteristicComparison<decimal> Build(decimal current, decimal similar, decimal standardDeviation)
    {
        return new SimilarSchoolCharacteristicComparison<decimal>(
            current,
            similar,
            Calculate(current, similar, standardDeviation));
    }

    private static SimilarSchoolCharacteristicComparison<int> Build(int current, int similar, int standardDeviation)
    {
        return new SimilarSchoolCharacteristicComparison<int>(
            current,
            similar,
            Calculate(current, similar, standardDeviation));
    }

    // Calculates standardized difference: d = (xA - xB) / standardDeviation.
    // Uses |d| to classify similarity:
    // <= 0.3 => Similar, <= 0.7 => LessSimilar, > 0.7 => NotSimilar.
    private static SchoolSimilarity Calculate(decimal xA, decimal xB, decimal standardDeviation)
    {
        if (standardDeviation <= 0)
            return SchoolSimilarity.NotSimilar;

        var d = (xA - xB) / standardDeviation;
        var absD = Math.Abs(d);

        if (absD <= 0.3m) return SchoolSimilarity.Similar;
        if (absD <= 0.7m) return SchoolSimilarity.LessSimilar;
        return SchoolSimilarity.NotSimilar;
    }

    private static int RoundInt(decimal value) =>
        Convert.ToInt32(Math.Round(value, MidpointRounding.AwayFromZero));

    private static decimal PopulationStandardDeviation(IEnumerable<decimal> values)
    {
        var samples = values.ToArray();
        if (samples.Length == 0)
        {
            return 0m;
        }

        var mean = samples.Average();
        var variance = samples
            .Select(v => (v - mean) * (v - mean))
            .Average();

        return (decimal)Math.Sqrt((double)variance);
    }
}

public record GetCharacteristicsComparisonRequest(
    string CurrentSchoolUrn,
    string SimilarSchoolUrn,
    SimilarityCalculationMethod SimilarityCalculationMethod = SimilarityCalculationMethod.National);

public enum SimilarityCalculationMethod
{
    National,
    Group
}

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
