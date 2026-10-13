using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using SAPSec.Web.Authentication;
using System.Security.Claims;

namespace SAPSec.Web.Tests.Authentication;

public class AuthCookieTrimmerTests
{
    private static CookieSigningInContext CreateContext(ClaimsPrincipal principal, AuthenticationProperties properties)
    {
        var scheme = new AuthenticationScheme(
            CookieAuthenticationDefaults.AuthenticationScheme,
            null,
            typeof(CookieAuthenticationHandler));

        return new CookieSigningInContext(
            new DefaultHttpContext(),
            scheme,
            new CookieAuthenticationOptions(),
            principal,
            properties,
            new CookieOptions());
    }

    [Fact]
    public async Task HandleSigningIn_KeepsOnlyIdToken()
    {
        var properties = new AuthenticationProperties();
        properties.StoreTokens(new[]
        {
            new AuthenticationToken { Name = "access_token", Value = "access" },
            new AuthenticationToken { Name = "id_token", Value = "id" },
            new AuthenticationToken { Name = "refresh_token", Value = "refresh" },
            new AuthenticationToken { Name = "expires_at", Value = "later" }
        });
        var context = CreateContext(new ClaimsPrincipal(new ClaimsIdentity("test")), properties);

        await AuthCookieTrimmer.HandleSigningIn(context);

        var tokens = context.Properties.GetTokens().ToList();
        tokens.Should().ContainSingle();
        tokens[0].Name.Should().Be("id_token");
        tokens[0].Value.Should().Be("id");
    }

    [Fact]
    public async Task HandleSigningIn_RemovesUnusedAndDuplicateClaims()
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-1"),
            new Claim(ClaimTypes.Email, "user@example.com"),
            new Claim("organisation", "{\"id\":\"org-1\"}"),
            new Claim("organisation", "{\"id\":\"org-1\"}"),
            new Claim(ClaimTypes.Role, "a"),
            new Claim(ClaimTypes.Role, "b"),
            new Claim("sid", "session"),
            new Claim("auth_time", "123"),
            new Claim("amr", "pwd")
        }, "test");
        var context = CreateContext(new ClaimsPrincipal(identity), new AuthenticationProperties());

        await AuthCookieTrimmer.HandleSigningIn(context);

        identity.Claims.Select(c => (c.Type, c.Value)).Should().BeEquivalentTo(new[]
        {
            (ClaimTypes.NameIdentifier, "user-1"),
            (ClaimTypes.Email, "user@example.com"),
            ("organisation", "{\"id\":\"org-1\"}"),
            (ClaimTypes.Role, "a"),
            (ClaimTypes.Role, "b")
        });
    }
}
