using Dapper;
using Microsoft.Extensions.Logging;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Model.Generated;

namespace SAPSec.Infrastructure.Postgres;

public class PostgresEstablishmentEmailRepository : IEstablishmentEmailRepository
{
    private readonly ILogger<PostgresEstablishmentEmailRepository> _logger;
    private readonly NpgsqlDataSourceFactory _factory;

    public PostgresEstablishmentEmailRepository(ILogger<PostgresEstablishmentEmailRepository> logger, NpgsqlDataSourceFactory factory)
    {
        _logger = logger;
        _factory = factory;
    }

    public async Task<EstablishmentEmail?> GetEstablishmentEmailAsync(string urn)
    {
        if (string.IsNullOrWhiteSpace(urn))
            return null;

        using var conn = await _factory.Create().OpenConnectionAsync();

        const string sql = """
            SELECT *
            FROM public.v_establishment_email
            WHERE "URN" = @urn
            LIMIT 1;
        """;

        var result = await conn.QuerySingleOrDefaultAsync<EstablishmentEmail>(sql, new { urn });

        return result;
    }
}
