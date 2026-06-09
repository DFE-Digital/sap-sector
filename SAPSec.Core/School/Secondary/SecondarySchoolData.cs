using SAPSec.Core.School.Info;

namespace SAPSec.Core.School.Secondary;

public record SecondarySchoolData<T>(
    SchoolInfo SchoolInfo,
    T? Data);
