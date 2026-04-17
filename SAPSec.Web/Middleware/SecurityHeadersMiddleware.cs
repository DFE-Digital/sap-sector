using System.Diagnostics.CodeAnalysis;
using SAPSec.Web.Helpers;

namespace SAPSec.Web.Middleware;

[ExcludeFromCodeCoverage]
public class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

        if (path.StartsWith("/signin-oidc") ||
            path.StartsWith("/signout-callback-oidc") ||
            path.StartsWith("/auth") ||
            path.StartsWith("/home/error") ||
            path.StartsWith("/error") ||
            path.StartsWith("/health"))
        {
            await next(context);
            return;
        }

        var nonce = CspHelper.GenerateNonce();
        context.Items[CspHelper.ScriptNonceKey] = nonce;

        context.Response.Headers.Append("Expect-CT", "max-age=86400, enforce");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Append("Arr-Disable-Session-Affinity", "true");
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-Permitted-Cross-Domain-Policies", "none");
        context.Response.Headers.Append("X-XSS-Protection", "0");
        context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
        context.Response.Headers.Append("Content-Security-Policy", CspHelper.BuildPolicy(nonce));

        await next(context);
    }
}
