using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SAPSec.Data.Json;
using SAPSec.Test.Common.Authentication;
using SAPSec.Web;

namespace SAPSec.Test.EndToEnd.Setup;

public class EndToEndTestsWebApplicationFactory : WebApplicationFactory<Program>
{
    private IHost? _host;
    private static string? _cachedWebProjectPath;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseUrls("http://127.0.0.1:0", "https://127.0.0.1:0");
        builder.UseEnvironment("EndToEndTests");

        // Set content root to web project so static files (wwwroot) are found
        var webProjectPath = GetWebProjectPath();
        builder.UseContentRoot(webProjectPath);
        builder.UseWebRoot(Path.Combine(webProjectPath, "wwwroot"));

        var configuration = new ConfigurationBuilder()
            .AddTestDsiConfiguration()
            .Build();

        builder
            .UseConfiguration(configuration)
            .ConfigureAppConfiguration(configurationBuilder =>
            {
                configurationBuilder.AddTestDsiConfiguration();
            })
            .ConfigureServices(services =>
            {
                services.AddTestDsiDependencies();
                services.AddJsonDependencies();
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
}
