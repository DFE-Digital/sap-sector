using SAPSec.Data.Dto.KS2.Performance;
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

        services.AddSingleton<IJsonFileFactory, JsonFileFactory>();
        services.AddJsonFile<EstablishmentPerformance>(JsonDataSource.PrimarySchools);
        services.AddJsonFile<LAPerformance>(JsonDataSource.PrimarySchools);
        services.AddJsonFile<EnglandPerformance>(JsonDataSource.PrimarySchools);
        services.AddSingleton<IKs2PerformanceRepository, JsonKs2PerformanceRepository>();

        // Formatters
        services.AddSingleton<ICharacteristicsComparisonFormatter, CharacteristicsComparisonFormatter>();
        services.AddSingleton<IPrimaryCharacteristicsComparisonFormatter, PrimaryCharacteristicsComparisonFormatter>();
    }
}
