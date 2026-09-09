using SAPSec.Data.Dto.RiseResources;
using SAPSec.Data.Repositories;

namespace SAPSec.Infrastructure.Json;

public class JsonRiseResourcesRepository(IJsonFile<RiseResourcesDocument> riseResourcesFile) : IRiseResourcesRepository
{
    public async Task<RiseResourcesDocument> GetAsync()
    {
        var documents = await riseResourcesFile.ReadAllAsync();

        return documents.FirstOrDefault()
            ?? throw new InvalidOperationException(
                "RISE resources content is missing or could not be read.");
    }
}
