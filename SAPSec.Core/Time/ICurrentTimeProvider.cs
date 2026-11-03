namespace SAPSec.Core.Time;

public interface ICurrentTimeProvider
{
    DateTime GetCurrentTimeUtc();
}
