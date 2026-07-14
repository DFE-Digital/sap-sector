using SAPSec.Core.School.Info;

namespace SAPSec.Core.Features.SimilarSchools;

public record SchoolData<T>(
    SchoolInfo SchoolInfo,
    T? Data);
