namespace SAPSec.Core.Features.Primary;

public record SimilarSchoolsData<T>(
    SchoolData<T> CurrentSchool,
    IReadOnlyCollection<SchoolData<T>> SimilarSchools);
