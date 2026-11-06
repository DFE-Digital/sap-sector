using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Globalization;

namespace SAPSec.Infrastructure.Postgres;

public sealed class NpgsqlDataSourceFactory
{
    private readonly Lazy<NpgsqlDataSource> _dataSource;

    public NpgsqlDataSourceFactory(IConfiguration configuration)
    {
        _dataSource = new Lazy<NpgsqlDataSource>(() =>
        {
            var connectionString =
                configuration.GetConnectionString("PostgresConnectionString");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'ConnectionStrings:PostgresConnectionString' is not configured. " +
                    "Set it via User Secrets (local dev) or environment variable " +
                    "'ConnectionStrings__PostgresConnectionString' (CI / containers).");
            }

            var settings = new NpgsqlConnectionStringBuilder(connectionString)
            {
                MaxPoolSize = configuration["Database:MaxPoolSize"] is { } poolSize
                    ? int.Parse(poolSize, CultureInfo.InvariantCulture)
                    : 100
            };

            if (configuration["Database:ConnectionTimeoutSeconds"] is { } timeout)
            {
                settings.Timeout = int.Parse(timeout, CultureInfo.InvariantCulture);
            }

            return NpgsqlDataSource.Create(settings.ConnectionString);

        }, isThreadSafe: true);
    }

    public NpgsqlDataSource Create() => _dataSource.Value;
}
