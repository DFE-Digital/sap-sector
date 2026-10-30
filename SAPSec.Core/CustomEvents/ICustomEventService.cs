namespace SAPSec.Core.CustomEvents;

public interface ICustomEventService  
{
    Task SendCustomEvent(EventData eventData, string eventName);

    Task IgnoreWebRequestEvent();
}
