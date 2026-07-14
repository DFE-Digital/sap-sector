namespace SAPSec.Core.School.Similarity;

public record SimilarSchoolsData<T>(
    SchoolData<T> CurrentSchool,
    IReadOnlyCollection<SchoolData<T>> SimilarSchools);
