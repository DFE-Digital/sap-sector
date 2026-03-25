using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model.Generated;
using SAPSec.Data;
using SAPSec.Infrastructure.Json;
using SAPSec.UI.Tests.Mocks;
using SAPSec.Web;

namespace SAPSec.UI.Tests.Infrastructure;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private IHost? _host;
    private static string? _cachedWebProjectPath;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseUrls("http://127.0.0.1:0", "https://127.0.0.1:0");
        builder.UseEnvironment("UITests");

        // Set content root to web project so static files (wwwroot) are found
        var webProjectPath = GetWebProjectPath();
        builder.UseContentRoot(webProjectPath);
        builder.UseWebRoot(Path.Combine(webProjectPath, "wwwroot"));

        var configurationValues = CreateConfigurationValues();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationValues)
            .Build();

        builder
            .UseConfiguration(configuration)
            .ConfigureAppConfiguration(configurationBuilder =>
            {
                configurationBuilder.AddInMemoryCollection(configurationValues);
            })
            .ConfigureServices(services =>
            {
                services.RemoveAll<IUserService>();
                services.RemoveAll<IDsiClient>();
                services.AddScoped<IUserService, MockUserService>();
                services.AddScoped<IDsiClient, MockDsiClient>();

                services.RemoveAll<IEstablishmentRepository>();
                services.RemoveAll<ISimilarSchoolsSecondaryRepository>();
                services.RemoveAll<IKs4PerformanceRepository>();
                services.RemoveAll<IKs4DestinationsRepository>();
                services.RemoveAll<IAbsenceRepository>();

                services.AddSingleton<IJsonFile<SimilarSchoolsSecondaryGroupsEntry>, JsonFile<SimilarSchoolsSecondaryGroupsEntry>>();
                services.AddSingleton<IJsonFile<SimilarSchoolsSecondaryValuesEntry>, JsonFile<SimilarSchoolsSecondaryValuesEntry>>();
                services.AddSingleton<IJsonFile<SimilarSchoolsSecondaryStandardDeviations>, JsonFile<SimilarSchoolsSecondaryStandardDeviations>>();
                services.AddSingleton<IJsonFile<Establishment>, JsonFile<Establishment>>();
                services.AddSingleton<IJsonFile<EstablishmentEmail>, JsonFile<EstablishmentEmail>>();
                services.AddSingleton<IJsonFile<EstablishmentPerformance>, JsonFile<EstablishmentPerformance>>();
                services.AddSingleton<IJsonFile<EstablishmentDestinations>, JsonFile<EstablishmentDestinations>>();
                services.AddSingleton<IJsonFile<EnglandAbsence>, JsonFile<EnglandAbsence>>();
                services.AddSingleton<IJsonFile<EnglandPerformance>, JsonFile<EnglandPerformance>>();
                services.AddSingleton<IJsonFile<EnglandDestinations>, JsonFile<EnglandDestinations>>();
                services.AddSingleton<IJsonFile<LAPerformance>, JsonFile<LAPerformance>>();
                services.AddSingleton<IJsonFile<LADestinations>, JsonFile<LADestinations>>();

                services.AddSingleton<IEstablishmentRepository, JsonEstablishmentRepository>();
                services.AddSingleton<ISimilarSchoolsSecondaryRepository, JsonSimilarSchoolsSecondaryRepository>();
                services.AddSingleton<IKs4PerformanceRepository, JsonKs4PerformanceRepository>();
                services.AddSingleton<IKs4DestinationsRepository, JsonKs4DestinationsRepository>();
                services.AddSingleton<IAbsenceRepository, JsonAbsenceRepository>();
            });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Use web project path for content root (for static files)
        var webProjectPath = GetWebProjectPath();
        builder.UseContentRoot(webProjectPath);

        // Create the host for TestServer
        var testHost = builder.Build();

        // Modify the host builder to use Kestrel with correct content root
        builder.ConfigureWebHost(webHostBuilder =>
        {
            webHostBuilder.UseKestrel();
            webHostBuilder.UseContentRoot(webProjectPath);
            webHostBuilder.UseWebRoot(Path.Combine(webProjectPath, "wwwroot"));
        });

        // Create and start the Kestrel server
        _host = builder.Build();
        _host.Start();

        // Extract the selected dynamic port
        var server = _host.Services.GetRequiredService<IServer>();
        var addresses = server.Features.Get<IServerAddressesFeature>();

        ClientOptions.BaseAddress = addresses!.Addresses
            .Select(x => new Uri(x))
            .Last();

        LogStartupInfo(webProjectPath);

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

    #region Path Resolution

    private static string GetWebProjectPath()
    {
        // Cache the path to avoid repeated file system walks
        if (_cachedWebProjectPath != null)
        {
            return _cachedWebProjectPath;
        }

        var currentDir = AppContext.BaseDirectory;
        var directory = new DirectoryInfo(currentDir);

        // Walk up to find solution root
        while (directory != null && !directory.GetFiles("*.sln").Any())
        {
            directory = directory.Parent;
        }

        if (directory == null)
        {
            throw new InvalidOperationException(
                $"Could not find solution root from {currentDir}");
        }

        // Find web project
        var possiblePaths = new[]
        {
            Path.Combine(directory.FullName, "SAPSec.Web"),
            Path.Combine(directory.FullName, "src", "SAPSec.Web"),
        };

        _cachedWebProjectPath = possiblePaths.FirstOrDefault(Directory.Exists)
            ?? throw new InvalidOperationException(
                $"Could not find SAPSec.Web. Searched: {string.Join(", ", possiblePaths)}");

        return _cachedWebProjectPath;
    }

    #endregion

    #region Configuration

    private static Dictionary<string, string?> CreateConfigurationValues()
    {
        return new Dictionary<string, string?>
        {
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
    }

    #endregion

    #region Logging

    private void LogStartupInfo(string webProjectPath)
    {
        var wwwrootPath = Path.Combine(webProjectPath, "wwwroot");
        var jsPath = Path.Combine(wwwrootPath, "js");

        if (Directory.Exists(jsPath))
        {
            var jsFiles = Directory.GetFiles(jsPath, "*.js", SearchOption.AllDirectories);
            // Check for key files
            var autocomplete = Path.Combine(jsPath, "accessible-autocomplete.min.js");
            var debounce = Path.Combine(jsPath, "lodash.debounce", "index.js");

        }
        else
        {
            Console.WriteLine($"❌ JS folder not found: {jsPath}");
        }
    }

    #endregion

    #region Constants

    private static class TestValues
    {
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
