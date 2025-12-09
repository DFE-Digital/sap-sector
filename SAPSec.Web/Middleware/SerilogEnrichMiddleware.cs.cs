using Microsoft.AspNetCore.Http;
using Serilog.Context;
using System.Threading.Tasks;

namespace SAPSec.Web.Middleware;

public class SerilogEnrichMiddleware
{
    private readonly RequestDelegate _next;

    public SerilogEnrichMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        using (LogContext.PushProperty("RequestId", context.TraceIdentifier))
        using (LogContext.PushProperty("RequestPath", context.Request?.Path.Value))
        using (LogContext.PushProperty("RequestMethod", context.Request?.Method))
        using (LogContext.PushProperty("RemoteIpAddress", context.Connection?.RemoteIpAddress?.ToString()))
        {
            var userId = context.User?.FindFirst("sub")?.Value
                         ?? context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                         ?? context.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userId))
            {
                using (LogContext.PushProperty("UserId", userId))
                {
                    var orgId = context.User?.FindFirst("organisation")?.Value;
                    if (!string.IsNullOrEmpty(orgId))
                    {
                        using (LogContext.PushProperty("OrganisationId", orgId))
                        {
                            await _next(context);
                            return;
                        }
                    }

                    await _next(context);
                    return;
                }
            }

            await _next(context);
        }
    }
}