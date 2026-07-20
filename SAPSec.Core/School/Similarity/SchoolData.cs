using SAPSec.Core.School.Info;

namespace SAPSec.Core.School.Similarity;

public record SchoolData<T>(
    SchoolInfo SchoolInfo,
    T? Data);
