using Microsoft.Extensions.DependencyInjection;
using SAPSec.Core.Features.Ks4HeadlineMeasures;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Interfaces.Repositories;
<<<<<<< HEAD:SAPSec.Infrastructure/Postgres/PostgresDependenciesExtensions.cs
=======
using SAPSec.Infrastructure.Factories;
using SAPSec.Infrastructure.Repositories.Postgres;
using SAPSec.Infrastructure.TypeHandlers;
>>>>>>> 8ccb9fc (add bar and charts):SAPSec.Infrastructure/Extensions/PostgresDependenciesExtensions.cs

namespace SAPSec.Infrastructure.Postgres;

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
