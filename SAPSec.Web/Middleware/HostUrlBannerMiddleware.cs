using System.Diagnostics.CodeAnalysis;

namespace SAPSec.Web.Middleware;

[ExcludeFromCodeCoverage]
public class HostUrlBannerMiddleware(RequestDelegate next)
{
    const string HostUrl = "get-school-improvement-insights-pr-369.test.teacherservices.cloud";
    
    public async Task InvokeAsync(HttpContext context)
    {
        var host = context.Request.Host.Host;

        context.Items["ShowHostUrlBanner"] = false;

        if (host == HostUrl)
        {
            context.Items["ShowHostUrlBanner"] = true;
        }

        await next(context);
    }
}
