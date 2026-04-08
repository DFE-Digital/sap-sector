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
        var results = await GetByUrnsAsync([urn]);
        return results.FirstOrDefault(x => string.Equals(x.Urn, urn, StringComparison.Ordinal));
    }

    public async Task<IReadOnlyCollection<Ks4DestinationsData>> GetByUrnsAsync(IEnumerable<string> urns)
    {
        var requestedUrns = urns
            .Where(urn => !string.IsNullOrWhiteSpace(urn))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (requestedUrns.Length == 0)
        {
            return Array.Empty<Ks4DestinationsData>();
        }

        using var conn = await _factory.Create().OpenConnectionAsync();

        const string sql = """
            SELECT "URN", "LAId"
            FROM public.v_establishment
            WHERE "URN" = ANY(@urns);
        
            SELECT *
            FROM public.v_establishment_destinations
            WHERE "Id" = ANY(@urns);
        
            SELECT *
            FROM public.v_la_destinations
            WHERE "Id" IN (
                SELECT DISTINCT "LAId" 
                FROM public.v_establishment 
                WHERE "URN" = ANY(@urns)
            );
            
            SELECT *
            FROM public.v_england_destinations
            WHERE "Id" = 'National';
        """;

        using var results = await conn.QueryMultipleAsync(sql, new { urns = requestedUrns });

        var laIds = (await results.ReadAsync<(string, string)>())
            .ToDictionary(x => x.Item1, x => x.Item2, StringComparer.Ordinal);

        var establishmentDestinations = (await results.ReadAsync<EstablishmentDestinations>())
            .ToDictionary(x => x.Id, StringComparer.Ordinal);

        var localAuthorityDestinations = (await results.ReadAsync<LADestinations>())
            .ToDictionary(x => x.Id, StringComparer.Ordinal);

        var englandDestinations = await results.ReadSingleOrDefaultAsync<EnglandDestinations>();

        var output = new List<Ks4DestinationsData>(requestedUrns.Length);

        foreach (var urn in requestedUrns)
        {
            if (!laIds.TryGetValue(urn, out var laId))
            {
                continue;
            }

            establishmentDestinations.TryGetValue(urn, out var schoolPerformance);
            localAuthorityDestinations.TryGetValue(laId, out var laPerformance);

            output.Add(new Ks4DestinationsData(
                urn,
                schoolPerformance,
                laPerformance,
                englandDestinations));
        }

        return output;
    }
}
