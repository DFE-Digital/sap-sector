using SAPSec.Data.Dto.KS2.Performance;
using SAPSec.Data.Dto.RiseResources;
using SAPSec.Data.Repositories;
using SAPSec.Infrastructure.Json;
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

        // RISE resources
        services.AddJsonFile<RiseResourcesDocument>(JsonDataSource.RiseResources);
        services.AddSingleton<IRiseResourcesRepository, JsonRiseResourcesRepository>();

        // Formatters
        services.AddSingleton<ISecondaryCharacteristicsComparisonFormatter, SecondaryCharacteristicsComparisonFormatter>();
        services.AddSingleton<IPrimaryCharacteristicsComparisonFormatter, PrimaryCharacteristicsComparisonFormatter>();
    }
}
