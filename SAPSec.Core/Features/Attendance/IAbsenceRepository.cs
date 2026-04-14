using SAPSec.Core.Model.Generated;

namespace SAPSec.Core.Features.Attendance;

public interface IAbsenceRepository
{
    Task<AbsenceData?> GetByUrnAsync(string urn);
    Task<IReadOnlyCollection<AbsenceData>> GetByUrnsAsync(IEnumerable<string> urns);
}

public record AbsenceData(
    string URN,
    EstablishmentAbsence? EstablishmentAbsence,
    LAAbsence? LocalAuthorityAbsence,
    EnglandAbsence? EnglandAbsence);