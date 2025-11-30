using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;  // ✅ Add this for ConfigureTestServices
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

        // ✅ Add configuration BEFORE Program.cs runs
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
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
            });
        });

        // ✅ Use ConfigureTestServices - runs AFTER Program.cs
        builder.ConfigureTestServices(services =>
        {
            // ✅ ALWAYS remove and replace DSI services (not just when using test auth)
            services.RemoveAll<IDsiUserService>();
            services.RemoveAll<IDsiApiService>();
            services.AddScoped<IDsiUserService, MockDsiUserService>();
            services.AddScoped<IDsiApiService, MockDsiApiService>();

            // ✅ Only replace authentication when using test auth
        });
    }
    /// <summary>
    /// Gets the path to the SAPSec.Web project directory
    /// </summary>
    private static string GetProjectPath()
    {
        // Get the path to the test assembly
        var startupAssembly = typeof(Program).Assembly;
        var projectName = "SAPSec.Web";

        // Get the target framework (e.g., net8.0)
        var currentDir = Directory.GetCurrentDirectory();

        // Navigate up from bin/Debug/net8.0 to find the solution root
        var directoryInfo = new DirectoryInfo(currentDir);

        while (directoryInfo != null)
        {
            // Check if SAPSec.Web exists at this level
            var webProjectPath = Path.Combine(directoryInfo.FullName, projectName);
            if (Directory.Exists(webProjectPath))
            {
                var viewsPath = Path.Combine(webProjectPath, "Views");
                if (Directory.Exists(viewsPath))
                {
                    return webProjectPath;
                }
            }

            // Also check if we're in the Tests folder structure
            var parentWebPath = Path.Combine(directoryInfo.FullName, "..", projectName);
            if (Directory.Exists(parentWebPath))
            {
                var fullPath = Path.GetFullPath(parentWebPath);
                var viewsPath = Path.Combine(fullPath, "Views");
                if (Directory.Exists(viewsPath))
                {
                    return fullPath;
                }
            }

            directoryInfo = directoryInfo.Parent;
        }

        // Fallback: use the assembly location to find the project
        var assemblyLocation = startupAssembly.Location;
        var assemblyDir = Path.GetDirectoryName(assemblyLocation);

        // Try common relative paths from test output
        var possiblePaths = new[]
        {
            Path.GetFullPath(Path.Combine(assemblyDir!, "..", "..", "..", "..", projectName)),
            Path.GetFullPath(Path.Combine(assemblyDir!, "..", "..", "..", "..", "..", projectName)),
            Path.GetFullPath(Path.Combine(assemblyDir!, "..", "..", "..", projectName)),
        };

        foreach (var path in possiblePaths)
        {
            if (Directory.Exists(path) && Directory.Exists(Path.Combine(path, "Views")))
            {
                return path;
            }
        }

        throw new DirectoryNotFoundException(
            $"Could not find {projectName} project directory. Current directory: {currentDir}");
    }
}