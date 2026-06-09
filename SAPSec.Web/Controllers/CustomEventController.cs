using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using System.Text.RegularExpressions;

namespace SAPSec.Web.Controllers;

[AllowAnonymous]
public class CustomEventController(ICustomEventService customEventService) : Controller
{
    //const string FeedbackForm = "https://forms.cloud.microsoft";
    //const string ServiceUrl = "https://get-school-improvement-insights.education.gov.uk/";
    //https://get-school-improvement-insights-test.test.teacherservices.cloud/
    //const string SignIn = "/auth/signin";
    //const string MailTo = "mailto:";

    const string FeedbackForm = "https://forms.cloud.microsoft";
    //const string ServiceUrl = "https://localhost:44300";
    const string SignIn = "/auth/signin";
    const string MailTo = "mailto:";
    const string External = "https://www";

    const string InboundLinkPattern = @"(?<=#).*$";
   // const string ServiceUrlPattern = @"https:\/\/localhost:44300\/.*$";
    //const string ServiceUrlPattern = @"https:\/\/https://get-school-improvement-insights.*$";
    // const string OverviewPagePattern = $"^{ServiceUrlPrefix}/school/\\d+$";

    [HttpPost("/custom-event-tracking")]
    public async Task<IActionResult> CustomEventTracking([FromBody] ClickData clickData)
    {
        if (clickData.Url.Contains(SignIn, StringComparison.OrdinalIgnoreCase))
        {
            await customEventService.SendCustomEvent(clickData, "cta_start_now_click");
        }

        if (clickData.Url.StartsWith(FeedbackForm, StringComparison.OrdinalIgnoreCase))
        {
            await customEventService.SendCustomEvent(clickData, "feedback_link_click");
        }

        if (clickData.Url.StartsWith(MailTo, StringComparison.OrdinalIgnoreCase))
        {
            await customEventService.SendCustomEvent(clickData, "mailto_link_click");
        }

        if (clickData.Url.StartsWith(External, StringComparison.OrdinalIgnoreCase))
        {
            await customEventService.SendCustomEvent(clickData, "outbound_link_click");
        }

        Match matchInbound = Regex.Match(clickData.Url, InboundLinkPattern);

        if (matchInbound.Success)
        {
            clickData.Text = matchInbound.Value;
            await customEventService.SendCustomEvent(clickData, "inbound_link_click");
        }

        //await customEventService.IgnoreWebRequestEvent();

        return Ok();
    }
}
