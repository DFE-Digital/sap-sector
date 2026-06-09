using SAPSec.Data.Dto;

namespace SAPSec.Data.Store;

public interface IAbsenceStore
{
    Task<AbsenceData?> GetByUrnAsync(string urn);
    Task<IReadOnlyCollection<AbsenceData>> GetByUrnsAsync(IEnumerable<string> urns);
}

public record AbsenceData(
    string URN,
    EstablishmentAbsence? EstablishmentAbsence,
    LAAbsence? LocalAuthorityAbsence,
    EnglandAbsence? EnglandAbsence);