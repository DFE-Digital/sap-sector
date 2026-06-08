using Dfe.Analytics.AspNetCore;
using Dfe.Analytics.Events;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;

namespace SAPSec.Web.Services;

public class CustomEventService(IHttpContextAccessor httpContextAccessor, IEventSender eventSender) : ICustomEventService
{
    public async Task SendCustomEvent(ClickData clickData, string eventName)
    {
        var customEvent = eventSender.CreateEvent(eventName);
        customEvent.AddData(clickData.Text, clickData.Url);

        await eventSender.SendEventAsync(customEvent);

        httpContextAccessor?.HttpContext?.IgnoreWebRequestEvent();
    }
}
