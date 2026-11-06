using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SAPSec.Core.Caching;
using SAPSec.Data.Repositories;

namespace SAPSec.Infrastructure.Postgres;

public static class PostgresDependenciesExtensions
{
    public static IServiceCollection AddPostgresqlDependencies(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddSingleton<NpgsqlDataSourceFactory>();

        // Always default to Postgres in the app
        services.AddRepository<IEstablishmentRepository, PostgresEstablishmentRepository, CachedEstablishmentRepository>(configuration);
        services.AddRepository<ISimilarSchoolsPrimaryRepository, PostgresSimilarSchoolsPrimaryRepository, CachedSimilarSchoolsPrimaryRepository>(configuration);
        services.AddRepository<ISimilarSchoolsSecondaryRepository, PostgresSimilarSchoolsSecondaryRepository, CachedSimilarSchoolsSecondaryRepository>(configuration);
        services.AddRepository<IKs4PerformanceRepository, PostgresKs4PerformanceRepository, CachedKs4PerformanceRepository>(configuration);
        services.AddRepository<IKs4DestinationsRepository, PostgresKs4DestinationsRepository, CachedKs4DestinationsRepository>(configuration);
        services.AddRepository<IKs2PerformanceRepository, PostgresKs2PerformanceRepository, CachedKs2PerformanceRepository>(configuration);
        services.AddRepository<IExclusionsRepository, PostgresExclusionsRepository, CachedExclusionsRepository>(configuration);

        return services;
    }
}
