namespace SAPSec.Core.Features.SchoolDetails;

public record ComparisonSchoolData<T>(
    SchoolData<T> CurrentSchool,
    SchoolData<T> ComparatorSchool)
    where T : class;

