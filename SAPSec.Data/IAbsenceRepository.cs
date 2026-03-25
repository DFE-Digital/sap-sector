using SAPSec.Core.Model.Generated;
using SAPSec.Data.Model.Generated;

namespace SAPSec.Data;

public interface IAbsenceRepository
{
    Task<AbsenceData?> GetByUrnAsync(string urn, string? laId);
}

public record AbsenceData(
    EstablishmentAbsence? EstablishmentAttendance,
    LAAbsence? LocalAuthorityAttendance,
    EnglandAbsence? EnglandAttendance);
