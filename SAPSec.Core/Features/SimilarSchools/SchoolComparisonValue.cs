namespace SAPSec.Core.Features.SimilarSchools;

public record SchoolComparisonValue<T>(
    T CurrentSchoolValue,
    T SimilarSchoolValue);
