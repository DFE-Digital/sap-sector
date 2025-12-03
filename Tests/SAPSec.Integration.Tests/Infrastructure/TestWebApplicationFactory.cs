using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
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
    private readonly Random _random = new();
    private IHost? _host;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var port = GetAvailablePort();  
        builder.UseUrls($"https://localhost:{port}");

        builder.UseEnvironment("Testing");

        // Find test data file
        var testDataFilePath = Path.Combine(AppContext.BaseDirectory, "TestData", "Establishments-Integration-Test-Data.csv");
        if (!File.Exists(testDataFilePath))
        {
            throw new FileNotFoundException("Test data file not found", testDataFilePath);
        }

        var configurationValues = new Dictionary<string, string?>
        {
            ["Establishments:CsvPath"] = testDataFilePath,
            ["DsiConfiguration:ClientId"] = "test-client-id",
            ["DsiConfiguration:ClientSecret"] = "test-client-secret",
            ["DsiConfiguration:Authority"] = "https://test-oidc.signin.education.gov.uk",
            ["DsiConfiguration:RequireHttpsMetadata"] = "false",
            ["DsiConfiguration:ValidateIssuer"] = "false",
            ["DsiConfiguration:ValidateAudience"] = "false",
            ["DsiConfiguration:ApiUri"] = "https://test-api.signin.education.gov.uk",
            ["DsiConfiguration:ApiSecret"] = "test-api-secret",
            ["DsiConfiguration:Audience"] = "test-audience",
            ["DsiConfiguration:TokenExpiryMinutes"] = "60"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationValues)
            .Build();

        builder
            .UseConfiguration(configuration)
            .ConfigureAppConfiguration(configurationBuilder =>
            {
                configurationBuilder.AddInMemoryCollection(configurationValues);
            });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IDsiUserService>();
            services.RemoveAll<IDsiApiService>();
            services.AddScoped<IDsiUserService, MockDsiUserService>();
            services.AddScoped<IDsiApiService, MockDsiApiService>();
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseContentRoot(Directory.GetCurrentDirectory());

        var testHost = builder.Build();

        builder.ConfigureWebHost(webHostBuilder => webHostBuilder.UseKestrel());

        _host = builder.Build();
        _host.Start();

        var server = _host.Services.GetRequiredService<IServer>();
        var addresses = server.Features.Get<IServerAddressesFeature>();
        ClientOptions.BaseAddress = addresses!.Addresses
            .Select(x => new Uri(x))
            .Last();

        Console.WriteLine($"✅ Test server started at: {ClientOptions.BaseAddress}");

        testHost.Start();
        return testHost;
    }

    protected override void Dispose(bool disposing)
    {
        _host?.Dispose();
        base.Dispose(disposing);
    }

    private static int GetAvailablePort()
    {
        var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}