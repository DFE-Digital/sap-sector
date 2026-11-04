using J2N.Collections.Generic.Extensions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SAPSec.Core.Caching;
using SAPSec.Data.Dto.RiseResources;
using SAPSec.Data.Repositories;
using SAPSec.Infrastructure.Json;
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
    }

    public static void AddCaching(this IServiceCollection services, IConfiguration configuration, Action<IServiceCollection, IConfiguration> configure)
    {
        var config = configuration.GetSection(nameof(CachingConfiguration)).Get<CachingConfiguration>();

        if (config is not null && config.EnableCaching)
        {
            services.Configure<CachingConfiguration>(configuration.GetSection(nameof(CachingConfiguration)));
            services.AddSingleton<RequestCache>();
            services.AddSingleton<InMemoryCache>();
            configure(services, configuration);
        }
    }

    public static void AddNestedCache<TOuterCache, TInnerCache>(this IServiceCollection services)
        where TOuterCache : class, ICache
        where TInnerCache : class, ICache
    {
        services.RemoveAll<ICache>();
        services.AddSingleton<ICache, NestedCache<TOuterCache, TInnerCache>>();
    }
}
