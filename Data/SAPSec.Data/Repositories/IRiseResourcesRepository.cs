using SAPSec.Data.Dto.RiseResources;

namespace SAPSec.Data.Repositories;

public interface IRiseResourcesRepository : IRepository
{
    Task<RiseResourcesDocument> GetAsync();
}
