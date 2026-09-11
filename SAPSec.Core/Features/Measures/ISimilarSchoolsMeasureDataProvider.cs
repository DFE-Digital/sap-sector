using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Measures;

public interface ISimilarSchoolsMeasureDataProvider<T>
    where T : class, IMeasureData
{
    Task<SimilarSchoolsMeasureData<T>> GetData(string currentSchoolUrn);
}