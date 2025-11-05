using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SAPSec.Web;

namespace SAPSec.Integration.Tests.Infrastructure;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly Random _random = new Random();
    private IHost? _host;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var port = _random.Next(5001, 5999);

        builder.UseUrls($"https://localhost:{port}");

        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((_, config) =>
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
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", _ => { });

        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseContentRoot(Directory.GetCurrentDirectory());

        // Create the host for TestServer now before we
        // modify the builder to use Kestrel instead.
        var testHost = builder.Build();

        // Modify the host builder to use Kestrel instead
        // of TestServer so we can listen on a real address.
        builder.ConfigureWebHost(webHostBuilder => webHostBuilder.UseKestrel());

        // Create and start the Kestrel server before the test server;
        // otherwise, due to the way the deferred host builder works
        // for minimal hosting, the server will not get "initialized
        // enough" for the address to be available.
        // See https://github.com/dotnet/aspnetcore/issues/33846.
        _host = builder.Build();
        _host.Start();

        // Extract the selected dynamic port out of the Kestrel server
        // and assign it onto the client options for convenience so it
        // "just works" as otherwise it'll be the default http://localhost
        // URL, which won't route to the Kestrel-hosted HTTP server.
         var server = _host.Services.GetRequiredService<IServer>();
         var addresses = server.Features.Get<IServerAddressesFeature>();

        ClientOptions.BaseAddress = addresses!.Addresses
            .Select(x => new Uri(x))
            .Last();

        // Return the host that uses TestServer, rather than the real one.
        // Otherwise, the internals will complain about the host's server
        // not being an instance of the concrete type TestServer.
        // See https://github.com/dotnet/aspnetcore/pull/34702.
        testHost.Start();
        return testHost;
    }

    protected override void Dispose(bool disposing)
    {
        _host?.Dispose();
    }
}