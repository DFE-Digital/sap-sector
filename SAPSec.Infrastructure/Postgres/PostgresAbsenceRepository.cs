using Dapper;
using SAPSec.Core.Features.Attendance;
using SAPSec.Core.Model.Generated;

namespace SAPSec.Infrastructure.Postgres;

public class PostgresAbsenceRepository(NpgsqlDataSourceFactory factory) : IAbsenceRepository
{
    private readonly NpgsqlDataSourceFactory _factory = factory;

    public async Task<AbsenceData?> GetByUrnAsync(string urn)
    {
        var results = await GetByUrnsAsync([urn]);
        return results.FirstOrDefault(x => string.Equals(x.URN, urn, StringComparison.Ordinal));
    }

    public async Task<IReadOnlyCollection<AbsenceData>> GetByUrnsAsync(IEnumerable<string> urns)
    {
        var requestedUrns = urns
            .Where(urn => !string.IsNullOrWhiteSpace(urn))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (requestedUrns.Length == 0)
        {
            return Array.Empty<AbsenceData>();
        }

        using var conn = await _factory.Create().OpenConnectionAsync();

        const string sql = """
            SELECT "URN", "LAId"
            FROM public.v_establishment
            WHERE "URN" = ANY(@urns);
        
            SELECT *
            FROM public.v_establishment_absence
            WHERE "Id" = ANY(@urns);
        
            SELECT *
            FROM public.v_la_absence
            WHERE "Id" IN (
                SELECT DISTINCT "LAId" 
                FROM public.v_establishment 
                WHERE "URN" = ANY(@urns)
            );
            
            SELECT *
            FROM public.v_england_absence
            WHERE "Id" = 'National';
        """;

        using var results = await conn.QueryMultipleAsync(sql, new { urns = requestedUrns });

        var laIds = (await results.ReadAsync<(string, string)>())
            .ToDictionary(x => x.Item1, x => x.Item2, StringComparer.Ordinal);

        var establishmentAbsence = (await results.ReadAsync<EstablishmentAbsence>())
            .ToDictionary(x => x.Id, StringComparer.Ordinal);

        var localAuthorityAbsence = (await results.ReadAsync<LAAbsence>())
            .ToDictionary(x => x.Id, StringComparer.Ordinal);

        var englandAbsence = await results.ReadSingleOrDefaultAsync<EnglandAbsence>();

        var output = new List<AbsenceData>(requestedUrns.Length);

        foreach (var urn in requestedUrns)
        {
            if (!laIds.TryGetValue(urn, out var laId))
            {
                continue;
            }

            establishmentAbsence.TryGetValue(urn, out var school);
            localAuthorityAbsence.TryGetValue(laId, out var la);

            output.Add(new AbsenceData(
                urn,
                school,
                la,
                englandAbsence));
        }

        return output;
    }
}