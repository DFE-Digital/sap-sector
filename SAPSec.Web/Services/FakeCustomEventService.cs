using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;

namespace SAPSec.Web.Services;

public class FakeCustomEventService : ICustomEventService
{
    public Task SendCustomEvent(ClickData clickData, string eventName)
    {
        return Task.CompletedTask;
    }
    public Task IgnoreWebRequestEvent()
    {
        return Task.CompletedTask;
    }
}
