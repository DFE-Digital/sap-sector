using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace SAPSec.Web.Middleware;

public class RequireAuthenticatedUserMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Allow health checks/static files/etc.
        var path = context.Request.Path.Value?.ToLowerInvariant();
        if (path != null && (path == "/" || path.StartsWith("/auth") || path.StartsWith("/health") || path.Contains("assets") || path.EndsWith("css") || path.EndsWith("js")))
        {
            Console.WriteLine($"skipping: {path}");
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