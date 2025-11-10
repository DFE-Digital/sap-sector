using System.Security.Cryptography;

namespace SAPSec.Web.Middleware;

public class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

        // ✅ CRITICAL: Skip security headers for OAuth callbacks
        // These paths need to work with DSI's requirements
        if (path.StartsWith("/signin-oidc") ||
            path.StartsWith("/signout-callback-oidc") ||
            path.StartsWith("/auth/sign-in") ||
            path.StartsWith("/auth/sign-out"))
        {
            await next(context);
            return;
        }

        // Set script nonce early
        var nonce = GenerateRandom();
        context.Items["ScriptNonce"] = nonce;

        // Set security headers
        context.Response.Headers.Append("Expect-CT", "max-age=86400, enforce");
        context.Response.Headers.Append("Referrer-Policy", "same-origin");
        context.Response.Headers.Append("Arr-Disable-Session-Affinity", "true");
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-Permitted-Cross-Domain-Policies", "none");
        context.Response.Headers.Append("X-XSS-Protection", "0");
        context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");

        // ✅ FIXED: Only prevent caching for HTML pages, not cookies
        // Allow browser to cache cookies and static assets
        if (context.Request.Path.Value?.EndsWith(".html") == true ||
            context.Response.ContentType?.Contains("text/html") == true)
        {
            context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            context.Response.Headers.Append("Pragma", "no-cache");
            context.Response.Headers.Append("Expires", "0");
        }

        // ✅ UPDATED: CSP with form-action for OAuth redirects
        context.Response.Headers.Append(
            "Content-Security-Policy",
            "base-uri 'self';"
            + "object-src 'none';"
            + "default-src 'self';"
            + "frame-ancestors 'none';"
            + "form-action 'self' https://test-oidc.signin.education.gov.uk https://oidc.signin.education.gov.uk;"  // ✅ Allow DSI forms
            + "connect-src 'self' *.google-analytics.com *.analytics.google.com https://www.compare-school-performance.service.gov.uk https://api.postcodes.io https://*.doubleclick.net https://*.clarity.ms https://c.bing.com https://*.applicationinsights.azure.com/ https://*.visualstudio.com/ https://test-oidc.signin.education.gov.uk https://oidc.signin.education.gov.uk;"  // ✅ Add DSI
            + "child-src 'none';"
            + "frame-src 'none';"
            + "img-src 'self' data: https://www.googletagmanager.com/ https://*.google-analytics.com https://atlas.microsoft.com https://*.clarity.ms https://c.bing.com https://js.monitor.azure.com/;"
            + "style-src 'self' 'unsafe-inline';"  // ✅ Allow inline styles (needed for some frameworks)
            + "font-src 'self' data:;"
            + $"script-src 'self' 'nonce-{nonce}' https://www.googletagmanager.com *.google-analytics.com https://*.clarity.ms https://c.bing.com https://js.monitor.azure.com/;"
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