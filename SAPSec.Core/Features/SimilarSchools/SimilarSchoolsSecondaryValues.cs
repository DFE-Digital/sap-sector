namespace SAPSec.Core.Features.SimilarSchools;

public class SimilarSchoolsSecondaryValues
{
    public string Urn { get; init; } = default!;
    public decimal Ks2ReadingScore { get; init; }
    public decimal Ks2MathsScore { get; init; }
    public decimal PupilPremiumEligibilityPercentage { get; init; }
    public decimal PupilsWithEalPercentage { get; init; }
    public decimal Polar4Quintile { get; init; }
    public decimal PupilStabilityRate { get; init; }
    public decimal AverageIdaciScore { get; init; }
    public decimal PupilsWithSenSupportPercentage { get; init; }
    public decimal PupilCount { get; init; }
    public decimal PupilsWithEhcPlanPercentage { get; init; }

    public decimal Ks2AverageScore => (Ks2ReadingScore + Ks2MathsScore) / 2m;
}
