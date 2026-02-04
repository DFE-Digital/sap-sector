using Microsoft.Extensions.Configuration;
using Npgsql;

namespace SAPSec.Infrastructure.Factories;

public sealed class NpgsqlDataSourceFactory
{
    private readonly IConfiguration _configuration;
    private NpgsqlDataSource? _dataSource;

    public NpgsqlDataSourceFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public NpgsqlDataSource Create()
    {
        if (_dataSource is not null) return _dataSource;

        var cs = _configuration.GetConnectionString("PostgresConnectionString");

        if (string.IsNullOrWhiteSpace(cs))
        {
            throw new InvalidOperationException(
                "Connection string 'ConnectionStrings:PostgresConnectionString' is not configured. " +
                "Set it via User Secrets (local dev) or environment variable " +
                "'ConnectionStrings__PostgresConnectionString' (CI / containers).");
        }

        _dataSource = NpgsqlDataSource.Create(cs);
        return _dataSource;
    }

    
}