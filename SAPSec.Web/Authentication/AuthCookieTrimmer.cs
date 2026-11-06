using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace SAPSec.Web.Authentication;

/// <summary>
/// Reduces the size of the authentication ticket stored in the auth cookie.
/// The whole ticket (claims + saved tokens) is serialised into the cookie and every chunk
/// is sent in a single Cookie header, so large tickets exceed the nginx header buffer.
/// </summary>
public static class AuthCookieTrimmer
{
    // id_token is needed as id_token_hint for federated sign-out; nothing else reads the saved tokens
    private static readonly HashSet<string> TokensToKeep = new(StringComparer.Ordinal)
    {
        "id_token"
    };

    // Claims read by UserService / authorisation - everything else is dropped from the cookie
    private static readonly HashSet<string> ClaimsToKeep = new(StringComparer.Ordinal)
    {
        "sub",
        ClaimTypes.NameIdentifier,
        "email",
        ClaimTypes.Email,
        "name",
        ClaimTypes.Name,
        "given_name",
        ClaimTypes.GivenName,
        "family_name",
        ClaimTypes.Surname,
        "organisation",
        "role",
        ClaimTypes.Role
    };

    public static Task HandleSigningIn(CookieSigningInContext context)
    {
        TrimTokens(context.Properties);

        if (context.Principal?.Identity is ClaimsIdentity identity)
        {
            TrimClaims(identity);
        }

        return Task.CompletedTask;
    }

    private static void TrimTokens(AuthenticationProperties properties)
    {
        var tokens = properties.GetTokens().ToList();
        if (tokens.Count == 0)
        {
            return;
        }

        properties.StoreTokens(tokens.Where(t => TokensToKeep.Contains(t.Name)));
    }

    private static void TrimClaims(ClaimsIdentity identity)
    {
        var seen = new HashSet<(string Type, string Value)>();

        foreach (var claim in identity.Claims.ToList())
        {
            if (!ClaimsToKeep.Contains(claim.Type) || !seen.Add((claim.Type, claim.Value)))
            {
                identity.RemoveClaim(claim);
            }
        }
    }
}
