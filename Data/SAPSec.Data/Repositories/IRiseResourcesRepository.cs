using SAPSec.Data.Dto.RiseResources;

namespace SAPSec.Data.Repositories;

/// <summary>
/// Source of RISE resources content. JSON-only (no PostgreSQL implementation);
/// backed by <c>SAPSec.Infrastructure/Content/rise-resources.json</c>.
/// </summary>
public interface IRiseResourcesRepository
{
    Task<IReadOnlyCollection<RiseResourceEntry>> GetAllAsync();
}
