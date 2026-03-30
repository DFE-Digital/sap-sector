using Dapper;
using Microsoft.Extensions.Logging;
using SAPSec.Core.Features.Ks4HeadlineMeasures;
using SAPSec.Core.Model.Generated;

namespace SAPSec.Infrastructure.Postgres;

public class PostgresKs4PerformanceRepository(
    ILogger<PostgresKs4PerformanceRepository> logger,
    NpgsqlDataSourceFactory factory) : IKs4PerformanceRepository
{
    private readonly ILogger<PostgresKs4PerformanceRepository> _logger = logger;
    private readonly NpgsqlDataSourceFactory _factory = factory;

    public async Task<Ks4HeadlineMeasuresData?> GetByUrnAsync(string urn)
    {
        var results = await GetByUrnsAsync([urn]);
        return results.GetValueOrDefault(urn);
    }

    public async Task<IReadOnlyDictionary<string, Ks4HeadlineMeasuresData?>> GetByUrnsAsync(IEnumerable<string> urns)
    {
        var requestedUrns = urns
            .Where(urn => !string.IsNullOrWhiteSpace(urn))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (requestedUrns.Length == 0)
        {
            return new Dictionary<string, Ks4HeadlineMeasuresData?>(StringComparer.Ordinal);
        }

        using var conn = await _factory.Create().OpenConnectionAsync();

        const string sql = """
            SELECT "URN", "LAId"
            FROM public.v_establishment
            WHERE "URN" = ANY(@urns);

            SELECT *
            FROM public.v_establishment_performance
            WHERE "Id" = ANY(@urns);

            SELECT *
            FROM public.v_establishment_destinations
            WHERE "Id" = ANY(@urns);

            SELECT *
            FROM public.v_england_performance
            WHERE "Id" = 'National'
            LIMIT 1;

            SELECT *
            FROM public.v_england_destinations
            WHERE "Id" = 'National'
            LIMIT 1;
        """;

        using var results = await conn.QueryMultipleAsync(sql, new { urns = requestedUrns });

        var establishments = (await results.ReadAsync<EstablishmentInfo>())
            .Where(x => !string.IsNullOrWhiteSpace(x.URN))
            .ToDictionary(x => x.URN!, StringComparer.Ordinal);
        var establishmentPerformance = (await results.ReadAsync<EstablishmentPerformance>())
            .ToDictionary(x => x.Id, StringComparer.Ordinal);
        var establishmentDestinations = (await results.ReadAsync<EstablishmentDestinations>())
            .ToDictionary(x => x.Id, StringComparer.Ordinal);
        var englandPerformance = await results.ReadSingleOrDefaultAsync<EnglandPerformance>();
        var englandDestinations = await results.ReadSingleOrDefaultAsync<EnglandDestinations>();

        var laIds = establishments.Values
            .Select(x => x.LAId)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var localAuthorityPerformance = new Dictionary<string, LAPerformance>(StringComparer.Ordinal);
        var localAuthorityDestinations = new Dictionary<string, LADestinations>(StringComparer.Ordinal);

        if (laIds.Length > 0)
        {
            const string laSql = """
                SELECT *
                FROM public.v_la_performance
                WHERE "Id" = ANY(@laIds);

                SELECT *
                FROM public.v_la_destinations
                WHERE "Id" = ANY(@laIds);
            """;

            using var laResults = await conn.QueryMultipleAsync(laSql, new { laIds });
            localAuthorityPerformance = (await laResults.ReadAsync<LAPerformance>())
                .ToDictionary(x => x.Id, StringComparer.Ordinal);
            localAuthorityDestinations = (await laResults.ReadAsync<LADestinations>())
                .ToDictionary(x => x.Id, StringComparer.Ordinal);
        }
        else
        {
            _logger.LogWarning("No LAIds found when loading KS4 performance for URNs {Urns}.", requestedUrns);
        }

        var output = new Dictionary<string, Ks4HeadlineMeasuresData?>(StringComparer.Ordinal);

        foreach (var urn in requestedUrns)
        {
            if (!establishments.TryGetValue(urn, out var establishment))
            {
                continue;
            }

            establishmentPerformance.TryGetValue(urn, out var schoolPerformance);
            establishmentDestinations.TryGetValue(urn, out var schoolDestinations);
            localAuthorityPerformance.TryGetValue(establishment.LAId ?? string.Empty, out var laPerformance);
            localAuthorityDestinations.TryGetValue(establishment.LAId ?? string.Empty, out var laDestinations);

            output[urn] = new Ks4HeadlineMeasuresData(
                schoolPerformance,
                laPerformance,
                englandPerformance,
                schoolDestinations,
                laDestinations,
                englandDestinations);
        }

        return output;
    }

    private sealed class EstablishmentInfo
    {
        public string? URN { get; set; }
        public string? LAId { get; set; }
    }
}
