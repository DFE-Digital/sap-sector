namespace SAPSec.Web.Helpers;

public static class AttendanceAxisCalculator
{
    private const decimal AxisHeadroomMultiplier = 1.1m;

    public static AttendanceAxisSettings OverallAbsence { get; } = new(10m, 1m);
    public static AttendanceAxisSettings PersistentAbsence { get; } = new(30m, 5m);

    public static decimal CalculateMax(IEnumerable<decimal?> values, AttendanceAxisSettings settings)
    {
        var maxValue = values
            .Where(v => v.HasValue)
            .Select(v => v!.Value)
            .DefaultIfEmpty(0m)
            .Max();

        if (maxValue <= settings.DefaultMax)
        {
            return settings.DefaultMax;
        }

        var adjustedMax = maxValue * AxisHeadroomMultiplier;
        return Math.Ceiling(adjustedMax / settings.Step) * settings.Step;
    }

    public static AttendanceAxisSettings ForAbsenceType(bool isPersistentAbsence) =>
        isPersistentAbsence ? PersistentAbsence : OverallAbsence;
}

public readonly record struct AttendanceAxisSettings(decimal DefaultMax, decimal Step);
