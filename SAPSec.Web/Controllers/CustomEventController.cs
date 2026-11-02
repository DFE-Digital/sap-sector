using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SAPSec.Core.CustomEvents;
using SAPSec.Web.Configuration;
using System.Text.RegularExpressions;

namespace SAPSec.Web.Controllers;

/// <summary>
/// Handles requests from client side /custom-event-tracking.
/// Uses NoOpCustomEventService in development as DfE Analytics is not available for local development.
/// </summary>
/// <param name="customEventService"></param>
[AllowAnonymous]
public class CustomEventController(ICustomEventService customEventService, IOptions<CustomEventLocations> customEventLocations) : Controller
{
    [HttpPost("/custom-event-tracking")]
    public async Task<IActionResult> CustomEventTracking([FromBody] EventData eventData)
    {
        switch (eventData.EventType)
        {
            case EventType.DetailsClick:
                await ProcessDetailsClick(eventData);
                break;

            case EventType.LinkClick:
                await ProcessLinkClick(eventData);
                break;

            default:
                return BadRequest($"Unsupported event type: {eventData.EventType}");
        }

        // Don't record the request from '/custom-event-tracking' as an additional web request event.
        // Requests to the backend are already recorded as a web request event.
        return Ok();
    }

    private async Task<IActionResult> ProcessDetailsClick(EventData eventData)
    {
        await customEventService.SendCustomEvent(eventData, "details_click");

        await customEventService.IgnoreWebRequestEvent();

        return Ok();
    }

    private async Task<IActionResult> ProcessLinkClick(EventData eventData)
    {
        if (eventData.LinkUrl.StartsWith(customEventLocations.Value.FeedbackForm))
        {
            eventData.LinkText = eventData.LinkUrl;
            await customEventService.SendCustomEvent(eventData, "feedback_link_click");

            return Ok();
        }

        if (eventData.LinkUrl.Contains(customEventLocations.Value.SignIn))
        {
            eventData.LinkText = eventData.LinkUrl;
            await customEventService.SendCustomEvent(eventData, "cta_start_now_click");

            return Ok();
        }

        if (eventData.LinkUrl.StartsWith(customEventLocations.Value.MailTo))
        {
            eventData.LinkText = eventData.LinkUrl;
            await customEventService.SendCustomEvent(eventData, "mailto_link_click");

            return Ok();
        }

        //check for outbound link clicks
        var serviceUrlsRegex = new Regex(string.Join("|", customEventLocations.Value.ServiceUrls.Select(url => $@"^{Regex.Escape(url)}/.*$")));

        Match match = serviceUrlsRegex.Match(eventData.LinkUrl);

        //"-pr-" checks for review app url
        if (!match.Success && !eventData.LinkUrl.Contains("-pr-"))
        {
            await customEventService.SendCustomEvent(eventData, "outbound_link_click");

            return Ok();
        }

        await customEventService.IgnoreWebRequestEvent();

        return Ok();
    }
}