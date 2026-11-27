using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SAPSec.Core.Caching;
using SAPSec.Infrastructure.Json;
using SAPSec.Web.Caching;
using SAPSec.Web.Extensions;

namespace SAPSec.Test.Integration.Setup;

public class JsonRepositoryIntegrationTestsWebApplicationFactory : IntegrationTestsWebApplicationFactory
{
    protected override IServiceCollection ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddNestedCache<HttpRequestCache, InMemoryCache>();
        services.AddJsonDependencies(configuration);

        return services;
    }
}
