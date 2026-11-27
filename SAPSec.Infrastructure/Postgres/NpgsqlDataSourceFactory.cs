using Microsoft.Extensions.Configuration;
using Npgsql;

namespace SAPSec.Infrastructure.Postgres;

public sealed class NpgsqlDataSourceFactory
{
    private readonly Lazy<NpgsqlDataSource> _dataSource;

    public NpgsqlDataSourceFactory(IConfiguration configuration)
    {
        var settings = configuration.GetSection(PostgresSettings.SectionName).Get<PostgresSettings>();
        _dataSource = new Lazy<NpgsqlDataSource>(() =>
        {
            var connectionString = !string.IsNullOrWhiteSpace(settings?.ConnectionString)
                ? settings.ConnectionString
                : configuration.GetConnectionString("PostgresConnectionString");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'Postgres:ConnectionString' is not configured. " +
                    "Set it via User Secrets (local dev) or environment variable " +
                    "'Postgres__ConnectionString' (CI / containers).");
            }

            var npgsqlSettings = new NpgsqlConnectionStringBuilder(connectionString)
            {
                MaxPoolSize = settings?.MaxPoolSize ?? 100,
            };

            if (settings?.ConnectionTimeoutSeconds is int timeout)
            {
                npgsqlSettings.Timeout = timeout;
            }

            return NpgsqlDataSource.Create(npgsqlSettings);

        }, isThreadSafe: true);
    }

    public NpgsqlDataSource Create() => _dataSource.Value;
}
