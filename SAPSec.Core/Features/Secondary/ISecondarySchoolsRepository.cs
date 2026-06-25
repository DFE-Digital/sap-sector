using SAPSec.Core.Features.Primary;
using SAPSec.Data.Store;

namespace SAPSec.Core.Features.Secondary;

public interface ISecondarySchoolsRepository
{
    Task<SimilarSchoolsData<Ks4PerformanceData>> GetSimilarSchoolsPerformance(string urn);
    Task<SimilarSchoolsData<Ks4DestinationsData>> GetSimilarSchoolsDestinations(string urn);
}
