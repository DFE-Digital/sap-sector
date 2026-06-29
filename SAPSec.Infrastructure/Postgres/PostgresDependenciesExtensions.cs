using Microsoft.Extensions.DependencyInjection;
using SAPSec.Data.Store;

namespace SAPSec.Infrastructure.Postgres;

public static class PostgresDependenciesExtensions
{
    public static IServiceCollection AddPostgresqlDependencies(this IServiceCollection services)
    {

        services.AddSingleton<NpgsqlDataSourceFactory>();

        // Always default to Postgres in the app
        services.AddSingleton<IEstablishmentStore, PostgresEstablishmentStore>();
        services.AddSingleton<ISimilarSchoolsSecondaryStore, PostgresSimilarSchoolsSecondaryStore>();
        services.AddSingleton<ISimilarSchoolsPrimaryStore, PostgresSimilarSchoolsPrimaryStore>();
        services.AddSingleton<IKs4PerformanceStore, PostgresKs4PerformanceStore>();
        services.AddSingleton<IKs4DestinationsStore, PostgresKs4DestinationsStore>();
        services.AddSingleton<IAbsenceStore, PostgresAbsenceStore>();

        return services;
    }
}
