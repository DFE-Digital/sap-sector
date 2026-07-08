namespace SAPSec.Core.Features.SimilarSchools;

public record SimilarSchoolsData<T>(
    SchoolData<T> CurrentSchool,
    IReadOnlyCollection<SchoolData<T>> SimilarSchools);
