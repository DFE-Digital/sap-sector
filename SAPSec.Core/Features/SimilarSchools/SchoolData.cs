namespace SAPSec.Core.Features.SimilarSchools;

public record SchoolData<T>(
    SchoolInfo.SchoolInfo SchoolInfo,
    T? Data);
