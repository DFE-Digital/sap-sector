using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Web.Configuration;
using System.Text.RegularExpressions;

namespace SAPSec.Web.Controllers;

/// <summary>
/// Handles requests from client side /custom-event-tracking.
/// Uses NoOpCustomEventService in development as DfE Analytics is not available for local development.
/// </summary>
/// <param name="customEventService"></param>
[AllowAnonymous]
public class CustomEventController(ICustomEventService customEventService, IOptions<CustomEventPatterns> customEventPatterns) : Controller
{
    [HttpPost("/custom-event-tracking")]
    public async Task<IActionResult> CustomEventTracking([FromBody] ClickData clickData)
    {
        var patterns = new Dictionary<string, string> {
            { customEventPatterns.Value.FeedbackForm, "feedback_link_click" },
            { customEventPatterns.Value.SignIn, "cta_start_now_click" },
            { customEventPatterns.Value.MailTo, "mailto_link_click" } };

        foreach (var pattern in patterns)
        {
            Match match = Regex.Match(clickData.Url, pattern.Key);

            if (match.Success)
            {
                clickData.Text = match.Value;
                await customEventService.SendCustomEvent(clickData, pattern.Value);

                return Ok();
            }

        }

        Match matchServiceUrl = Regex.Match(clickData.Url, customEventPatterns.Value.ServiceUrls);

        if (!matchServiceUrl.Success)
        {
            await customEventService.SendCustomEvent(clickData, "outbound_link_click");

            return Ok();
        }

        //Don't record the request from '/custom-event-tracking' as an additional web request event.
        //Requests to the backend are already recorded as a web request event.
        await customEventService.IgnoreWebRequestEvent();

        return Ok();
    }
}
