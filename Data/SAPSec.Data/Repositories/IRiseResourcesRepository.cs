using SAPSec.Data.Dto.RiseResources;

namespace SAPSec.Data.Repositories;

public interface IRiseResourcesRepository
{
    Task<RiseResourcesDocument> GetAsync();
}
