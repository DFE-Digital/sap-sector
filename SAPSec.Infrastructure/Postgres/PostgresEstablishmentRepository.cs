using Dapper;
using Microsoft.Extensions.Logging;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Model.Generated;

namespace SAPSec.Infrastructure.Postgres;

public class PostgresEstablishmentRepository : IEstablishmentRepository
{
    private readonly ILogger<PostgresEstablishmentRepository> _logger;
    private readonly NpgsqlDataSourceFactory _factory;

    public PostgresEstablishmentRepository(ILogger<PostgresEstablishmentRepository> logger, NpgsqlDataSourceFactory factory)
    {
        _logger = logger;
        _factory = factory;
    }

    public async Task<IReadOnlyCollection<Establishment>> GetAllEstablishmentsAsync()
    {
        using var conn = await _factory.Create().OpenConnectionAsync();

        const string sql = """
            SELECT "URN",
                   "EstablishmentName",
                   "Street",
                   "Postcode"
            FROM public.v_establishment;
        """;

        var result = await conn.QueryAsync<Establishment>(sql);
        return result.ToList();
    }

    public async Task<IReadOnlyCollection<Establishment>> GetEstablishmentsAsync(IEnumerable<string> urns)
    {
        using var conn = await _factory.Create().OpenConnectionAsync();

        const string sql = """
            SELECT *
            FROM public.v_establishment
            WHERE "URN" = ANY(@urns);
        """;

        var result = await conn.QueryAsync<Establishment>(sql, new { urns = urns.ToArray() });
        return result.ToList();
    }

    public async Task<Establishment?> GetEstablishmentAsync(string urn)
    {
        if (string.IsNullOrWhiteSpace(urn))
            return null;

        using var conn = await _factory.Create().OpenConnectionAsync();

        const string sql = """
            SELECT *
            FROM public.v_establishment
            WHERE "URN" = @urn
            LIMIT 1;
        """;

        var result = await conn.QuerySingleOrDefaultAsync<Establishment>(sql, new { urn });

        return result;
    }

    public async Task<Establishment?> GetEstablishmentByAnyNumberAsync(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
            return null;

        using var conn = await _factory.Create().OpenConnectionAsync();

        const string sql = """
            SELECT *
            FROM public.v_establishment
            WHERE "URN" = @number
                OR "UKPRN" = @number
                OR CONCAT("LAId", "EstablishmentNumber") = @number
            LIMIT 1;
        """;

        var result = await conn.QuerySingleOrDefaultAsync<Establishment>(sql, new { number });

        return result;
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
