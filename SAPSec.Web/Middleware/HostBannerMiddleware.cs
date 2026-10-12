using System.Diagnostics.CodeAnalysis;

namespace SAPSec.Web.Middleware;

[ExcludeFromCodeCoverage]
public class HostBannerMiddleware(RequestDelegate next, IWebHostEnvironment environment)
{
    public async Task InvokeAsync(HttpContext context, IWebHostEnvironment environment)
    {
        var host = context.Request.Host.Host;

        context.Items["ShowBanner"] = false;

        if (host == "localhost")
        {
            context.Items["ShowBanner"] = true;
        }

        await next(context);
    }
}
