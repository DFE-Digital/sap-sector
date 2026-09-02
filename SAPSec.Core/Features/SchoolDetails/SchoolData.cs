namespace SAPSec.Core.Features.SchoolDetails;

public record SchoolData<T>(
    SchoolInfo.SchoolInfo SchoolInfo,
    T? Data)
    where T : class;
