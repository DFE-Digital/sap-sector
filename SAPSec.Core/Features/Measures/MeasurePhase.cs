namespace SAPSec.Core.Features.Measures;

public enum MeasurePhase
{
    Primary,
    Secondary
}

public static class MeasurePhaseExtensions
{
    public static string KeyPrefix(this MeasurePhase phase) =>
        phase is MeasurePhase.Primary ? "primary-" : "secondary-";
}
