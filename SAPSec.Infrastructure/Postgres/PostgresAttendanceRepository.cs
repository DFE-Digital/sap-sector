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
                "Abs_Tot_Eng_Current_Pct",
                "Abs_Tot_Eng_Previous_Pct",
                "Abs_Tot_Eng_Previous2_Pct",
                "Abs_Persistent_Eng_Current_Pct",
                "Abs_Persistent_Eng_Previous_Pct",
                "Abs_Persistent_Eng_Previous2_Pct"
            FROM public.v_england_absence
            LIMIT 1;
        """;

        using var results = await conn.QueryMultipleAsync(sql, new { urn });

        var establishmentAttendance = await results.ReadSingleOrDefaultAsync<EstablishmentAttendance>();
        var englandRow = await results.ReadSingleOrDefaultAsync<EnglandAttendanceRow>();

        if (establishmentAttendance is null && englandRow is null)
        {
            return null;
        }

        return new AttendanceMeasuresData(
            establishmentAttendance,
            englandRow?.ToEnglandAttendance(),
            Array.Empty<string>());
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
