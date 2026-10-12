namespace SAPSec.Core.Features.SchoolDetails;

public record SchoolData<T>(
    SchoolInfo.School SchoolInfo,
    T? Data)
    where T : class;
