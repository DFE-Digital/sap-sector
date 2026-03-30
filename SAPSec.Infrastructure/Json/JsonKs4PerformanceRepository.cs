using SAPSec.Core.Features.Ks4HeadlineMeasures;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Model.Generated;

namespace SAPSec.Infrastructure.Json;

public class JsonKs4PerformanceRepository(
    IEstablishmentRepository establishmentRepository,
    IJsonFile<EstablishmentPerformance> establishmentPerformanceRepository,
    IJsonFile<LAPerformance> localAuthorityPerformanceRepository,
    IJsonFile<EnglandPerformance> englandPerformanceRepository,
    IJsonFile<EstablishmentDestinations> establishmentDestinationsRepository,
    IJsonFile<LADestinations> localAuthorityDestinationsRepository,
    IJsonFile<EnglandDestinations> englandDestinationsRepository) : IKs4PerformanceRepository
{
    public async Task<Ks4HeadlineMeasuresData?> GetByUrnAsync(string urn)
    {
        var results = await GetByUrnsAsync([urn]);
        return results.GetValueOrDefault(urn);
    }

    public async Task<IReadOnlyDictionary<string, Ks4HeadlineMeasuresData?>> GetByUrnsAsync(IEnumerable<string> urns)
    {
        var requestedUrns = urns
            .Where(urn => !string.IsNullOrWhiteSpace(urn))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (requestedUrns.Length == 0)
        {
            return new Dictionary<string, Ks4HeadlineMeasuresData?>(StringComparer.Ordinal);
        }

        var establishments = (await establishmentRepository.GetEstablishmentsAsync(requestedUrns))
            .Where(x => !string.IsNullOrWhiteSpace(x.URN))
            .ToDictionary(x => x.URN, StringComparer.Ordinal);
        var performanceByUrn = (await establishmentPerformanceRepository.ReadAllAsync())
            .Where(x => establishments.ContainsKey(x.Id))
            .ToDictionary(x => x.Id, StringComparer.Ordinal);
        var destinationsByUrn = (await establishmentDestinationsRepository.ReadAllAsync())
            .Where(x => establishments.ContainsKey(x.Id))
            .ToDictionary(x => x.Id, StringComparer.Ordinal);

        var laIds = establishments.Values
            .Select(x => x.LAId)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var localAuthorityPerformanceByLaId = (await localAuthorityPerformanceRepository.ReadAllAsync())
            .Where(x => laIds.Contains(x.Id, StringComparer.Ordinal))
            .ToDictionary(x => x.Id, StringComparer.Ordinal);
        var localAuthorityDestinationsByLaId = (await localAuthorityDestinationsRepository.ReadAllAsync())
            .Where(x => laIds.Contains(x.Id, StringComparer.Ordinal))
            .ToDictionary(x => x.Id, StringComparer.Ordinal);

        var englandPerformance = (await englandPerformanceRepository.ReadAllAsync()).FirstOrDefault();
        var englandDestinations = (await englandDestinationsRepository.ReadAllAsync()).FirstOrDefault();

        var results = new Dictionary<string, Ks4HeadlineMeasuresData?>(StringComparer.Ordinal);

        foreach (var urn in requestedUrns)
        {
            if (!establishments.TryGetValue(urn, out var establishment))
            {
                continue;
            }

            performanceByUrn.TryGetValue(urn, out var establishmentPerformance);
            destinationsByUrn.TryGetValue(urn, out var establishmentDestinations);
            localAuthorityPerformanceByLaId.TryGetValue(establishment.LAId, out var localAuthorityPerformance);
            localAuthorityDestinationsByLaId.TryGetValue(establishment.LAId, out var localAuthorityDestinations);

            results[urn] = new Ks4HeadlineMeasuresData(
                establishmentPerformance,
                localAuthorityPerformance,
                englandPerformance,
                establishmentDestinations,
                localAuthorityDestinations,
                englandDestinations);
        }

        return results;
    }
}
