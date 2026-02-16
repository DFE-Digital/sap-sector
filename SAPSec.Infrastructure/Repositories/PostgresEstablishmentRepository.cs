using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Model;
using SAPSec.Infrastructure.Factories;


namespace SAPSec.Infrastructure.Repositories;

public class PostgresEstablishmentRepository : IEstablishmentRepository
{
    private readonly ILogger<PostgresEstablishmentRepository> _logger;
    private readonly NpgsqlDataSourceFactory _factory;

    public PostgresEstablishmentRepository(ILogger<PostgresEstablishmentRepository> logger, NpgsqlDataSourceFactory factory)
    {
        _logger = logger;
        _factory = factory;
    }

    public IEnumerable<Establishment> GetAllEstablishments()
    {
        using var conn = _factory.Create().OpenConnection();

        const string sql = """
                               SELECT "URN",
                                      "EstablishmentName",
                                      "Street" AS "AddressStreet",
                                      "Postcode" AS "AddressPostcode"
                               FROM public.v_establishment;
                           """;

        foreach (var establishment in conn.Query<Establishment>(sql, buffered: false))
        {
            yield return establishment;
        }
    }

    public Establishment GetEstablishment(string urn)
    {
        if (string.IsNullOrWhiteSpace(urn))
            return new Establishment();

        using var conn = _factory.Create().OpenConnection();

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

        return conn.QuerySingleOrDefault<Establishment>(sql, new { urn }) ?? new Establishment();
    }

    public Establishment GetEstablishmentByAnyNumber(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
            return new Establishment();

        using var conn = _factory.Create().OpenConnection();

        int? ukprn = int.TryParse(number, out var k) ? k : (int?)null;

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

        return conn.QuerySingleOrDefault<Establishment>(sql, new { number, ukprn })
               ?? new Establishment();
        
    }
}
