using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Infrastructure.Factories;
using SAPSec.Infrastructure.Repositories;

namespace SAPSec.Infrastructure.Extensions;

public static class PostgresDependenciesExtensions
{
    public static IServiceCollection AddPostgresqlDependencies(this IServiceCollection services)
    {
        services.AddSingleton<NpgsqlDataSourceFactory>();

        // Always default to Postgres in the app
        services.AddSingleton<IEstablishmentRepository, PostgresEstablishmentRepository>();

        return services;
    }
}