using Dapper;
using Microsoft.Extensions.Logging;
using SAPSec.Core.Features.Ks4HeadlineMeasures;
using SAPSec.Core.Model.Generated;

namespace SAPSec.Infrastructure.Postgres;

public class PostgresKs4DestinationsRepository(
    ILogger<PostgresKs4DestinationsRepository> logger,
    NpgsqlDataSourceFactory factory) : IKs4DestinationsRepository
{
    private readonly ILogger<PostgresKs4DestinationsRepository> _logger = logger;
    private readonly NpgsqlDataSourceFactory _factory = factory;

    public async Task<Ks4DestinationsData?> GetByUrnAsync(string urn)
    {
        using var conn = await _factory.Create().OpenConnectionAsync();

        const string sql = """
            SELECT *
            FROM public.v_establishment_destinations
            WHERE "Id" = @urn;

            SELECT d.*
            FROM public.v_la_destinations d
            INNER JOIN public.v_establishment e on e."LAId" = d."Id"
            WHERE e."URN" = @urn;
        
            SELECT *
            FROM public.v_england_destinations
            WHERE "Id" = 'National';
        """;

        using var results = await conn.QueryMultipleAsync(sql, new { urn });

        var establishmentDestinations = await results.ReadSingleOrDefaultAsync<EstablishmentDestinations>();
        var localAuthorityDestinations = await results.ReadSingleOrDefaultAsync<LADestinations>();
        var englandDestinations = await results.ReadSingleOrDefaultAsync<EnglandDestinations>();

        if (establishmentDestinations is null
            && localAuthorityDestinations is null
            && englandDestinations is null)
        {
            return null;
        }

        return new(
            establishmentDestinations,
            localAuthorityDestinations,
            englandDestinations);
    }
}
