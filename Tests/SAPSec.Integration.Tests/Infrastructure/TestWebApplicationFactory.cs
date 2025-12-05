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
using System.Net;
using System.Net.Sockets;

namespace SAPSec.Integration.Tests.Infrastructure;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private IHost? _host;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var port = GetAvailablePort();

        builder.UseUrls($"https://localhost:{port}");
        builder.UseEnvironment(Environments.Testing);

        ConfigureApplication(builder);
        ConfigureTestServices(builder);
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseContentRoot(Directory.GetCurrentDirectory());

        var testHost = builder.Build();

        _host = CreateKestrelHost(builder);
        _host.Start();

        SetBaseAddress();

        testHost.Start();
        return testHost;
    }

    protected override void Dispose(bool disposing)
    {
        _host?.Dispose();
        base.Dispose(disposing);
    }

    #region Configuration

    private static void ConfigureApplication(IWebHostBuilder builder)
    {
        var configuration = BuildTestConfiguration();

        builder
            .UseConfiguration(configuration)
            .ConfigureAppConfiguration(configBuilder =>
            {
                configBuilder.AddInMemoryCollection(GetConfigurationValues());
            });
    }

    private static IConfiguration BuildTestConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(GetConfigurationValues())
            .Build();
    }

    private static Dictionary<string, string?> GetConfigurationValues()
    {
        return new Dictionary<string, string?>
        {
            // Test Data
            [ConfigKeys.EstablishmentsCsvPath] = GetTestDataFilePath(),

            // DSI Configuration
            [ConfigKeys.DsiClientId] = TestValues.ClientId,
            [ConfigKeys.DsiClientSecret] = TestValues.ClientSecret,
            [ConfigKeys.DsiAuthority] = TestValues.Authority,
            [ConfigKeys.DsiRequireHttpsMetadata] = "false",
            [ConfigKeys.DsiValidateIssuer] = "false",
            [ConfigKeys.DsiValidateAudience] = "false",
            [ConfigKeys.DsiApiUri] = TestValues.ApiUri,
            [ConfigKeys.DsiApiSecret] = TestValues.ApiSecret,
            [ConfigKeys.DsiAudience] = TestValues.Audience,
            [ConfigKeys.DsiTokenExpiryMinutes] = TestValues.TokenExpiryMinutes
        };
    }

    private static string GetTestDataFilePath()
    {
        var testDataFilePath = Path.Combine(
            AppContext.BaseDirectory,
            TestValues.TestDataFolder,
            TestValues.TestDataFileName);

        if (!File.Exists(testDataFilePath))
        {
            throw new FileNotFoundException(
                $"Test data file not found at: {testDataFilePath}",
                testDataFilePath);
        }

        return testDataFilePath;
    }

    #endregion

    #region Service Configuration

    private static void ConfigureTestServices(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            RemoveRealServices(services);
            AddMockServices(services);
        });
    }

    private static void RemoveRealServices(IServiceCollection services)
    {
        services.RemoveAll<IUserService>();
        services.RemoveAll<IDsiClient>();
    }

    private static void AddMockServices(IServiceCollection services)
    {
        services.AddScoped<IUserService, MockDsiUserService>();
        services.AddScoped<IDsiClient, MockDsiApiService>();
    }

    #endregion

    #region Host Configuration

    private static IHost CreateKestrelHost(IHostBuilder builder)
    {
        builder.ConfigureWebHost(webHostBuilder => webHostBuilder.UseKestrel());
        return builder.Build();
    }

    private void SetBaseAddress()
    {
        var server = _host!.Services.GetRequiredService<IServer>();
        var addresses = server.Features.Get<IServerAddressesFeature>();

        ClientOptions.BaseAddress = addresses!.Addresses
            .Select(x => new Uri(x))
            .Last();

        LogServerStarted();
    }

    private void LogServerStarted()
    {
        Console.WriteLine($"✅ Integration test server started at: {ClientOptions.BaseAddress}");
    }

    private static int GetAvailablePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    #endregion

    #region Constants

    private static class Environments
    {
        public const string Testing = "Testing";
    }

    private static class ConfigKeys
    {
        // Establishments
        public const string EstablishmentsCsvPath = "Establishments:CsvPath";

        // DSI Configuration
        public const string DsiClientId = "DsiConfiguration:ClientId";
        public const string DsiClientSecret = "DsiConfiguration:ClientSecret";
        public const string DsiAuthority = "DsiConfiguration:Authority";
        public const string DsiRequireHttpsMetadata = "DsiConfiguration:RequireHttpsMetadata";
        public const string DsiValidateIssuer = "DsiConfiguration:ValidateIssuer";
        public const string DsiValidateAudience = "DsiConfiguration:ValidateAudience";
        public const string DsiApiUri = "DsiConfiguration:ApiUri";
        public const string DsiApiSecret = "DsiConfiguration:ApiSecret";
        public const string DsiAudience = "DsiConfiguration:Audience";
        public const string DsiTokenExpiryMinutes = "DsiConfiguration:TokenExpiryMinutes";
    }

    private static class TestValues
    {
        // Test Data
        public const string TestDataFolder = "TestData";
        public const string TestDataFileName = "Establishments-Integration-Test-Data.csv";

        // DSI Test Values
        public const string ClientId = "test-client-id";
        public const string ClientSecret = "test-client-secret";
        public const string Authority = "https://test-oidc.signin.education.gov.uk";
        public const string ApiUri = "https://test-api.signin.education.gov.uk";
        public const string ApiSecret = "test-api-secret";
        public const string Audience = "test-audience";
        public const string TokenExpiryMinutes = "60";
    }

    #endregion
}