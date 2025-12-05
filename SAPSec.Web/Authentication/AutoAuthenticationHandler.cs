using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace SAPSec.Web.Authentication;

/// <summary>
/// Automatically authenticates all requests for UI testing
/// </summary>
public class AutoAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "UI Test User"),
            new Claim(ClaimTypes.NameIdentifier, "ui-test-user-123"),
            new Claim("sub", "ui-test-user-123"),
            new Claim("email", "uitest@example.com"),
            new Claim("organisation", "ui-test-org-123"),
            new Claim("organisation:name", "UI Test Organisation")
        };

        var identity = new ClaimsIdentity(claims, "TestScheme");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}