using SAPSec.Core.Caching;
using SAPSec.Data.Dto.RiseResources;
using SAPSec.Data.Repositories;
using SAPSec.Infrastructure.Json;
using SAPSec.Infrastructure.Redis;
using SAPSec.Web.Areas.AllThrough.Services;
using SAPSec.Web.Formatters;
using System.Diagnostics.CodeAnalysis;

namespace SAPSec.Web.Extensions;

[ExcludeFromCodeCoverage]
public static class DependenciesExtensions
{
    public static void AddDependencies(this IServiceCollection services)
    {
        services.AddScoped<IAllThroughSimilarSchoolsTabUrlBuilder, AllThroughSimilarSchoolsTabUrlBuilder>();

        // Formatters
        services.AddSingleton<ISecondaryCharacteristicsComparisonFormatter, SecondaryCharacteristicsComparisonFormatter>();
        services.AddSingleton<IPrimaryCharacteristicsComparisonFormatter, PrimaryCharacteristicsComparisonFormatter>();

        // RISE resources
        services.AddSingleton<IJsonFileFactory, JsonFileFactory>();
        services.AddJsonFile<RiseResourcesDocument>(JsonDataSource.RiseResources);
        services.AddSingleton<IRiseResourcesRepository, JsonRiseResourcesRepository>();

        // Caching
        services.AddSingleton<RequestCache>();
        services.AddSingleton<RedisCache>();
        services.AddSingleton<InMemoryCache>();

        services.AddSingleton<ICache>(sp => new NestedCache(
            sp.GetRequiredService<RequestCache>(),
            sp.GetRequiredService<RedisCache>()));
        services.AddHostedService<StartupCacheClearer>();
    }
}
