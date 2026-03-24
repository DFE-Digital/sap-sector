using Dapper;
using SAPSec.Core.Features.Attendance;

namespace SAPSec.Infrastructure.Postgres;

public class PostgresAttendanceRepository(NpgsqlDataSourceFactory factory) : IAttendanceRepository
{
    private readonly NpgsqlDataSourceFactory _factory = factory;

    public async Task<AttendanceMeasuresData?> GetByUrnAsync(string urn, string? laId = null)
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

        var establishmentAttendance = await results.ReadSingleOrDefaultAsync<EstablishmentAttendance>();
        var localAuthorityAttendance = await results.ReadSingleOrDefaultAsync<LocalAuthorityAttendance>();
        var englandRow = await results.ReadSingleOrDefaultAsync<EnglandAttendanceRow>();

        if (establishmentAttendance is null && localAuthorityAttendance is null && englandRow is null)
        {
            return null;
        }

        return new AttendanceMeasuresData(
            establishmentAttendance,
            localAuthorityAttendance,
            englandRow?.ToEnglandAttendance());
    }

    private sealed class EnglandAttendanceRow
    {
        public decimal? Abs_Tot_Eng_Current_Pct { get; init; }
        public decimal? Abs_Tot_Eng_Previous_Pct { get; init; }
        public decimal? Abs_Tot_Eng_Previous2_Pct { get; init; }
        public decimal? Abs_Persistent_Eng_Current_Pct { get; init; }
        public decimal? Abs_Persistent_Eng_Previous_Pct { get; init; }
        public decimal? Abs_Persistent_Eng_Previous2_Pct { get; init; }

        public EnglandAttendance ToEnglandAttendance() => new()
        {
            Abs_Tot_Eng_Current_Pct = Abs_Tot_Eng_Current_Pct,
            Abs_Tot_Eng_Previous_Pct = Abs_Tot_Eng_Previous_Pct,
            Abs_Tot_Eng_Previous2_Pct = Abs_Tot_Eng_Previous2_Pct,
            Abs_Persistent_Eng_Current_Pct = Abs_Persistent_Eng_Current_Pct,
            Abs_Persistent_Eng_Previous_Pct = Abs_Persistent_Eng_Previous_Pct,
            Abs_Persistent_Eng_Previous2_Pct = Abs_Persistent_Eng_Previous2_Pct
        };
    }
}
