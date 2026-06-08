using SAPSec.Data.Dto;
using SAPSec.Data.Store;

namespace SAPSec.Data.Json;

public class JsonKs4PerformanceStore(
    IEstablishmentStore establishmentRepository,
    IJsonFile<EstablishmentPerformance> establishmentPerformanceJsonFile,
    IJsonFile<LAPerformance> localAuthorityPerformanceJsonFile,
    IJsonFile<EnglandPerformance> englandPerformanceJsonFile) : IKs4PerformanceStore
{
    public async Task<Ks4PerformanceData?> GetByUrnAsync(string urn)
    {
        var results = await GetByUrnsAsync([urn]);
        return results.FirstOrDefault(x => string.Equals(x.URN, urn, StringComparison.Ordinal));
    }

    public async Task<IReadOnlyCollection<Ks4PerformanceData>> GetByUrnsAsync(IEnumerable<string> urns)
    {
        var requestedUrns = urns
            .Where(urn => !string.IsNullOrWhiteSpace(urn))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (requestedUrns.Length == 0)
        {
            return [];
        }

        var establishments = (await establishmentRepository.GetEstablishmentsAsync(requestedUrns))
            .Where(x => !string.IsNullOrWhiteSpace(x.URN))
            .ToDictionary(x => x.URN, StringComparer.Ordinal);
        var performanceByUrn = (await establishmentPerformanceJsonFile.ReadAllAsync())
            .Where(x => establishments.ContainsKey(x.Id))
            .ToDictionary(x => x.Id, StringComparer.Ordinal);

        var laIds = establishments.Values
            .Select(x => x.LAId)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var localAuthorityPerformanceByLaId = (await localAuthorityPerformanceJsonFile.ReadAllAsync())
            .Where(x => laIds.Contains(x.Id, StringComparer.Ordinal))
            .ToDictionary(x => x.Id, StringComparer.Ordinal);

        var englandPerformance = (await englandPerformanceJsonFile.ReadAllAsync()).FirstOrDefault();

        var results = new List<Ks4PerformanceData>(requestedUrns.Length);

        foreach (var urn in requestedUrns)
        {
            if (!establishments.TryGetValue(urn, out var establishment))
            {
                continue;
            }

            performanceByUrn.TryGetValue(urn, out var establishmentPerformance);
            localAuthorityPerformanceByLaId.TryGetValue(establishment.LAId, out var localAuthorityPerformance);

            results.Add(new Ks4PerformanceData(
                urn,
                establishmentPerformance,
                localAuthorityPerformance,
                englandPerformance));
        }

        return results;
    }
}
