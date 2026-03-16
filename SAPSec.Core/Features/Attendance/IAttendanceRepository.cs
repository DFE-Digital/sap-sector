namespace SAPSec.Core.Features.Attendance;

public interface IAttendanceRepository
{
    Task<AttendanceMeasuresData?> GetByUrnAsync(string urn);
}

public record AttendanceMeasuresData(
    EstablishmentAttendance? EstablishmentAttendance,
    EnglandAttendance? EnglandAttendance);

public sealed class EstablishmentAttendance
{
    public decimal? Abs_Tot_Est_Current_Pct { get; set; }
    public decimal? Abs_Tot_Est_Previous_Pct { get; set; }
    public decimal? Abs_Tot_Est_Previous2_Pct { get; set; }
    public decimal? Abs_Persistent_Est_Current_Pct { get; set; }
    public decimal? Abs_Persistent_Est_Previous_Pct { get; set; }
    public decimal? Abs_Persistent_Est_Previous2_Pct { get; set; }
}

public sealed class EnglandAttendance
{
    public decimal? Abs_Tot_Eng_Current_Pct { get; set; }
    public decimal? Abs_Tot_Eng_Previous_Pct { get; set; }
    public decimal? Abs_Tot_Eng_Previous2_Pct { get; set; }
    public decimal? Abs_Persistent_Eng_Current_Pct { get; set; }
    public decimal? Abs_Persistent_Eng_Previous_Pct { get; set; }
    public decimal? Abs_Persistent_Eng_Previous2_Pct { get; set; }
}
