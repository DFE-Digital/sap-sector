using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Web.Configuration;
using System.Runtime.CompilerServices;
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
    public async Task<IActionResult> CustomEventTracking([FromBody] ClickData clickData)
    {

      //  var feedbackFormRegex = new Regex($".*{Regex.Escape(customEventLocations.Value.FeedbackForm)}.+$");

       // Match match = feedbackFormRegex.Match(clickData.Url);

        if (clickData.Url.StartsWith(customEventLocations.Value.FeedbackForm))
        {
            clickData.Text = clickData.Url;
            await customEventService.SendCustomEvent(clickData, "feedback_link_click");

            return Ok();
        }


        //var signInRegex = new Regex($".*{Regex.Escape(customEventLocations.Value.SignIn)}.+$");

      //  match = signInRegex.Match(clickData.Url);

        if (clickData.Url.Contains(customEventLocations.Value.SignIn))
        {
            clickData.Text = clickData.Url;
            await customEventService.SendCustomEvent(clickData, "cta_start_now_click");

            return Ok();
        }


        //var mailToRegex = new Regex($"^{Regex.Escape(customEventLocations.Value.MailTo)}.+$");

        //match = mailToRegex.Match(clickData.Url);

        if (clickData.Url.StartsWith(customEventLocations.Value.MailTo))
        {
            clickData.Text = clickData.Url;
            await customEventService.SendCustomEvent(clickData, "mailto_link_click");

            return Ok();
        }


        var serviceUrlsRegex = new Regex(string.Join("|", customEventLocations.Value.ServiceUrls.Select(url => $@"^{Regex.Escape(url)}/.*$")));

        Match match = serviceUrlsRegex.Match(clickData.Url);

        if (!match.Success || !clickData.Url.Contains("-pr-"))
        {
            await customEventService.SendCustomEvent(clickData, "outbound_link_click");

            return Ok();
        }


        //var reviewAppUrlRegex = new Regex(".*-pr-\\d+\\.test");

        //match = reviewAppUrlRegex.Match(clickData.Url);

        //if (!match.Success)
        //{
        //    await customEventService.SendCustomEvent(clickData, "outbound_link_click");

        //    return Ok();
        //}

        //Don't record the request from '/custom-event-tracking' as an additional web request event.
        //Requests to the backend are already recorded as a web request event.
        await customEventService.IgnoreWebRequestEvent();

        return Ok();
    }
}
