using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Integration.Tests.Mocks;
using SAPSec.Web;

namespace SAPSec.Integration.Tests.Infrastructure;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private IHost? _host;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseUrls("http://127.0.0.1:0", "https://127.0.0.1:0");

        builder.UseEnvironment("IntegrationTests");

        var testDataFilePath = Path.Combine(AppContext.BaseDirectory, "TestData", "Establishments-Integration-Test-Data.csv");

        if (!File.Exists(testDataFilePath)) throw new FileNotFoundException("Test data file not found", testDataFilePath);

        var configurationValues = new Dictionary<string, string?>
        {
            { "Establishments:CsvPath", testDataFilePath },
			{ "DsiConfiguration:ClientId", TestValues.ClientId },
            { "DsiConfiguration:ClientSecret", TestValues.ClientSecret },
            { "DsiConfiguration:Authority", TestValues.Authority },
            { "DsiConfiguration:RequireHttpsMetadata", "false" },
            { "DsiConfiguration:ValidateIssuer", "false" },
            { "DsiConfiguration:ValidateAudience", "false" },
            { "DsiConfiguration:ApiUri", TestValues.ApiUri },
            { "DsiConfiguration:ApiSecret", TestValues.ApiSecret },
            { "DsiConfiguration:Audience", TestValues.Audience },
            { "DsiConfiguration:TokenExpiryMinutes", TestValues.TokenExpiryMinutes }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationValues)
            .Build();

        builder
            // This configuration is used during the creation of the application
            // (e.g. BEFORE WebApplication.CreateBuilder(args) is called in Program.cs).
            .UseConfiguration(configuration)
            .ConfigureAppConfiguration(configurationBuilder =>
            {
                configurationBuilder.AddInMemoryCollection(configurationValues);
            })
            .ConfigureServices(services =>
            {
                // Add or replace any services that the application needs during testing.
				services.RemoveAll<IUserService>();
		        services.RemoveAll<IDsiClient>();
				services.AddScoped<IUserService, MockDsiUserService>();
        		services.AddScoped<IDsiClient, MockDsiApiService>();
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
        if (disposing)
        {
            _host?.Dispose();
        }
        base.Dispose(disposing);
    }

    private static class TestValues
    {
        // DSI Test Values
        public const string ClientId = "test-client-id";
        public const string ClientSecret = "test-client-secret";
        public const string Authority = "https://test-oidc.signin.education.gov.uk";
        public const string ApiUri = "https://test-api.signin.education.gov.uk";
        public const string ApiSecret = "test-api-secret";
        public const string Audience = "test-audience";
        public const string TokenExpiryMinutes = "60";
    }
}