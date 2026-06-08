using Dfe.Analytics.AspNetCore;
using Dfe.Analytics.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using System.Text.RegularExpressions;

namespace SAPSec.Web.Controllers;

[AllowAnonymous]
public class CustomEventController(ICustomEventService customEventService) : Controller
{
    const string FeedbackFormPrefix = "https://forms.cloud.microsoft";
    const string ServiceUrlPrefix = "https://get-school-improvement-insights-pr-240.test.teacherservices.cloud";
    const string SignInPrefix = "https://get-school-improvement-insights-pr-240.test.teacherservices.cloud/auth/signin";
    const string MailToPrefix = "mailto:";

    //const string FeedbackFormPrefix = "https://forms.cloud.microsoft";
    //const string ServiceUrlPrefix = "https://localhost:44300";
    //const string SignInPrefix = "https://localhost:44300/auth/signin";
    //const string MailToPrefix = "mailto:";

    const string InboundLinkPattern = @"(?<=#).*$";
    const string OverviewPagePattern = $"^{ServiceUrlPrefix}/school/\\d+$";

    [HttpPost("/custom-event-tracking")]
   // [IgnoreAntiforgeryToken]
    public async Task<IActionResult> CustomEventTracking([FromBody] ClickData clickData)
    {
        if (clickData.Url.StartsWith(SignInPrefix, StringComparison.OrdinalIgnoreCase))
        {
            await customEventService.SendCustomEvent(clickData, "cta_start_now_click");
        }

        if (clickData.Url.StartsWith(FeedbackFormPrefix, StringComparison.OrdinalIgnoreCase))
        {
            await customEventService.SendCustomEvent(clickData, "feedback_link_click");

            return Ok(new { success = true});
        }

        if (clickData.Url.StartsWith(MailToPrefix, StringComparison.OrdinalIgnoreCase))
        {
            await customEventService.SendCustomEvent(clickData, "mailto_link_click");
        }

        if (!clickData.Url.StartsWith(ServiceUrlPrefix, StringComparison.OrdinalIgnoreCase))
        {
            await customEventService.SendCustomEvent(clickData, "outbound_link_click");
        }

        Match matchInbound = Regex.Match(clickData.Url, InboundLinkPattern);

        if (matchInbound.Success)
        {
            clickData.Text = matchInbound.Value;
            await customEventService.SendCustomEvent(clickData, "inbound_link_click");
        }

        Match matchOverview = Regex.Match(clickData.Url, OverviewPagePattern);

        if (matchOverview.Success)
        {
            clickData.Text = matchOverview.Value;
            await customEventService.SendCustomEvent(clickData, "overview_page");
        }

        return Ok();
    }
}
