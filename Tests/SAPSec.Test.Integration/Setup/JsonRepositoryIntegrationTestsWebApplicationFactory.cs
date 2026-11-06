using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SAPSec.Core.Caching;
using SAPSec.Infrastructure.Json;
using SAPSec.Web.Extensions;

namespace SAPSec.Test.Integration.Setup;

public class JsonRepositoryIntegrationTestsWebApplicationFactory : IntegrationTestsWebApplicationFactory
{
    protected override IServiceCollection ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddNestedCache<RequestCache, InMemoryCache>();
        services.AddJsonDependencies(configuration);

        return services;
    }
}
