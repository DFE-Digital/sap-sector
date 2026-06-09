namespace SAPSec.Core.School.Secondary;

public record SecondarySimilarSchoolsData<T>(
    SecondarySchoolData<T> CurrentSchool,
    IReadOnlyCollection<SecondarySchoolData<T>> SimilarSchools);
