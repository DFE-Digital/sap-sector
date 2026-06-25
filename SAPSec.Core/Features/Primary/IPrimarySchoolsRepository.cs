using SAPSec.Data.Store;

namespace SAPSec.Core.Features.Primary;

public interface IPrimarySchoolsRepository
{
    Task<SimilarSchoolsData<Ks2PerformanceData>> GetSimilarSchoolsPerformance(string urn);
}
