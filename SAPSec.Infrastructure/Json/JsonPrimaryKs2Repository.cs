using SAPSec.Data.Dto.KS2.Performance;
using SAPSec.Data.Repositories;

namespace SAPSec.Infrastructure.Json;

public class JsonPrimaryKs2Repository(
    IEstablishmentRepository establishmentRepository,
    IJsonFileFactory jsonFileFactory) : IPrimaryKs2Repository
{
    private readonly IJsonFile<EstablishmentPerformance> _establishmentPerformanceRepository =
        jsonFileFactory.Create<EstablishmentPerformance>(JsonDataSource.PrimarySchools);
    private readonly IJsonFile<EstablishmentSubjectEntries> _establishmentSubjectEntriesRepository =
        jsonFileFactory.Create<EstablishmentSubjectEntries>(JsonDataSource.PrimarySchools);
    private readonly IJsonFile<LAPerformance> _localAuthorityPerformanceRepository =
        jsonFileFactory.Create<LAPerformance>(JsonDataSource.PrimarySchools);
    private readonly IJsonFile<LASubjectEntries> _localAuthoritySubjectEntriesRepository =
        jsonFileFactory.Create<LASubjectEntries>(JsonDataSource.PrimarySchools);
    private readonly IJsonFile<EnglandPerformance> _englandPerformanceRepository =
        jsonFileFactory.Create<EnglandPerformance>(JsonDataSource.PrimarySchools);

    public async Task<PrimaryKs2Data?> GetByUrnAsync(string urn)
    {
        var results = await GetByUrnsAsync([urn]);
        return results.FirstOrDefault(x => string.Equals(x.URN, urn, StringComparison.Ordinal));
    }

    public async Task<IReadOnlyCollection<PrimaryKs2Data>> GetByUrnsAsync(IEnumerable<string> urns)
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

        var establishmentPerformanceByUrn = (await _establishmentPerformanceRepository.ReadAllAsync())
            .Where(x => establishments.ContainsKey(x.Id))
            .ToDictionary(x => x.Id, StringComparer.Ordinal);

        var establishmentSubjectEntriesByUrn = (await _establishmentSubjectEntriesRepository.ReadAllAsync())
            .Where(x => !string.IsNullOrWhiteSpace(x.school_urn) && establishments.ContainsKey(x.school_urn))
            .GroupBy(x => x.school_urn, StringComparer.Ordinal)
            .ToDictionary(x => x.Key, x => (IReadOnlyCollection<EstablishmentSubjectEntries>)x.ToArray(), StringComparer.Ordinal);

        var laIds = establishments.Values
            .Select(x => x.LAId)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var localAuthorityPerformanceByLaId = (await _localAuthorityPerformanceRepository.ReadAllAsync())
            .Where(x => laIds.Contains(x.Id, StringComparer.Ordinal))
            .ToDictionary(x => x.Id, StringComparer.Ordinal);

        var localAuthoritySubjectEntriesByLaCode = (await _localAuthoritySubjectEntriesRepository.ReadAllAsync())
            .Where(x => !string.IsNullOrWhiteSpace(x.new_la_code))
            .GroupBy(x => x.new_la_code, StringComparer.Ordinal)
            .ToDictionary(x => x.Key, x => (IReadOnlyCollection<LASubjectEntries>)x.ToArray(), StringComparer.Ordinal);

        var englandPerformance = (await _englandPerformanceRepository.ReadAllAsync())
            .FirstOrDefault(x => string.Equals(x.Id, "National", StringComparison.OrdinalIgnoreCase))
            ?? (await _englandPerformanceRepository.ReadAllAsync()).FirstOrDefault();

        var results = new List<PrimaryKs2Data>(requestedUrns.Length);

        foreach (var urn in requestedUrns)
        {
            if (!establishments.TryGetValue(urn, out var establishment))
            {
                continue;
            }

            establishmentPerformanceByUrn.TryGetValue(urn, out var establishmentPerformance);
            establishmentSubjectEntriesByUrn.TryGetValue(urn, out var establishmentSubjectEntries);
            localAuthorityPerformanceByLaId.TryGetValue(establishment.LAId, out var localAuthorityPerformance);
            localAuthoritySubjectEntriesByLaCode.TryGetValue(ToPrimaryLaCode(establishment.LAId), out var localAuthoritySubjectEntries);

            results.Add(new PrimaryKs2Data(
                urn,
                establishmentPerformance,
                establishmentSubjectEntries ?? [],
                localAuthorityPerformance,
                localAuthoritySubjectEntries ?? [],
                englandPerformance));
        }

        return results;
    }

    private static string ToPrimaryLaCode(string laId)
    {
        return string.IsNullOrWhiteSpace(laId)
            ? string.Empty
            : $"E{laId.Trim().PadLeft(8, '0')}";
    }
}
