using SAPSec.Core.Model;

namespace SAPSec.Core.Interfaces.Services;

public interface ICustomEventService  
{
    Task SendCustomEvent(ClickData clickData, string eventName);

    Task IgnoreWebRequestEvent();
}
