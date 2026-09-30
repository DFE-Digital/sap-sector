namespace SAPSec.Core.CustomEvents;

public interface ICustomEventService  
{
    Task SendCustomEvent(ClickData clickData, string eventName);

    Task IgnoreWebRequestEvent();
}
