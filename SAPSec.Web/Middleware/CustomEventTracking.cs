using Dfe.Analytics.Events;

namespace SAPSec.Web.Middleware
{
    public static class CustomEventTracking
    {
        public static void MapCustomEventTracking(this IEndpointRouteBuilder app)
        {
            app.MapPost("/custom-event-tracking", async (ClickData data, IEventSender eventSender) =>
            {
                if (data.Url.StartsWith("https://forms.cloud.microsoft", StringComparison.OrdinalIgnoreCase))
                {
                    return await SendCustomEvent(eventSender, data, "feedback_link_click", "Feedback Link Click");
                }

                if (!data.Url.StartsWith("https://get-school-improvement-insights-pr-240.test.teacherservices.cloud", StringComparison.OrdinalIgnoreCase))
                {
                    return await SendCustomEvent(eventSender, data, "outbound_link_click", "Outbound Link Click");
                }

                return Results.NoContent();
            });
        }

        private static async Task<IResult> SendCustomEvent(IEventSender eventSender, ClickData data, string eventName, string eventLabel)
        {
            var customEvent = eventSender.CreateEvent(eventName);
            customEvent.AddData(eventLabel, data.Url, data.Text);

            await eventSender.SendEventAsync(customEvent);

            return Results.Redirect(data.Url);
        }
    }

    public record ClickData(string Url, string Text);

    //https://get-school-improvement-insights-pr-240.test.teacherservices.cloud
    //https://localhost:44300
}