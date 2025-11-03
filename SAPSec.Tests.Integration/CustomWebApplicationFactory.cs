using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SAPSec.Web;

namespace SAPSec.Tests.Integration;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((context, config) =>
        {
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                // DFE Sign In configuration (prevents ClientId errors)
                ["DFESignInSettings:ClientId"] = "test-client-id",
                ["DFESignInSettings:ClientSecret"] = "test-client-secret",
                ["DFESignInSettings:Authority"] = "https://test-authority.com",
                ["DFESignInSettings:MetadataAddress"] = "https://test-authority.com/.well-known/openid-configuration",
            });
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
        });

        builder.ConfigureServices(services =>
        {
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
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseContentRoot(Directory.GetCurrentDirectory());
        return base.CreateHost(builder);
    }
}