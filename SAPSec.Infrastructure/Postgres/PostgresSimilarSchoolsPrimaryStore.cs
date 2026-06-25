using Dapper;
using Microsoft.Extensions.Logging;
using SAPSec.Data.Dto.SimilarSchools.Primary;
using SAPSec.Data.Store;

namespace SAPSec.Infrastructure.Postgres;

public class PostgresSimilarSchoolsPrimaryStore : ISimilarSchoolsPrimaryStore
{
    private readonly ILogger<PostgresSimilarSchoolsPrimaryStore> _logger;
    private readonly NpgsqlDataSourceFactory _factory;

    public PostgresSimilarSchoolsPrimaryStore(ILogger<PostgresSimilarSchoolsPrimaryStore> logger, NpgsqlDataSourceFactory factory)
    {
        _logger = logger;
        _factory = factory;
    }

    public async Task<IReadOnlyCollection<SimilarSchoolsPrimaryGroupsEntry>> GetGroupAsync(string urn)
    {
        using var conn = await _factory.Create().OpenConnectionAsync();

        const string sql = """
            SELECT *
            FROM public.v_similar_schools_primary_groups 
            WHERE "URN" = @urn
        """;

        var results = await conn.QueryAsync<SimilarSchoolsPrimaryGroupsEntry>(sql, new { urn });

        return results
            .ToList()
            .AsReadOnly();
    }

    public async Task<IReadOnlyCollection<SimilarSchoolsPrimaryValuesEntry>> GetValuesByUrnsAsync(IEnumerable<string> urns)
    {
        if (!urns.Any())
        {
            return [];
        }

        const string sql = """
            SELECT *
            FROM public.v_similar_schools_primary_values
            WHERE "URN" = ANY(@urns);
        """;

        using var conn = await _factory.Create().OpenConnectionAsync();

        var results = await conn.QueryAsync<SimilarSchoolsPrimaryValuesEntry>(sql, new { urns = urns.ToArray() });

        return results
            .ToList()
            .AsReadOnly();
    }

    public async Task<IReadOnlyCollection<string>> GetAllUrnsInSimilarSchoolsDataSet()
    {
        const string sql = """
            SELECT
                DISTINCT "URN" 
            FROM v_similar_schools_primary_values;
        """;

        using var conn = await _factory.Create().OpenConnectionAsync();
        var result = await conn.QueryAsync<string>(sql);

        return result.ToList();
    }
}
