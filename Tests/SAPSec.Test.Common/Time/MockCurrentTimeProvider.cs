using SAPSec.Core.Time;

namespace SAPSec.Test.Common.Time;

public class MockCurrentTimeProvider : ICurrentTimeProvider
{
    private DateTime? _override = null;

    public void Override(DateTime time) =>
        _override = time;

    public void ClearOverride() =>
        _override = null;

    public DateTime GetCurrentTimeUtc() => _override ?? DateTime.UtcNow;
}
