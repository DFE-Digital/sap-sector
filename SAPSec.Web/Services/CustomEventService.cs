using Dfe.Analytics.AspNetCore;
using Dfe.Analytics.Events;
using SAPSec.Core.CustomEvents;

namespace SAPSec.Web.Services;

/// <summary>
/// Creates and sends custom events to Big Query using Dfe.Analytics.
/// </summary>
/// <param name="httpContextAccessor"></param>
/// <param name="eventSender"></param>
public class CustomEventService(IHttpContextAccessor httpContextAccessor, IEventSender eventSender) : ICustomEventService
{
    public async Task SendCustomEvent(EventData eventData, string eventName)
    {
        var customEvent = eventSender.CreateEvent(eventName);

        switch (eventData.EventType)
        {
            case EventType.LinkClick:
                customEvent.AddData(eventData.LinkText, eventData.LinkText);
                break;
            case EventType.DetailsClick:
                customEvent.AddData(eventData.DetailsSectionName, eventData.DetailsSectionLabel, eventData.DetailsInteractionType);
                break;
            default:
                throw new ArgumentException($"Unknown event type: {eventData.EventType}");
        }

        await eventSender.SendEventAsync(customEvent);

        // Ignore the web request event for this custom event to avoid creating a duplicate entry in Big Query
        httpContextAccessor?.HttpContext?.IgnoreWebRequestEvent();
    }


    public async Task IgnoreWebRequestEvent()
    {
        httpContextAccessor?.HttpContext?.IgnoreWebRequestEvent();
    }
}
