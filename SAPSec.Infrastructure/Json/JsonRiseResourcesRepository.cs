using SAPSec.Data.Dto.RiseResources;
using SAPSec.Data.Repositories;

namespace SAPSec.Infrastructure.Json;

/// <summary>
/// Reads the RISE resources content file once at construction and serves it from memory.
/// Registered as a singleton, so the file is parsed once per process. Fails fast
/// (throws) on a missing or malformed file rather than serving an empty result.
/// </summary>
public class JsonRiseResourcesRepository(IJsonFile<RiseResourceEntry> riseResourcesFile) : IRiseResourcesRepository
{
    public async Task<IReadOnlyCollection<RiseResourceEntry>> GetAllAsync()
    {
        var rows = await  riseResourcesFile.ReadAllAsync();
        return [.. rows];
    }
}
