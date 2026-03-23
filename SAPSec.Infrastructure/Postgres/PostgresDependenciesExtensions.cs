using Microsoft.Extensions.DependencyInjection;
using SAPSec.Core.Features.Attendance;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Data;

namespace SAPSec.Infrastructure.Postgres;

public static class PostgresDependenciesExtensions
{
    public static IServiceCollection AddPostgresqlDependencies(this IServiceCollection services)
    {

        services.AddSingleton<NpgsqlDataSourceFactory>();

        // Always default to Postgres in the app
        services.AddSingleton<IEstablishmentRepository, PostgresEstablishmentRepository>();
        services.AddSingleton<ISimilarSchoolsSecondaryRepository, PostgresSimilarSchoolsSecondaryRepository>();
        services.AddSingleton<IKs4PerformanceRepository, PostgresKs4PerformanceRepository>();
        services.AddSingleton<IAbsenceRepository, PostgresAbsenceRepository>();

        return services;
    }
}
