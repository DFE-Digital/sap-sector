using SAPSec.Data.Dto.Exclusions;
using SAPSec.Data.Repositories;

namespace SAPSec.Infrastructure.Json;

public class JsonExclusionsRepository(
    IEstablishmentRepository establishmentRepository,
    IJsonFile<EstablishmentExclusions> establishmentExclusionsFile) : IExclusionsRepository
{
    public async Task<ExclusionsData?> GetByUrnAsync(string urn)
    {
        var results = await GetByUrnsAsync([urn]);
        return results.FirstOrDefault(x => string.Equals(x.Urn, urn, StringComparison.Ordinal));
    }

    public async Task<IReadOnlyCollection<ExclusionsData>> GetByUrnsAsync(IEnumerable<string> urns)
    {
        var requestedUrns = urns
            .Where(urn => !string.IsNullOrWhiteSpace(urn))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (requestedUrns.Length == 0)
        {
            return [];
        }

        var establishmentUrns = (await establishmentRepository.GetEstablishmentsAsync(requestedUrns))
            .Select(x => x.URN)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet(StringComparer.Ordinal);
        var exclusionsByUrn = (await establishmentExclusionsFile.ReadAllAsync())
            .Where(x => establishmentUrns.Contains(x.Id))
            .ToDictionary(x => x.Id, StringComparer.Ordinal);

        return requestedUrns
            .Where(establishmentUrns.Contains)
            .Select(urn => new ExclusionsData(urn, exclusionsByUrn.GetValueOrDefault(urn)))
            .ToList();
    }
}
