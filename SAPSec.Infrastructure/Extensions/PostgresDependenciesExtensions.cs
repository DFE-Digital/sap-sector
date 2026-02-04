using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace SAPSec.Infrastructure.Extensions;

public static class PostgresDependenciesExtensions
{
    public static IServiceCollection AddPostgresqlDependencies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgresConnectionString");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'ConnectionStrings:PostgresConnectionString' is not configured. " +
                "Set it via User Secrets (local dev) or environment variable " +
                "'ConnectionStrings__PostgresConnectionString' (CI / containers).");
        }

        services.AddSingleton<NpgsqlDataSource>(_ =>
            NpgsqlDataSource.Create(connectionString));

        return services;
    }
}