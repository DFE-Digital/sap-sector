using Dapper;
using Microsoft.Extensions.Logging;
using SAPSec.Data.Dto.SimilarSchools.Primary;
using SAPSec.Data.Repositories;

namespace SAPSec.Infrastructure.Postgres;

public class PostgresSimilarSchoolsPrimaryRepository : ISimilarSchoolsPrimaryRepository
{
    private readonly ILogger<PostgresSimilarSchoolsPrimaryRepository> _logger;
    private readonly NpgsqlDataSourceFactory _factory;

    public PostgresSimilarSchoolsPrimaryRepository(
        ILogger<PostgresSimilarSchoolsPrimaryRepository> logger,
        NpgsqlDataSourceFactory factory)
    {
        _logger = logger;
        _factory = factory;
    }

    public async Task<IReadOnlyCollection<SimilarSchoolsPrimaryGroupsEntry>> GetGroupAsync(string urn)
    {
        const string sql = """
            SELECT *
            FROM public.v_similar_schools_primary_groups
            WHERE "URN" = @urn
        """;

        using var conn = await _factory.Create().OpenConnectionAsync();
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
            SELECT DISTINCT "URN"
            FROM public.v_similar_schools_primary_values;
        """;

        using var conn = await _factory.Create().OpenConnectionAsync();
        var result = await conn.QueryAsync<string>(sql);

        return result.ToList();
    }
}
