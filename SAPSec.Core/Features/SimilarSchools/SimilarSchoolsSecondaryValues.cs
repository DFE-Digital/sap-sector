namespace SAPSec.Core.Features.SimilarSchools;

public class SimilarSchoolsSecondaryValues : SimilarSchoolsSecondaryMetrics
{
    public string Urn { get; init; } = default!;
    public decimal Ks2ReadingScore { get; init; }
    public decimal Ks2MathsScore { get; init; }

    public new decimal Ks2AverageScore => (Ks2ReadingScore + Ks2MathsScore) / 2m;
}
