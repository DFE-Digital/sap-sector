namespace SAPSec.Core.Features.SimilarSchools;

public record SchoolComparisonValue<T>(
    T CurrentSchoolValue,
    T ComparatorSchoolValue,
    SchoolSimilarity? Similarity = null);
