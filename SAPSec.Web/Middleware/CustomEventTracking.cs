using Dfe.Analytics.AspNetCore;
using Dfe.Analytics.Events;
using System.Text.RegularExpressions;

namespace SAPSec.Web.Middleware
{
    public static class CustomEventTracking
    {
        public static void MapCustomEventTracking(this IEndpointRouteBuilder app)
        {
            app.MapPost("/custom-event-tracking", async (ClickData data, HttpContext httpContext, IEventSender eventSender) =>
            {
                //Capture clicks on phase banner feedback link
                if (data.Url.StartsWith("https://forms.cloud.microsoft", StringComparison.OrdinalIgnoreCase))
                {
                    return await SendCustomEvent(eventSender, data, "feedback_link_click");
                }

                //Capture all external link clicks
                if (!data.Url.StartsWith("https://get-school-improvement-insights-pr-240.test.teacherservices.cloud", StringComparison.OrdinalIgnoreCase))
                {
                    return await SendCustomEvent(eventSender, data, "outbound_link_click");
                }

                //Capture inbound link clicks
                //e.g. - /school/136994/ks4-headline-measures#year-by-year
                //capture the text after the # symbol
                Match match = Regex.Match(data.Url, @"(?<=#).*$");

                if (match.Success)
                {
                    data.Text = match.Value;
                    return await SendCustomEvent(eventSender, data, "inbound_link_click");
                }

                //If the data passed to /custom-event-tracking is not one of the above then don't record a web request event
                httpContext.IgnoreWebRequestEvent();

                return Results.NoContent();
            });
        }

        private static async Task<IResult> SendCustomEvent(IEventSender eventSender, ClickData data, string eventName)
        {
            var customEvent = eventSender.CreateEvent(eventName);
            customEvent.AddData(data.Text, data.Url);

            await eventSender.SendEventAsync(customEvent);

            return Results.Redirect(data.Url);
        }
    }

    public record ClickData
    {
        public string Text { get; set; }
        public string Url { get; set; }
    }

    //https://get-school-improvement-insights-pr-240.test.teacherservices.cloud
    //https://localhost:44300
    //check for clicks on Start button on homepage
    //scroll depth
    //ignoring certain webrequest events 
    //Capture when someone visits the Overview page
    //Inbound link clicks - things that start with #
    //school details interaction
    //disclosure interactions
    
}