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
    public async Task<AbsenceData?> GetByUrnAsync(string urn, string? laId)
    {
        var establishment = await establishmentRepository.GetEstablishmentAsync(urn);
        if (string.IsNullOrWhiteSpace(establishment?.URN))
        {
            return null;
        }

        var establishmentAbsence = (await establishmentAbsenceRepository.ReadAllAsync())
            .FirstOrDefault(p => p.Id == urn);

        var laAbsence = (await laAbsenceRepository.ReadAllAsync())
            .FirstOrDefault(p => p.Id == laId);

        var englandAbsence = (await englandAbsenceRepository.ReadAllAsync())
            .FirstOrDefault();

        return new(
            establishmentAbsence,
            laAbsence,
            englandAbsence);
    }
}
