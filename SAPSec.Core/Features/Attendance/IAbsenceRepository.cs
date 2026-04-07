using SAPSec.Core.Model.Generated;

namespace SAPSec.Core.Features.Attendance;

public interface IAbsenceRepository
{
    Task<AbsenceData?> GetByUrnAsync(string urn, string? laId);
}

public record AbsenceData(
    EstablishmentAbsence? EstablishmentAttendance,
    LAAbsence? LocalAuthorityAttendance,
    EnglandAbsence? EnglandAttendance);