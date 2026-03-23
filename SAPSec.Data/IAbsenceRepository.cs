using SAPSec.Core.Model.Generated;

namespace SAPSec.Core.Features.Attendance;

public interface IAbsenceRepository
{
    Task<AbsenceData?> GetByUrnAsync(string urn);
}

public record AbsenceData(
    EstablishmentAbsence? EstablishmentAttendance,
    EnglandAbsence? EnglandAttendance);
