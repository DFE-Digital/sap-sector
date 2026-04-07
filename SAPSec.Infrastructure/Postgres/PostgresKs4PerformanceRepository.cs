using Dapper;
using Microsoft.Extensions.Logging;
using SAPSec.Data;
using SAPSec.Data.Model.Generated;

namespace SAPSec.Infrastructure.Postgres;

public class PostgresKs4PerformanceRepository(
    ILogger<PostgresKs4PerformanceRepository> logger,
    NpgsqlDataSourceFactory factory) : IKs4PerformanceRepository
{
    private readonly ILogger<PostgresKs4PerformanceRepository> _logger = logger;
    private readonly NpgsqlDataSourceFactory _factory = factory;

    public async Task<Ks4PerformanceData?> GetByUrnAsync(string urn)
    {
        var results = await GetByUrnsAsync([urn]);
        return results.FirstOrDefault(x => string.Equals(x.Urn, urn, StringComparison.Ordinal));
    }

    public async Task<IReadOnlyCollection<Ks4PerformanceData>> GetByUrnsAsync(IEnumerable<string> urns)
    {
        var requestedUrns = urns
            .Where(urn => !string.IsNullOrWhiteSpace(urn))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (requestedUrns.Length == 0)
        {
            return Array.Empty<Ks4PerformanceData>();
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
            FROM public.v_la_performance
            WHERE "Id" IN (
                SELECT DISTINCT "LAId" 
                FROM public.v_establishment 
                WHERE "URN" = ANY(@urns)
            );
            
            SELECT *
            FROM public.v_england_performance
            WHERE "Id" = 'National';
        """;

        using var results = await conn.QueryMultipleAsync(sql, new { urns = requestedUrns });

        var laIds = (await results.ReadAsync<(string, string)>())
            .ToDictionary(x => x.Item1, x => x.Item2, StringComparer.Ordinal);

        var establishmentPerformance = (await results.ReadAsync<EstablishmentPerformance>())
            .ToDictionary(x => x.Id, StringComparer.Ordinal);

        var localAuthorityPerformance = (await results.ReadAsync<LAPerformance>())
            .ToDictionary(x => x.Id, StringComparer.Ordinal);

        var englandPerformance = await results.ReadSingleOrDefaultAsync<EnglandPerformance>();

        var output = new List<Ks4PerformanceData>(requestedUrns.Length);

        foreach (var urn in requestedUrns)
        {
            if (!laIds.TryGetValue(urn, out var laId))
            {
                continue;
            }

            establishmentPerformance.TryGetValue(urn, out var schoolPerformance);
            localAuthorityPerformance.TryGetValue(laId, out var laPerformance);

            output.Add(new Ks4PerformanceData(
                urn,
                schoolPerformance,
                laPerformance,
                englandPerformance));
        }

        return output;
    }
}
