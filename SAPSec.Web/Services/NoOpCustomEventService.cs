using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;

namespace SAPSec.Web.Services;

/// <summary>
/// An implementation of ICustomEventService for development purposes.
/// Required as Dfe.Analytics is not available for local development.
/// </summary>
public class NoOpCustomEventService : ICustomEventService
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
