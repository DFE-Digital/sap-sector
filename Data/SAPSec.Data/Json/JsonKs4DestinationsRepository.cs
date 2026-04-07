using SAPSec.Data.Model.Generated;

namespace SAPSec.Data.Json;

public class JsonKs4DestinationsRepository(
    IEstablishmentRepository establishmentRepository,
    IJsonFile<EstablishmentDestinations> establishmentDestinationsRepository,
    IJsonFile<LADestinations> localAuthorityDestinationsRepository,
    IJsonFile<EnglandDestinations> englandDestinationsRepository) : IKs4DestinationsRepository
{
    public async Task<Ks4DestinationsData?> GetByUrnAsync(string urn)
    {
        var results = await GetByUrnsAsync([urn]);
        return results.FirstOrDefault(x => string.Equals(x.Urn, urn, StringComparison.Ordinal));
    }

    public async Task<IReadOnlyCollection<Ks4DestinationsData>> GetByUrnsAsync(IEnumerable<string> urns)
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
        var destinationsByUrn = (await establishmentDestinationsRepository.ReadAllAsync())
            .Where(x => establishments.ContainsKey(x.Id))
            .ToDictionary(x => x.Id, StringComparer.Ordinal);

        var laIds = establishments.Values
            .Select(x => x.LAId)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var localAuthorityDestinationsByLaId = (await localAuthorityDestinationsRepository.ReadAllAsync())
            .Where(x => laIds.Contains(x.Id, StringComparer.Ordinal))
            .ToDictionary(x => x.Id, StringComparer.Ordinal);

        var englandDestinations = (await englandDestinationsRepository.ReadAllAsync()).FirstOrDefault();

        var results = new List<Ks4DestinationsData>(requestedUrns.Length);

        foreach (var urn in requestedUrns)
        {
            if (!establishments.TryGetValue(urn, out var establishment))
            {
                continue;
            }

            destinationsByUrn.TryGetValue(urn, out var establishmentDestinations);
            localAuthorityDestinationsByLaId.TryGetValue(establishment.LAId, out var localAuthorityDestinations);

            results.Add(new Ks4DestinationsData(
                urn,
                establishmentDestinations,
                localAuthorityDestinations,
                englandDestinations));
        }

        return results;
    }

}
