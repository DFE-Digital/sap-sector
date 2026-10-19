namespace SAPSec.Core.Time;

public class CurrentTimeProvider : ICurrentTimeProvider
{
    public DateTime GetCurrentTimeUtc() => DateTime.UtcNow;
}
