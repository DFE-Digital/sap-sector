using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SAPSec.Web;

namespace SAPSec.Tests.Integration;

// Custom factory for better control over the test environment
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove ALL authentication-related service descriptors
            var authServicesToRemove = services
                .Where(d =>
                    d.ServiceType.FullName?.Contains("Authentication") == true ||
                    d.ServiceType == typeof(IConfigureOptions<OpenIdConnectOptions>) ||
                    d.ServiceType == typeof(IPostConfigureOptions<OpenIdConnectOptions>) ||
                    d.ServiceType == typeof(IConfigureOptions<CookieAuthenticationOptions>) ||
                    d.ServiceType == typeof(IPostConfigureOptions<CookieAuthenticationOptions>) ||
                    d.ServiceType == typeof(IConfigureOptions<AuthenticationOptions>) ||
                    d.ServiceType == typeof(IPostConfigureOptions<AuthenticationOptions>))
                .ToList();

            foreach (var service in authServicesToRemove)
            {
                services.Remove(service);
            }

            

            // Clear and re-add authentication services with test configuration
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "TestScheme";
                options.DefaultAuthenticateScheme = "TestScheme";
                options.DefaultChallengeScheme = "TestScheme";
                options.DefaultSignInScheme = "TestScheme";
                options.DefaultSignOutScheme = "TestScheme";
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });
        });
        builder.ConfigureServices(services =>
        {
            services.AddDataProtection()
                .SetApplicationName("SAPSec.Tests");
        });
        builder.UseEnvironment("Test");
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Ensure the content root is set correctly for tests
        builder.UseContentRoot(Directory.GetCurrentDirectory());
        return base.CreateHost(builder);
    }
}