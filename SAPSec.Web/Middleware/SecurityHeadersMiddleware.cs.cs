using SAPSec.Web.Helpers;

namespace SAPSec.Web.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Set script nonce early
        var nonce = CSPHelper.RandomCharacters;
        context.Items["ScriptNonce"] = nonce;

        // Set all security headers before processing the request
        context.Response.Headers.Append("Expect-CT", "max-age=86400, enforce");
        context.Response.Headers.Append("Referrer-Policy", "same-origin");
        context.Response.Headers.Append("Arr-Disable-Session-Affinity", "true");
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-Permitted-Cross-Domain-Policies", "none");
        context.Response.Headers.Append("X-XSS-Protection", "0");
        context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000;includeSubDomains; preload");
        context.Response.Headers.Append(
            "Content-Security-Policy",
            "base-uri 'self';"
            + "object-src 'none';"
            + "default-src 'self';"
            + "frame-ancestors 'none';"
            + "connect-src 'self' *.google-analytics.com *.analytics.google.com https://www.compare-school-performance.service.gov.uk https://api.postcodes.io https://*.doubleclick.net https://*.clarity.ms https://c.bing.com https://*.applicationinsights.azure.com/ https://*.visualstudio.com/; child-src 'none';"
            + "frame-src 'none';"
            + "img-src 'self' data: https://www.googletagmanager.com/ https://*.google-analytics.com https://atlas.microsoft.com https://*.clarity.ms https://c.bing.com https://js.monitor.azure.com/;"
            + "style-src 'self';"
            + "font-src 'self' data:;"
            + $"script-src 'self' 'nonce-{nonce}' https://www.googletagmanager.com *.google-analytics.com https://*.clarity.ms https://c.bing.com https://js.monitor.azure.com/;"
        );

        await _next(context);
    }
}

public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>();
    }
}