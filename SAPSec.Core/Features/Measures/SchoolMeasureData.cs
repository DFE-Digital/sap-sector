using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Measures;

public record SchoolMeasureData<T>(
    SchoolInfo.SchoolInfo SchoolInfo,
    T? Data)
    where T : class, IMeasureData;
