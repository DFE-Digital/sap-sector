using System.Security.Cryptography;

namespace SAPSec.Web.Helpers;

public static class CspHelper
{
    public const string ScriptNonceKey = "ScriptNonce";

    public static string GenerateNonce()
    {
        var byteArray = new byte[32];
        using var generator = RandomNumberGenerator.Create();
        generator.GetBytes(byteArray);
        return Convert.ToBase64String(byteArray);
    }

    public static string BuildPolicy(string nonce)
    {
        return string.Join(" ",
            "base-uri 'self';",
            "object-src 'none';",
            "default-src 'self';",
            "frame-ancestors 'none';",
            "form-action 'self' https://test-oidc.signin.education.gov.uk https://pp-oidc.signin.education.gov.uk https://oidc.signin.education.gov.uk;",
            "connect-src 'self' *.google-analytics.com *.analytics.google.com https://www.compare-school-performance.service.gov.uk https://api.postcodes.io https://*.doubleclick.net https://*.clarity.ms https://c.bing.com https://*.applicationinsights.azure.com/ https://*.visualstudio.com/;",
            "img-src 'self' data: https://www.googletagmanager.com/ https://*.google-analytics.com https://atlas.microsoft.com https://*.clarity.ms https://c.bing.com https://js.monitor.azure.com/ https://*.tile.openstreetmap.org;",
            "style-src 'self' 'unsafe-inline';",
            "font-src 'self' data:;",
            $"script-src 'self' 'nonce-{nonce}' https://www.googletagmanager.com *.google-analytics.com https://*.clarity.ms https://c.bing.com https://js.monitor.azure.com/;");
    }
}
