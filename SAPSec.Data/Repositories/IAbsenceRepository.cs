using SAPSec.Data.Dto.Absence;

namespace SAPSec.Data.Repositories;

public interface IAbsenceRepository
{
    Task<AbsenceData?> GetByUrnAsync(string urn);
    Task<IReadOnlyCollection<AbsenceData>> GetByUrnsAsync(IEnumerable<string> urns);
}

public record AbsenceData(
    string Urn,
    EstablishmentAbsence? EstablishmentAbsence,
    LAAbsence? LocalAuthorityAbsence,
    EnglandAbsence? EnglandAbsence);