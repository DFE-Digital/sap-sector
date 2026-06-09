using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using System.Text.RegularExpressions;

namespace SAPSec.Web.Controllers;

/// <summary>
/// Handles requests from client side /custom-event-tracking.
/// Used FakeCustomEventService in development as Dfe.Analytics is not available for local development.
/// </summary>
/// <param name="customEventService"></param>
[AllowAnonymous]
public class CustomEventController(ICustomEventService customEventService) : Controller
{
    const string FeedbackForm = @"^https:\/\/forms.cloud.microsoft.+$";
    const string SignIn = @".*\/auth\/signin.*$";
    const string MailTo = @"^mailto:.+$";
   // const string ServiceUrl = @"^(?!(https://get-school-improvement-insights\.education\.gov\.uk/.*$|https://get-school-improvement-insights-test\.test\.teacherservices\.cloud/.*$))";

    const string ServiceUrl = @"^https:\/\/get-school-improvement-insights-pr-240\.test\.teacherservices\.cloud\/.*$";
    // const string ServiceUrl = @"^https:\/\/localhost:44300\/.*$";

    [HttpPost("/custom-event-tracking")]
    public async Task<IActionResult> CustomEventTracking([FromBody] ClickData clickData)
    {
        var patterns = new Dictionary<string, string> {
            { FeedbackForm, "feedback_link_click" },
            { SignIn, "cta_start_now_click" },
            { MailTo, "mailto_link_click" } };

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

        Match matchServiceUrl = Regex.Match(clickData.Url, ServiceUrl);

        if (!matchServiceUrl.Success)
        {
            clickData.Text = matchServiceUrl.Value;
            await customEventService.SendCustomEvent(clickData, "outbound_link_click");

            return Ok();
        }

        await customEventService.IgnoreWebRequestEvent();

        return Ok();
    }
}
