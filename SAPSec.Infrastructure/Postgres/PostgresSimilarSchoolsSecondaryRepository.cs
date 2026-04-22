using Dapper;
using Microsoft.Extensions.Logging;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Model.Generated;

namespace SAPSec.Infrastructure.Postgres;

public class PostgresSimilarSchoolsSecondaryRepository : ISimilarSchoolsSecondaryRepository
{
    private readonly ILogger<PostgresSimilarSchoolsSecondaryRepository> _logger;
    private readonly NpgsqlDataSourceFactory _factory;

    public PostgresSimilarSchoolsSecondaryRepository(ILogger<PostgresSimilarSchoolsSecondaryRepository> logger, NpgsqlDataSourceFactory factory)
    {
        _logger = logger;
        _factory = factory;
    }

    public async Task<IReadOnlyCollection<SimilarSchoolsSecondaryGroupsEntry>> GetSimilarSchoolsGroupAsync(string urn)
    {
        using var conn = await _factory.Create().OpenConnectionAsync();

        const string sql = """
            SELECT *
            FROM public.v_similar_schools_secondary_groups 
            WHERE "URN" = @urn
        """;

        var results = await conn.QueryAsync<SimilarSchoolsSecondaryGroupsEntry>(sql, new { urn });

        return results
            .ToList()
            .AsReadOnly();
    }

    public async Task<IReadOnlyCollection<SimilarSchoolsSecondaryValuesEntry>> GetSecondaryValuesByUrnsAsync(IEnumerable<string> urns)
    {
        if (!urns.Any())
        {
            return [];
        }

        const string sql = """
            SELECT *
            FROM public.v_similar_schools_secondary_values
            WHERE "URN" = ANY(@urns);
        """;

        using var conn = await _factory.Create().OpenConnectionAsync();

        var results = await conn.QueryAsync<SimilarSchoolsSecondaryValuesEntry>(sql, new { urns = urns.ToArray() });

        return results
            .ToList()
            .AsReadOnly();
    }

    public async Task<SimilarSchoolsSecondaryStandardDeviationsEntry?> GetSimilarSchoolsSecondaryStandardDeviationsAsync()
    {
        const string sql = """
            SELECT
                "KS2AVG",
                "PPPerc",
                "PercentEAL",
                "Polar4QuintilePupils",
                "PStability",
                "IdaciPupils",
                "PercentSchSupport",
                "NumberOfPupils",
                "PercentageStatementOrEHP"
            FROM public.v_similar_schools_secondary_values_national_sd;
        """;

        using var conn = await _factory.Create().OpenConnectionAsync();
        var result = await conn.QuerySingleOrDefaultAsync<SimilarSchoolsSecondaryStandardDeviationsEntry>(sql);

        return result;
    }

    public async Task<IReadOnlyCollection<string>> GetAllUrnsInSimilarSchoolsDataSet()
    {
        const string sql = """
            SELECT
                DISTINCT "URN" 
            FROM v_similar_schools_secondary_values;
        """;

        using var conn = await _factory.Create().OpenConnectionAsync();
        var result = await conn.QueryAsync<string>(sql);

        return result.ToList();
    }
}
