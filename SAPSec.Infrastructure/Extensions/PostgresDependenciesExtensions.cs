using Microsoft.Extensions.DependencyInjection;
using SAPSec.Core.Features.Ks4HeadlineMeasures;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Infrastructure.Factories;
using SAPSec.Infrastructure.Repositories.Postgres;
using SAPSec.Infrastructure.TypeHandlers;

namespace SAPSec.Infrastructure.Extensions;

public static class PostgresDependenciesExtensions
{
    public static IServiceCollection AddPostgresqlDependencies(this IServiceCollection services)
    {
        Dapper.SqlMapper.AddTypeHandler(typeof(double?), new NullableDoubleHandler());

        services.AddSingleton<NpgsqlDataSourceFactory>();

        // Always default to Postgres in the app
        services.AddSingleton<IEstablishmentRepository, PostgresEstablishmentRepository>();
        services.AddSingleton<ISimilarSchoolsSecondaryRepository, PostgresSimilarSchoolsSecondaryRepository>();
        services.AddSingleton<IKs4PerformanceRepository, PostgresKs4PerformanceRepository>();

        return services;
    }
}
