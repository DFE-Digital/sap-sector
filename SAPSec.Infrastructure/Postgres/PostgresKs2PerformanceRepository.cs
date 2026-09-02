using Dapper;
using SAPSec.Data.Dto.KS2.Performance;
using SAPSec.Data.Repositories;

namespace SAPSec.Infrastructure.Postgres;

public class PostgresKs2PerformanceRepository(NpgsqlDataSourceFactory factory) : IKs2PerformanceRepository
{
    private readonly NpgsqlDataSourceFactory _factory = factory;

    public async Task<Ks2PerformanceData?> GetByUrnAsync(string urn)
    {
        var results = await GetByUrnsAsync([urn]);
        return results.FirstOrDefault(x => string.Equals(x.Urn, urn, StringComparison.Ordinal));
    }

    public async Task<IReadOnlyCollection<Ks2PerformanceData>> GetByUrnsAsync(IEnumerable<string> urns)
    {
        var requestedUrns = urns
            .Where(urn => !string.IsNullOrWhiteSpace(urn))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (requestedUrns.Length == 0)
        {
            return Array.Empty<Ks2PerformanceData>();
        }

        using var conn = await _factory.Create().OpenConnectionAsync();

        const string sql = """
            SELECT "URN", "LAId"
            FROM public.v_establishment
            WHERE "URN" = ANY(@urns);

            SELECT *
            FROM public.v_establishment_ks2_performance
            WHERE "Id" = ANY(@urns);

            SELECT *
            FROM public.v_la_ks2_performance
            WHERE "Id" IN (
                SELECT DISTINCT "LAId"
                FROM public.v_establishment
                WHERE "URN" = ANY(@urns)
            );

            SELECT *
            FROM public.v_england_ks2_performance
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

        var output = new List<Ks2PerformanceData>(requestedUrns.Length);

        foreach (var urn in requestedUrns)
        {
            if (!laIds.TryGetValue(urn, out var laId))
            {
                continue;
            }

            establishmentPerformance.TryGetValue(urn, out var schoolPerformance);
            localAuthorityPerformance.TryGetValue(laId, out var laPerformance);

            output.Add(new Ks2PerformanceData(
                urn,
                schoolPerformance,
                laPerformance,
                englandPerformance));
        }

        return output;
    }
}
