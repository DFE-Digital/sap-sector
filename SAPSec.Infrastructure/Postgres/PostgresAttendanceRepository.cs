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
                ROUND(AVG(NULLIF(la."Abs_Tot_Est_Current_Pct", '')::numeric), 2) AS "Abs_Tot_La_Current_Pct",
                ROUND(AVG(NULLIF(la."Abs_Tot_Est_Previous_Pct", '')::numeric), 2) AS "Abs_Tot_La_Previous_Pct",
                ROUND(AVG(NULLIF(la."Abs_Tot_Est_Previous2_Pct", '')::numeric), 2) AS "Abs_Tot_La_Previous2_Pct",
                ROUND(AVG(NULLIF(la."Abs_Persistent_Est_Current_Pct", '')::numeric), 2) AS "Abs_Persistent_La_Current_Pct",
                ROUND(AVG(NULLIF(la."Abs_Persistent_Est_Previous_Pct", '')::numeric), 2) AS "Abs_Persistent_La_Previous_Pct",
                ROUND(AVG(NULLIF(la."Abs_Persistent_Est_Previous2_Pct", '')::numeric), 2) AS "Abs_Persistent_La_Previous2_Pct"
            FROM public.v_establishment_absence la
            INNER JOIN public.v_establishment selected_school ON selected_school."URN" = @urn
            INNER JOIN public.v_establishment la_school ON la_school."URN" = la."Id"
            WHERE la_school."LAId" = selected_school."LAId"
              AND CASE
                    WHEN la_school."PhaseOfEducationName" ILIKE '%primary%' THEN 'State-funded primary'
                    WHEN la_school."PhaseOfEducationName" ILIKE '%secondary%' THEN 'State-funded secondary'
                    WHEN la_school."PhaseOfEducationName" ILIKE '%special%' THEN 'Special'
                    ELSE NULL
                  END = CASE
                    WHEN selected_school."PhaseOfEducationName" ILIKE '%primary%' THEN 'State-funded primary'
                    WHEN selected_school."PhaseOfEducationName" ILIKE '%secondary%' THEN 'State-funded secondary'
                    WHEN selected_school."PhaseOfEducationName" ILIKE '%special%' THEN 'Special'
                    ELSE NULL
                  END;

            SELECT
                MAX(CASE WHEN t."time_period" = '202324' THEN NULLIF(t."sess_overall_percent", '')::numeric END) AS "Abs_Tot_Eng_Current_Pct",
                MAX(CASE WHEN t."time_period" = '202223' THEN NULLIF(t."sess_overall_percent", '')::numeric END) AS "Abs_Tot_Eng_Previous_Pct",
                MAX(CASE WHEN t."time_period" = '202122' THEN NULLIF(t."sess_overall_percent", '')::numeric END) AS "Abs_Tot_Eng_Previous2_Pct",
                MAX(CASE WHEN t."time_period" = '202324' THEN NULLIF(t."enrolments_pa_10_exact_percent", '')::numeric END) AS "Abs_Persistent_Eng_Current_Pct",
                MAX(CASE WHEN t."time_period" = '202223' THEN NULLIF(t."enrolments_pa_10_exact_percent", '')::numeric END) AS "Abs_Persistent_Eng_Previous_Pct",
                MAX(CASE WHEN t."time_period" = '202122' THEN NULLIF(t."enrolments_pa_10_exact_percent", '')::numeric END) AS "Abs_Persistent_Eng_Previous2_Pct"
            FROM public.t_1_absence_3term_nat__2642eb995e t
            INNER JOIN public.v_establishment e ON e."URN" = @urn
            WHERE t."geographic_level" = 'National'
              AND t."education_phase" = CASE
                  WHEN e."PhaseOfEducationName" ILIKE '%primary%' THEN 'State-funded primary'
                  WHEN e."PhaseOfEducationName" ILIKE '%secondary%' THEN 'State-funded secondary'
                  WHEN e."PhaseOfEducationName" ILIKE '%special%' THEN 'Special'
                  ELSE NULL
              END;
        """;

        using var results = await conn.QueryMultipleAsync(sql, new { urn });

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
