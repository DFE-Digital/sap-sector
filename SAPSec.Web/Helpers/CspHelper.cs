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

    public static string BuildPolicy(string nonce, IWebHostEnvironment environment)
    {
        if (environment.IsProduction())
        { 
        return string.Join(" ",
            "base-uri 'self';",
            "object-src 'none';",
            "default-src 'self';",
            "frame-ancestors 'none';",
            "form-action 'self' https://oidc.signin.education.gov.uk;",
            "connect-src 'self' *.google-analytics.com *.analytics.google.com https://*.clarity.ms https://c.bing.com;",
            "img-src 'self' data: https://www.googletagmanager.com/ https://*.google-analytics.com https://*.clarity.ms https://c.bing.com https://*.tile.openstreetmap.org;",
            "style-src 'self';",
            "font-src 'self' data:;",
            $"script-src 'self' 'nonce-{nonce}' https://www.googletagmanager.com *.google-analytics.com https://*.clarity.ms https://c.bing.com;");
        }
        else
        {
            return string.Join(" ",
            "base-uri 'self';",
            "object-src 'none';",
            "default-src 'self';",
            "frame-ancestors 'none';",
            "form-action 'self' https://test-oidc.signin.education.gov.uk https://pp-oidc.signin.education.gov.uk;",
            "connect-src 'self' *.google-analytics.com *.analytics.google.com https://*.clarity.ms https://c.bing.com https://*.visualstudio.com/ ws://localhost:* http://localhost:*;",
            "img-src 'self' data: https://www.googletagmanager.com/ https://*.google-analytics.com https://*.clarity.ms https://c.bing.com https://*.tile.openstreetmap.org;",
            "style-src 'self';",
            "font-src 'self' data:;",
            $"script-src 'self' 'nonce-{nonce}' https://www.googletagmanager.com *.google-analytics.com https://*.clarity.ms https://c.bing.com;");
         }

        //Notes:
        //https://c.bing.com is required by Clarity
        //https://*.visualstudio.com/ used for live share in Visual Studio
        //ws://localhost:* http://localhost:* used by https://browsersync.io/
    }
}
