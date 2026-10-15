using System.Diagnostics.CodeAnalysis;

namespace SAPSec.Web.Middleware;

[ExcludeFromCodeCoverage]
public class HostUrlBannerMiddleware(RequestDelegate next)
{
    //const string HostUrl = "get-school-improvement-insights-pr-390.test.teacherservices.cloud";
    const string HostUrl = "localhost";

    public async Task InvokeAsync(HttpContext context)
    {
        var host = context.Request.Host.Host;

        context.Items["ShowBanner"] = false;

        if (host == HostUrl)
        {
            context.Items["ShowBanner"] = true;
        }

        await next(context);
    }
}
