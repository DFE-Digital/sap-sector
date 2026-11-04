using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SAPSec.Infrastructure.Json;

namespace SAPSec.Test.Integration.Setup;

public class JsonRepositoryIntegrationTestsWebApplicationFactory : IntegrationTestsWebApplicationFactory
{
    protected override IServiceCollection ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddJsonDependencies(configuration);

        return services;
    }
}
