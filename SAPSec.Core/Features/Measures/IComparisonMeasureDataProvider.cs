using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Measures;

public interface IComparisonMeasureDataProvider<T>
    where T : class, IMeasureData
{
    Task<ComparisonMeasureData<T>> GetData(string currentSchoolUrn, string comparatorSchoolUrn);
}
