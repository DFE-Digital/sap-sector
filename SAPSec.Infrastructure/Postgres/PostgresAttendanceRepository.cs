using Dapper;
using SAPSec.Core.Features.Attendance;

namespace SAPSec.Infrastructure.Postgres;

public class PostgresAttendanceRepository(NpgsqlDataSourceFactory factory) : IAttendanceRepository
{
    private readonly NpgsqlDataSourceFactory _factory = factory;

    public async Task<AttendanceMeasuresData?> GetByUrnAsync(string urn)
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
                AVG(CASE WHEN "Abs_Tot_Est_Current_Pct" ~ '^-?[0-9]+(\\.[0-9]+)?$' THEN "Abs_Tot_Est_Current_Pct"::numeric END) AS "Abs_Tot_Eng_Current_Pct",
                AVG(CASE WHEN "Abs_Tot_Est_Previous_Pct" ~ '^-?[0-9]+(\\.[0-9]+)?$' THEN "Abs_Tot_Est_Previous_Pct"::numeric END) AS "Abs_Tot_Eng_Previous_Pct",
                AVG(CASE WHEN "Abs_Tot_Est_Previous2_Pct" ~ '^-?[0-9]+(\\.[0-9]+)?$' THEN "Abs_Tot_Est_Previous2_Pct"::numeric END) AS "Abs_Tot_Eng_Previous2_Pct",
                AVG(CASE WHEN "Abs_Persistent_Est_Current_Pct" ~ '^-?[0-9]+(\\.[0-9]+)?$' THEN "Abs_Persistent_Est_Current_Pct"::numeric END) AS "Abs_Persistent_Eng_Current_Pct",
                AVG(CASE WHEN "Abs_Persistent_Est_Previous_Pct" ~ '^-?[0-9]+(\\.[0-9]+)?$' THEN "Abs_Persistent_Est_Previous_Pct"::numeric END) AS "Abs_Persistent_Eng_Previous_Pct",
                AVG(CASE WHEN "Abs_Persistent_Est_Previous2_Pct" ~ '^-?[0-9]+(\\.[0-9]+)?$' THEN "Abs_Persistent_Est_Previous2_Pct"::numeric END) AS "Abs_Persistent_Eng_Previous2_Pct"
            FROM public.v_establishment_absence;
        """;

        using var results = await conn.QueryMultipleAsync(sql, new { urn });

        var establishmentAttendance = await results.ReadSingleOrDefaultAsync<EstablishmentAttendance>();
        var englandAttendance = await results.ReadSingleOrDefaultAsync<EnglandAttendance>();

        if (establishmentAttendance is null && englandAttendance is null)
        {
            return null;
        }

        return new AttendanceMeasuresData(
            establishmentAttendance,
            englandAttendance);
    }
}
