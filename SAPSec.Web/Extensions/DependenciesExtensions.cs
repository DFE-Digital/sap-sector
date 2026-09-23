using SAPSec.Data.Dto.RiseResources;
using SAPSec.Data.Repositories;
using SAPSec.Infrastructure.Json;
using SAPSec.Web.Areas.AllThrough.Services;
using SAPSec.Web.Formatters;
using SAPSec.Web.Services;
using System.Diagnostics.CodeAnalysis;

namespace SAPSec.Web.Extensions;

[ExcludeFromCodeCoverage]
public static class DependenciesExtensions
{
    public static void AddDependencies(this IServiceCollection services)
    {
        services.AddScoped<IRequestSchoolAccessor, RequestSchoolAccessor>();
        services.AddScoped<IAllThroughSimilarSchoolsTabUrlBuilder, AllThroughSimilarSchoolsTabUrlBuilder>();
        
        // Formatters
        services.AddSingleton<ISecondaryCharacteristicsComparisonFormatter, SecondaryCharacteristicsComparisonFormatter>();
        services.AddSingleton<IPrimaryCharacteristicsComparisonFormatter, PrimaryCharacteristicsComparisonFormatter>();

        // RISE resources
        services.AddSingleton<IJsonFileFactory, JsonFileFactory>();
        services.AddJsonFile<RiseResourcesDocument>(JsonDataSource.RiseResources);
        services.AddSingleton<IRiseResourcesRepository, JsonRiseResourcesRepository>();
    }
}
