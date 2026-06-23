using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using SAPSec.Infrastructure.Json;
using SAPSec.Test.Common.Authentication;
using SAPSec.Web;

namespace SAPSec.Test.Integration.Setup;

public class IntegrationTestsWebApplicationFactory : WebApplicationFactory<Program>
{
    public IntegrationTestsWebApplicationFactory()
    {
        // Use HTTPS base address in TestServer to avoid invalid redirect port resolution.
        ClientOptions.BaseAddress = new Uri("https://localhost");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseUrls("http://127.0.0.1:0", "https://127.0.0.1:0");

        builder.UseEnvironment("IntegrationTests");

        var configuration = new ConfigurationBuilder()
            .AddTestDsiConfiguration()
            .Build();

        builder
            // This configuration is used during the creation of the application
            // (e.g. BEFORE WebApplication.CreateBuilder(args) is called in Program.cs).
            .UseConfiguration(configuration)
            .ConfigureAppConfiguration(configurationBuilder =>
            {
                configurationBuilder.AddTestDsiConfiguration();
            })
            .ConfigureServices(services =>
            {
                // Add or replace any services that the application needs during testing.
                services.AddTestDsiDependencies();
                services.AddJsonDependencies();
            });
    }
}
