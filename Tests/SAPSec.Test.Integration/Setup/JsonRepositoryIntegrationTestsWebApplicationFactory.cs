using Microsoft.Extensions.DependencyInjection;
using SAPSec.Infrastructure.Json;

namespace SAPSec.Test.Integration.Setup;

public class JsonRepositoryIntegrationTestsWebApplicationFactory : IntegrationTestsWebApplicationFactory
{
    protected override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        services.AddJsonDependencies();

        return services;
    }
}
