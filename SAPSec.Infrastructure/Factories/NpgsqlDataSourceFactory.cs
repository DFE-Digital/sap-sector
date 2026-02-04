using Microsoft.Extensions.Configuration;
using Npgsql;

namespace SAPSec.Infrastructure.Factories;

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

            return NpgsqlDataSource.Create(connectionString);

        }, isThreadSafe: true);
    }

    public NpgsqlDataSource Create() => _dataSource.Value;
}