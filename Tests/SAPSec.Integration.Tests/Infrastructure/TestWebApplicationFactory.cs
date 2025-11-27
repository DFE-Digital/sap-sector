using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SAPSec.Web;

namespace SAPSec.Integration.Tests.Infrastructure;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private bool _useTestAuth = false;
    private TestUserData? _testUserData;

    public TestWebApplicationFactory WithTestAuthentication(
        string userId = "test-user-123",
        string email = "test@example.com",
        string organisationId = "org-123",
        string organisationName = "Test Organisation")
    {
        _useTestAuth = true;
        _testUserData = new TestUserData
        {
            UserId = userId,
            Email = email,
            OrganisationId = organisationId,
            OrganisationName = organisationName
        };
        return this;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DsiConfiguration:ClientId"] = "test-client-id",
                ["DsiConfiguration:ClientSecret"] = "test-client-secret",
                ["DsiConfiguration:Authority"] = "https://test-oidc.signin.education.gov.uk",
                ["DsiConfiguration:RequireHttpsMetadata"] = "false",
                ["DsiConfiguration:ValidateIssuer"] = "false",
                ["DsiConfiguration:ValidateAudience"] = "false"
            });
        });

        if (_useTestAuth && _testUserData != null)
        {
            builder.ConfigureServices(services =>
            {
                // ✅ CRITICAL: Remove ALL authentication-related services
                var authServiceDescriptors = services
                    .Where(d => d.ServiceType.FullName?.Contains("Authentication") == true)
                    .ToList();

                foreach (var descriptor in authServiceDescriptors)
                {
                    services.Remove(descriptor);
                }

                // ✅ Remove specific authentication services
                services.RemoveAll<IAuthenticationService>();
                services.RemoveAll<IAuthenticationHandlerProvider>();
                services.RemoveAll<IAuthenticationSchemeProvider>();
                services.RemoveAll<AuthenticationOptions>();

                // ✅ Add fresh authentication with ONLY our test scheme
                services.AddAuthentication(options =>
                {
                    options.DefaultScheme = "TestScheme";
                    options.DefaultAuthenticateScheme = "TestScheme";
                    options.DefaultChallengeScheme = "TestScheme";
                    options.DefaultSignInScheme = "TestScheme";
                    options.DefaultSignOutScheme = "TestScheme";
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(
                    "TestScheme",
                    options => { });

                // ✅ Register test user data
                services.AddSingleton(_testUserData);
            });
        }
    }
}