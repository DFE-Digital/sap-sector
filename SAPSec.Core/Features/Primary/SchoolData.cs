namespace SAPSec.Core.Features.Primary;

public record SchoolData<T>(
    SchoolInfo.SchoolInfo SchoolInfo,
    T? Data);
