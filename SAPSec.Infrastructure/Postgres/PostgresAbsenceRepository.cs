using Dapper;
using SAPSec.Core.Features.Attendance;
using SAPSec.Core.Model.Generated;

namespace SAPSec.Infrastructure.Postgres;

public class PostgresAbsenceRepository(NpgsqlDataSourceFactory factory) : IAbsenceRepository
{
    private readonly NpgsqlDataSourceFactory _factory = factory;

    public async Task<AbsenceData?> GetByUrnAsync(string urn)
    {
        using var conn = await _factory.Create().OpenConnectionAsync();

        const string sql = """
            SELECT
                "Abs_Tot_Est_Current_Pct",
                "Abs_Tot_Est_Previous_Pct",
                "Abs_Tot_Est_Previous2_Pct",
                "Abs_Persistent_Est_Current_Pct",
                "Abs_Persistent_Est_Previous_Pct",
                "Abs_Persistent_Est_Previous2_Pct"
            FROM public.v_establishment_absence
            WHERE "Id" = @urn
            LIMIT 1;

            SELECT
                "Abs_Tot_Eng_Current_Pct",
                "Abs_Tot_Eng_Previous_Pct",
                "Abs_Tot_Eng_Previous2_Pct",
                "Abs_Persistent_Eng_Current_Pct",
                "Abs_Persistent_Eng_Previous_Pct",
                "Abs_Persistent_Eng_Previous2_Pct"
            FROM public.v_england_absence
            WHERE "Id" = 'National'
            LIMIT 1;
        """;

        using var results = await conn.QueryMultipleAsync(sql, new { urn });

        var establishmentAbsence = await results.ReadSingleOrDefaultAsync<EstablishmentAbsence>();
        var englandAbsence = await results.ReadSingleOrDefaultAsync<EnglandAbsence>();

        if (establishmentAbsence is null && englandAbsence is null)
        {
            return null;
        }

        return new AbsenceData(
            establishmentAbsence,
            englandAbsence);
    }
}
