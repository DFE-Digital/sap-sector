using Dapper;
using Microsoft.Extensions.Logging;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Model;
using SAPSec.Infrastructure.Factories;

namespace SAPSec.Infrastructure.Repositories.Postgres;

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
                   "Street" AS "AddressStreet",
                   "Postcode" AS "AddressPostcode"
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

    public async Task<Establishment> GetEstablishmentAsync(string urn)
    {
        if (string.IsNullOrWhiteSpace(urn))
            return new Establishment();

        using var conn = await _factory.Create().OpenConnectionAsync();

        const string sql = """
            SELECT *,
                   "Street" AS "AddressStreet",
                   "Locality" AS "AddressLocality",
                   "Address3" AS "AddressAddress3",
                   "Town" AS "AddressTown",
                   "Postcode" AS "AddressPostcode"
            FROM public.v_establishment
            WHERE "URN" = @urn
            LIMIT 1;
        """;

        var result = await conn.QuerySingleOrDefaultAsync<Establishment>(sql, new { urn });
        return result ?? new Establishment();
    }

    public async Task<Establishment> GetEstablishmentByAnyNumberAsync(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
            return new Establishment();

        using var conn = await _factory.Create().OpenConnectionAsync();

        int? ukprn = int.TryParse(number, out var k) ? k : null;

        //Missing Field "DfENumberSearchable" in database
        const string sql = """
            SELECT *,
                   "Street" AS "AddressStreet",
                   "Locality" AS "AddressLocality",
                   "Address3" AS "AddressAddress3",
                   "Town" AS "AddressTown",
                   "Postcode" AS "AddressPostcode"
            FROM public.v_establishment
            WHERE "URN" = @number
                OR (@ukprn IS NOT NULL AND "UKPRN" = @ukprn)
            LIMIT 1;
        """;

        var result = await conn.QuerySingleOrDefaultAsync<Establishment>(sql, new { number, ukprn });
        return result ?? new Establishment();
    }
}
