using System.Diagnostics.CodeAnalysis;

namespace SAPSec.Web.Middleware;

[ExcludeFromCodeCoverage]
public class HostUrlBannerMiddleware(RequestDelegate next)
{
    static readonly string[] HostUrl = { "get-school-improvement-insights.education.gov.uk", "get-school-improvement-insights-test.test.teacherservices.cloud" };

    public async Task InvokeAsync(HttpContext context)
    {
        var host = context.Request.Host.Host;

        context.Items["ShowHostUrlBanner"] = false;

        if (HostUrl.Contains(host))
        {
            context.Items["ShowHostUrlBanner"] = true;
        }

        await next(context);
    }
}
