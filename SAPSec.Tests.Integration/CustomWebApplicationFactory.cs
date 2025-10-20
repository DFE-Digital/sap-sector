using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using SAPSec.Web;

namespace SAPSec.Tests.Integration;
// Custom factory for better control over the test environment
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Add any test-specific service configurations here if needed
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