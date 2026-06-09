using SAPSec.Data.Store;

namespace SAPSec.Core.School.Secondary;

public interface ISecondarySchoolsRepository
{
    Task<SecondarySimilarSchoolsData<Ks4PerformanceData>> GetSimilarSchoolsPerformance(string urn);
    Task<SecondarySimilarSchoolsData<Ks4DestinationsData>> GetSimilarSchoolsDestinations(string urn);
}
