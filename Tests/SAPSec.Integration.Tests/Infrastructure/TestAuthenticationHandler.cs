using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SAPSec.Integration.Tests.Infrastructure;

public class TestUserData
{
    public string UserId { get; set; } = "test-user";
    public string Email { get; set; } = "test@example.com";
    public string OrganisationId { get; set; } = "test-org";
    public string OrganisationName { get; set; } = "Test Organisation";
}

public class TestAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly TestUserData _userData;

    public TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        TestUserData userData)
        : base(options, logger, encoder)
    {
        _userData = userData;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.NameIdentifier, _userData.UserId),
            new Claim("sub", _userData.UserId),
            new Claim("email", _userData.Email),
            new Claim("organisation", _userData.OrganisationId),
            new Claim("organisation:name", _userData.OrganisationName)
        };

        var identity = new ClaimsIdentity(claims, "TestScheme");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    // ✅ Override challenge to prevent redirect
    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        // Don't redirect, just return 401
        Response.StatusCode = 401;
        return Task.CompletedTask;
    }
}