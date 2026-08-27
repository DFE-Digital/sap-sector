using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Measures;

public interface IMeasureDataProvider<T>
    where T : class, IMeasureData
{
    Task<SchoolMeasureData<T>> GetData(string currentSchoolUrn);
}
