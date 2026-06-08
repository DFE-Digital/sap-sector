using Dfe.Analytics.Events;

namespace SAPSec.Web.Services;

public class FakeEventSenderService : IEventSender
{
    public IEventFactory EventFactory => throw new NotImplementedException();

    public Task CreatedEvent(string eventName)
    {
        return Task.CompletedTask;
    }

    public Task SendEventAsync(Event @event)
    {
        return Task.CompletedTask;
    }
}
