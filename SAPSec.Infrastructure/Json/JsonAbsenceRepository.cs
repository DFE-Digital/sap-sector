using SAPSec.Core.Features.Attendance;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Model.Generated;

namespace SAPSec.Infrastructure.Json;

public class JsonAbsenceRepository(
    IEstablishmentRepository establishmentRepository,
    IJsonFile<EstablishmentAbsence> establishmentAbsenceRepository,
    IJsonFile<LAAbsence> laAbsenceRepository,
    IJsonFile<EnglandAbsence> englandAbsenceRepository) : IAbsenceRepository
{
    public async Task<AbsenceData?> GetByUrnAsync(string urn)
    {
        var results = await GetByUrnsAsync([urn]);
        return results.FirstOrDefault(x => string.Equals(x.URN, urn, StringComparison.Ordinal));
    }

    public async Task<IReadOnlyCollection<AbsenceData>> GetByUrnsAsync(IEnumerable<string> urns)
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
        var absenceByUrn = (await establishmentAbsenceRepository.ReadAllAsync())
            .Where(x => establishments.ContainsKey(x.Id))
            .ToDictionary(x => x.Id, StringComparer.Ordinal);

        var laIds = establishments.Values
            .Select(x => x.LAId)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var localAuthorityAbsenceByLaId = (await laAbsenceRepository.ReadAllAsync())
            .Where(x => laIds.Contains(x.Id, StringComparer.Ordinal))
            .ToDictionary(x => x.Id, StringComparer.Ordinal);

        var englandAbsence = (await englandAbsenceRepository.ReadAllAsync()).FirstOrDefault();

        var results = new List<AbsenceData>(requestedUrns.Length);

        foreach (var urn in requestedUrns)
        {
            if (!establishments.TryGetValue(urn, out var establishment))
            {
                continue;
            }

            absenceByUrn.TryGetValue(urn, out var establishmentAbsence);
            localAuthorityAbsenceByLaId.TryGetValue(establishment.LAId, out var localAuthorityAbsence);

            results.Add(new AbsenceData(
                urn,
                establishmentAbsence,
                localAuthorityAbsence,
                englandAbsence));
        }

        return results;
    }
}
