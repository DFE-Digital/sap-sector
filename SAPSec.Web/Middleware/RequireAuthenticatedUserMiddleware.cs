using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace SAPSec.Web.Middleware;

public class RequireAuthenticatedUserMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Allow health checks/static files/etc.
        var path = context.Request.Path.Value?.ToLowerInvariant();
        if (path == "/" ||
                 path.StartsWith("/auth") ||
                 path.StartsWith("/signin-oidc") ||              // ✅ ADD THIS!
                 path.StartsWith("/signout-callback-oidc") ||    // ✅ ADD THIS!
                 path.StartsWith("/health") ||
                 path.StartsWith("/healthcheck") ||
                 path.StartsWith("/error") ||                    // ✅ ADD THIS!
                 path.StartsWith("/home/error") ||               // ✅ ADD THIS!
                 path.Contains("/assets/") ||
                 path.EndsWith(".css") ||
                 path.EndsWith(".js") ||
                 path.EndsWith(".map") ||
                 path.EndsWith(".png") ||
                 path.EndsWith(".jpg") ||
                 path.EndsWith(".ico") ||
                 path.EndsWith(".svg"))
        {
            await next(context);
            return;
        }
        // If the user is NOT authenticated → trigger an OIDC challenge
        if (context.User.Identity is { IsAuthenticated: false })
        {
            await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme);
            return;
        }

        await next(context);
    }
}