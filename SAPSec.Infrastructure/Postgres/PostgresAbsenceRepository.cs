using Dapper;
using SAPSec.Data.Model.Generated;
using SAPSec.Data;
using SAPSec.Data.Model.Generated;

namespace SAPSec.Infrastructure.Postgres;

public class PostgresAbsenceRepository(NpgsqlDataSourceFactory factory) : IAbsenceRepository
{
    private readonly NpgsqlDataSourceFactory _factory = factory;

    public async Task<AbsenceData?> GetByUrnAsync(string urn, string? laId = null)
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
                la."Abs_Tot_LA_Current_Pct",
                la."Abs_Tot_LA_Previous_Pct",
                la."Abs_Tot_LA_Previous2_Pct",
                la."Abs_Persistent_LA_Current_Pct",
                la."Abs_Persistent_LA_Previous_Pct",
                la."Abs_Persistent_LA_Previous2_Pct"
            FROM public.v_la_absence la
            WHERE la."Id" = @laId
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

        using var results = await conn.QueryMultipleAsync(sql, new { urn, laId });

        var establishmentAbsence = await results.ReadSingleOrDefaultAsync<EstablishmentAbsence>();
        var laAbsence = await results.ReadSingleOrDefaultAsync<LAAbsence>();
        var englandAbsence = await results.ReadSingleOrDefaultAsync<EnglandAbsence>();

        if (establishmentAbsence is null && englandAbsence is null)
        {
            return null;
        }

        return new AbsenceData(
            establishmentAbsence,
            laAbsence,
            englandAbsence);
    }
}