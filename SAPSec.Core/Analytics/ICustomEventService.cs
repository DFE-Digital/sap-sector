namespace SAPSec.Core.Analytics;

public interface ICustomEventService  
{
    Task SendCustomEvent(ClickData clickData, string eventName);

    Task IgnoreWebRequestEvent();
}
