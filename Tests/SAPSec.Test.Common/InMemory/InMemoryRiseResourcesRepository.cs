using SAPSec.Data.Dto.RiseResources;
using SAPSec.Data.Repositories;

namespace SAPSec.Test.Common.InMemory;

public class InMemoryRiseResourcesRepository : IRiseResourcesRepository
{
    private IReadOnlyList<RiseResourceCategoryEntry> _categories = [];
    private IReadOnlyList<RiseResourceEntry> _entries = [];

    public void SetupCategories(params RiseResourceCategoryEntry[] categories)
    {
        _categories = categories;
    }

    public void SetupResources(params RiseResourceEntry[] entries)
    {
        _entries = entries;
    }

    public void ClearDown()
    {
        _categories = [];
        _entries = [];
    }

    public Task<RiseResourcesDocument> GetAsync() =>
        Task.FromResult(new RiseResourcesDocument
        {
            ResourceCategories = _categories,
            ResourceEntries = _entries
        });
}
