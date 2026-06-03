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
                    var customEvent = eventSender.CreateEvent("feedback_link_click");
                    customEvent.AddData("Feedback link click", data.Url, data.Text);

                    await eventSender.SendEventAsync(customEvent);

                    return Results.Redirect(data.Url);
                }

                if (!data.Url.StartsWith("https://get-school-improvement-insights-pr-240.test.teacherservices.cloud", StringComparison.OrdinalIgnoreCase))
                {
                    var customEvent = eventSender.CreateEvent("outbound_link_click");
                    customEvent.AddData("Outbound link click", data.Url, data.Text);

                    await eventSender.SendEventAsync(customEvent);

                    return Results.Redirect(data.Url);
                }

                return Results.NoContent();

            });

        }
    }

    public record ClickData(string Url, string Text);
}
