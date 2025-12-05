using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

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

        // ✅ Generate nonce and store in HttpContext
        var nonce = GenerateRandom();
        context.Items["ScriptNonce"] = nonce;

        context.Response.Headers.Append("Expect-CT", "max-age=86400, enforce");
        context.Response.Headers.Append("Referrer-Policy", "same-origin");
        context.Response.Headers.Append("Arr-Disable-Session-Affinity", "true");
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-Permitted-Cross-Domain-Policies", "none");
        context.Response.Headers.Append("X-XSS-Protection", "0");
        context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");

        context.Response.Headers.Append(
            "Content-Security-Policy",
            "base-uri 'self'; " +
            "object-src 'none'; " +
            "default-src 'self'; " +
            "frame-ancestors 'none'; " +
            "form-action 'self' https://test-oidc.signin.education.gov.uk https://oidc.signin.education.gov.uk; " +
            "connect-src 'self' *.google-analytics.com *.analytics.google.com https://www.compare-school-performance.service.gov.uk https://api.postcodes.io https://*.doubleclick.net https://*.clarity.ms https://c.bing.com https://*.applicationinsights.azure.com/ https://*.visualstudio.com/; " +
            "img-src 'self' data: https://www.googletagmanager.com/ https://*.google-analytics.com https://atlas.microsoft.com https://*.clarity.ms https://c.bing.com https://js.monitor.azure.com/; " +
            "style-src 'self' 'unsafe-inline'; " +
            "font-src 'self' data:; " +
            $"script-src 'self' 'nonce-{nonce}' https://www.googletagmanager.com *.google-analytics.com https://*.clarity.ms https://c.bing.com https://js.monitor.azure.com/;"  // ✅ Uses nonce
        );

        await next(context);
    }

    private static string GenerateRandom()
    {
        var byteArray = new byte[32];
        using (var generator = RandomNumberGenerator.Create())
        {
            generator.GetBytes(byteArray);
        }
        return Convert.ToBase64String(byteArray);
    }
}