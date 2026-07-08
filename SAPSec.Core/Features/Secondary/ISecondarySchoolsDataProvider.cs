using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.Secondary;

public interface ISecondarySchoolsDataProvider
{
    Task<SimilarSchoolsData<Ks4PerformanceData>> GetSimilarSchoolsPerformance(string urn);
    Task<SimilarSchoolsData<Ks4DestinationsData>> GetSimilarSchoolsDestinations(string urn);
}
