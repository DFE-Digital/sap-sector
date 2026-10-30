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
    private const string DetailsClick = "details_click";
    private const string LinkClick = "link_click";

    [HttpPost("/custom-event-tracking")]
    public async Task<IActionResult> CustomEventTracking([FromBody] ClickData clickData)
    {
        //if (clickData is null || string.IsNullOrWhiteSpace(clickData.EventType))
        //{
        //    return BadRequest("EventType is required.");
        //}

        switch (clickData.EventType)
        {
            case DetailsClick:
                await ProcessDetailsClick(clickData);
                break;

            case LinkClick:
                await ProcessLinkClick(clickData);
                break;

            default:
                return BadRequest($"Unsupported event type: {clickData.EventType}");
        }

        // Don't record the request from '/custom-event-tracking' as an additional web request event.
        // Requests to the backend are already recorded as a web request event.
        return Ok();
    }

    private async Task<IActionResult> ProcessDetailsClick(ClickData clickData)
    {
        await customEventService.SendCustomEvent(clickData, "details_click");

        await customEventService.IgnoreWebRequestEvent();

        return Ok();
    }

    private async Task<IActionResult> ProcessLinkClick(ClickData clickData)
    {
        if (clickData.LinkUrl.StartsWith(customEventLocations.Value.FeedbackForm))
        {
            clickData.LinkText = clickData.LinkUrl;
            await customEventService.SendCustomEvent(clickData, "feedback_link_click");

            return Ok();
        }

        if (clickData.LinkUrl.Contains(customEventLocations.Value.SignIn))
        {
            clickData.LinkText = clickData.LinkUrl;
            await customEventService.SendCustomEvent(clickData, "cta_start_now_click");

            return Ok();
        }

        if (clickData.LinkUrl.StartsWith(customEventLocations.Value.MailTo))
        {
            clickData.LinkText = clickData.LinkUrl;
            await customEventService.SendCustomEvent(clickData, "mailto_link_click");

            return Ok();
        }

        //check for outbound link clicks
        var serviceUrlsRegex = new Regex(string.Join("|", customEventLocations.Value.ServiceUrls.Select(url => $@"^{Regex.Escape(url)}/.*$")));

        Match match = serviceUrlsRegex.Match(clickData.LinkUrl);

        //"-pr-" checks for review app url
        if (!match.Success && !clickData.LinkUrl.Contains("-pr-"))
        {
            await customEventService.SendCustomEvent(clickData, "outbound_link_click");

            return Ok();
        }

        await customEventService.IgnoreWebRequestEvent();

        return Ok();
    }
}
