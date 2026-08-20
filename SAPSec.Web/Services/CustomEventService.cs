using Dfe.Analytics.AspNetCore;
using Dfe.Analytics.Events;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;

namespace SAPSec.Web.Services;

/// <summary>
/// Creates and sends custom events to Big Query using Dfe.Analytics.
/// </summary>
/// <param name="httpContextAccessor"></param>
/// <param name="eventSender"></param>
public class CustomEventService(IHttpContextAccessor httpContextAccessor, IEventSender eventSender) : ICustomEventService
{
    public async Task SendCustomEvent(ClickData clickData, string eventName)
    {
        var customEvent = eventSender.CreateEvent(eventName);
        customEvent.AddData(clickData.Text, clickData.Url);

        await eventSender.SendEventAsync(customEvent);


        //Ignore the web request event for this custom event to avoid creating a duplicate entry in Big Query
        httpContextAccessor?.HttpContext?.IgnoreWebRequestEvent();
    }


    public async Task IgnoreWebRequestEvent()
    {
        httpContextAccessor?.HttpContext?.IgnoreWebRequestEvent();
    }
}
