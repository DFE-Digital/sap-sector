using SAPSec.Core.Features.Attendance;

namespace SAPSec.Integration.Tests.Mocks;

public class MockAttendanceRepository : IAttendanceRepository
{
    public Task<AttendanceMeasuresData?> GetByUrnAsync(string urn, string? laId = null)
    {
        return Task.FromResult<AttendanceMeasuresData?>(new AttendanceMeasuresData(
            new EstablishmentAttendance
            {
                Abs_Tot_Est_Current_Pct = 5.12m,
                Abs_Tot_Est_Previous_Pct = 5.08m,
                Abs_Tot_Est_Previous2_Pct = 4.97m,
                Abs_Persistent_Est_Current_Pct = 16.42m,
                Abs_Persistent_Est_Previous_Pct = 16.18m,
                Abs_Persistent_Est_Previous2_Pct = 15.96m
            },
            new LocalAuthorityAttendance
            {
                Abs_Tot_La_Current_Pct = 4.89m,
                Abs_Tot_La_Previous_Pct = 4.84m,
                Abs_Tot_La_Previous2_Pct = 4.81m,
                Abs_Persistent_La_Current_Pct = 15.78m,
                Abs_Persistent_La_Previous_Pct = 15.55m,
                Abs_Persistent_La_Previous2_Pct = 15.31m
            },
            new EnglandAttendance
            {
                Abs_Tot_Eng_Current_Pct = 4.91m,
                Abs_Tot_Eng_Previous_Pct = 4.83m,
                Abs_Tot_Eng_Previous2_Pct = 4.78m,
                Abs_Persistent_Eng_Current_Pct = 15.63m,
                Abs_Persistent_Eng_Previous_Pct = 15.41m,
                Abs_Persistent_Eng_Previous2_Pct = 15.22m
            }));
    }
}
