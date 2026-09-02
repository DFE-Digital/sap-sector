using SAPSec.Data.Dto.RiseResources;
using SAPSec.Data.Repositories;

namespace SAPSec.Test.Common.InMemory;

public class InMemoryRiseResourcesRepository : IRiseResourcesRepository
{
    private List<RiseResourceEntry> _entries = new();

    public void SetupResources(params RiseResourceEntry[] entries)
    {
        _entries = entries.ToList();
    }

    public void ClearDown()
    {
        _entries = [];
    }

    public Task<IReadOnlyCollection<RiseResourceEntry>> GetAllAsync()
        => Task.FromResult((IReadOnlyCollection<RiseResourceEntry>)_entries.AsReadOnly());
}
