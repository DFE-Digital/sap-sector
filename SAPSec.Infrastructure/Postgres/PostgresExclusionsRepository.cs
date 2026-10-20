using Dapper;
using SAPSec.Data.Dto.Exclusions;
using SAPSec.Data.Repositories;

namespace SAPSec.Infrastructure.Postgres;

public class PostgresExclusionsRepository(NpgsqlDataSourceFactory factory) : IExclusionsRepository
{
    private readonly NpgsqlDataSourceFactory _factory = factory;

    public async Task<ExclusionsData?> GetByUrnAsync(string urn)
    {
        var results = await GetByUrnsAsync([urn]);
        return results.FirstOrDefault(x => string.Equals(x.Urn, urn, StringComparison.Ordinal));
    }

    public async Task<IReadOnlyCollection<ExclusionsData>> GetByUrnsAsync(IEnumerable<string> urns)
    {
        var requestedUrns = urns
            .Where(urn => !string.IsNullOrWhiteSpace(urn))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (requestedUrns.Length == 0)
        {
            return Array.Empty<ExclusionsData>();
        }

        using var conn = await _factory.Create().OpenConnectionAsync();

        const string sql = """
            SELECT "URN"
            FROM public.v_establishment
            WHERE "URN" = ANY(@urns);

            SELECT *
            FROM public.v_establishment_exclusions
            WHERE "Id" = ANY(@urns);
        """;

        using var results = await conn.QueryMultipleAsync(sql, new { urns = requestedUrns });

        var establishmentUrns = (await results.ReadAsync<string>())
            .ToHashSet(StringComparer.Ordinal);

        var establishmentExclusions = (await results.ReadAsync<EstablishmentExclusions>())
            .ToDictionary(x => x.Id, StringComparer.Ordinal);

        return requestedUrns
            .Where(establishmentUrns.Contains)
            .Select(urn => new ExclusionsData(urn, establishmentExclusions.GetValueOrDefault(urn)))
            .ToList();
    }
}
