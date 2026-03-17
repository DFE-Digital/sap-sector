using Dapper;
using Microsoft.Extensions.Logging;
using SAPSec.Core.Features.Ks4HeadlineMeasures;
using SAPSec.Core.Model.KS4.Destinations;
using SAPSec.Core.Model.KS4.Performance;
using System.Globalization;

namespace SAPSec.Infrastructure.Postgres;

public class PostgresKs4PerformanceRepository(
    ILogger<PostgresKs4PerformanceRepository> logger,
    NpgsqlDataSourceFactory factory) : IKs4PerformanceRepository
{
    private readonly ILogger<PostgresKs4PerformanceRepository> _logger = logger;
    private readonly NpgsqlDataSourceFactory _factory = factory;

    public async Task<Ks4HeadlineMeasuresData?> GetByUrnAsync(string urn)
    {
        using var conn = await _factory.Create().OpenConnectionAsync();

        const string sql = """
            SELECT "LAId"
            FROM public.v_establishment
            WHERE "URN" = @urn
            LIMIT 1;

            SELECT *
            FROM public.v_establishment_performance
            WHERE "Id" = @urn;

            SELECT *
            FROM public.v_england_performance
            WHERE "Id" = 'National'
            LIMIT 1;

            SELECT
                "Id",
                "AllDest_Tot_Est_Current_Pct"::text AS "AllDestCurrentPct",
                "AllDest_Tot_Est_Previous_Pct"::text AS "AllDestPreviousPct",
                "AllDest_Tot_Est_Previous2_Pct"::text AS "AllDestPrevious2Pct",
                "Education_Tot_Est_Current_Pct"::text AS "EducationCurrentPct",
                "Education_Tot_Est_Previous_Pct"::text AS "EducationPreviousPct",
                "Education_Tot_Est_Previous2_Pct"::text AS "EducationPrevious2Pct",
                "Employment_Tot_Est_Current_Pct"::text AS "EmploymentCurrentPct",
                "Employment_Tot_Est_Previous_Pct"::text AS "EmploymentPreviousPct",
                "Employment_Tot_Est_Previous2_Pct"::text AS "EmploymentPrevious2Pct"
            FROM public.v_establishment_destinations
            WHERE "Id" = @urn;

            SELECT
                "Id",
                "AllDest_Tot_Eng_Current_Pct"::text AS "AllDestCurrentPct",
                "AllDest_Tot_Eng_Previous_Pct"::text AS "AllDestPreviousPct",
                "AllDest_Tot_Eng_Previous2_Pct"::text AS "AllDestPrevious2Pct",
                "Education_Tot_Eng_Current_Pct"::text AS "EducationCurrentPct",
                "Education_Tot_Eng_Previous_Pct"::text AS "EducationPreviousPct",
                "Education_Tot_Eng_Previous2_Pct"::text AS "EducationPrevious2Pct",
                "Employment_Tot_Eng_Current_Pct"::text AS "EmploymentCurrentPct",
                "Employment_Tot_Eng_Previous_Pct"::text AS "EmploymentPreviousPct",
                "Employment_Tot_Eng_Previous2_Pct"::text AS "EmploymentPrevious2Pct"
            FROM public.v_england_destinations
            WHERE "Id" = 'National'
            LIMIT 1;
        """;

        using var results = await conn.QueryMultipleAsync(sql, new { urn });

        var establishmentInfo = await results.ReadSingleOrDefaultAsync<EstablishmentInfo>();
        var establishmentPerformance = await results.ReadSingleOrDefaultAsync<EstablishmentPerformance>();
        var englandPerformance = await results.ReadSingleOrDefaultAsync<EnglandPerformance>();
        var establishmentDestinationsDao = await results.ReadSingleOrDefaultAsync<DestinationTotalsDao>();
        var englandDestinationsDao = await results.ReadSingleOrDefaultAsync<DestinationTotalsDao>();
        var establishmentDestinations = ToEstablishmentDestinations(establishmentDestinationsDao);
        var englandDestinations = ToEnglandDestinations(englandDestinationsDao);

        LAPerformance? localAuthorityPerformance = null;
        LADestinations? localAuthorityDestinations = null;
        if (!string.IsNullOrWhiteSpace(establishmentInfo?.LAId))
        {
            const string laSql = """
                SELECT *
                FROM public.v_la_performance
                WHERE "Id" = @laId
                LIMIT 1;

                SELECT
                    "Id",
                    "AllDest_Tot_LA_Current_Pct"::text AS "AllDestCurrentPct",
                    "AllDest_Tot_LA_Previous_Pct"::text AS "AllDestPreviousPct",
                    "AllDest_Tot_LA_Previous2_Pct"::text AS "AllDestPrevious2Pct",
                    "Education_Tot_LA_Current_Pct"::text AS "EducationCurrentPct",
                    "Education_Tot_LA_Previous_Pct"::text AS "EducationPreviousPct",
                    "Education_Tot_LA_Previous2_Pct"::text AS "EducationPrevious2Pct",
                    "Employment_Tot_LA_Current_Pct"::text AS "EmploymentCurrentPct",
                    "Employment_Tot_LA_Previous_Pct"::text AS "EmploymentPreviousPct",
                    "Employment_Tot_LA_Previous2_Pct"::text AS "EmploymentPrevious2Pct"
                FROM public.v_la_destinations
                WHERE "Id" = @laId
                LIMIT 1;
            """;
            using var laResults = await conn.QueryMultipleAsync(laSql, new { laId = establishmentInfo!.LAId });
            localAuthorityPerformance = await laResults.ReadSingleOrDefaultAsync<LAPerformance>();
            var localAuthorityDestinationsDao = await laResults.ReadSingleOrDefaultAsync<DestinationTotalsDao>();
            localAuthorityDestinations = ToLocalAuthorityDestinations(localAuthorityDestinationsDao);
        }
        else
        {
            _logger.LogWarning("No LAId found for URN {Urn} when loading KS4 performance.", urn);
        }

        if (establishmentPerformance is null
            && localAuthorityPerformance is null
            && englandPerformance is null
            && establishmentDestinations is null
            && localAuthorityDestinations is null
            && englandDestinations is null)
        {
            return null;
        }

        return new(
            establishmentPerformance,
            localAuthorityPerformance,
            englandPerformance,
            establishmentDestinations,
            localAuthorityDestinations,
            englandDestinations);
    }

    private sealed class EstablishmentInfo
    {
        public string? LAId { get; set; }
    }

    private sealed class DestinationTotalsDao
    {
        public string? Id { get; init; }
        public string? AllDestCurrentPct { get; init; }
        public string? AllDestPreviousPct { get; init; }
        public string? AllDestPrevious2Pct { get; init; }
        public string? EducationCurrentPct { get; init; }
        public string? EducationPreviousPct { get; init; }
        public string? EducationPrevious2Pct { get; init; }
        public string? EmploymentCurrentPct { get; init; }
        public string? EmploymentPreviousPct { get; init; }
        public string? EmploymentPrevious2Pct { get; init; }
    }

    private static EstablishmentDestinations? ToEstablishmentDestinations(DestinationTotalsDao? dao)
    {
        if (dao is null) return null;

        return new EstablishmentDestinations
        {
            Id = dao.Id ?? string.Empty,
            AllDest_Tot_Est_Current_Pct = ParseNullableDouble(dao.AllDestCurrentPct),
            AllDest_Tot_Est_Previous_Pct = ParseNullableDouble(dao.AllDestPreviousPct),
            AllDest_Tot_Est_Previous2_Pct = ParseNullableDouble(dao.AllDestPrevious2Pct),
            Education_Tot_Est_Current_Pct = ParseNullableDouble(dao.EducationCurrentPct),
            Education_Tot_Est_Previous_Pct = ParseNullableDouble(dao.EducationPreviousPct),
            Education_Tot_Est_Previous2_Pct = ParseNullableDouble(dao.EducationPrevious2Pct),
            Employment_Tot_Est_Current_Pct = ParseNullableDouble(dao.EmploymentCurrentPct),
            Employment_Tot_Est_Previous_Pct = ParseNullableDouble(dao.EmploymentPreviousPct),
            Employment_Tot_Est_Previous2_Pct = ParseNullableDouble(dao.EmploymentPrevious2Pct)
        };
    }

    private static LADestinations? ToLocalAuthorityDestinations(DestinationTotalsDao? dao)
    {
        if (dao is null) return null;

        return new LADestinations
        {
            Id = dao.Id ?? string.Empty,
            AllDest_Tot_LA_Current_Pct = ParseNullableDouble(dao.AllDestCurrentPct),
            AllDest_Tot_LA_Previous_Pct = ParseNullableDouble(dao.AllDestPreviousPct),
            AllDest_Tot_LA_Previous2_Pct = ParseNullableDouble(dao.AllDestPrevious2Pct),
            Education_Tot_LA_Current_Pct = ParseNullableDouble(dao.EducationCurrentPct),
            Education_Tot_LA_Previous_Pct = ParseNullableDouble(dao.EducationPreviousPct),
            Education_Tot_LA_Previous2_Pct = ParseNullableDouble(dao.EducationPrevious2Pct),
            Employment_Tot_LA_Current_Pct = ParseNullableDouble(dao.EmploymentCurrentPct),
            Employment_Tot_LA_Previous_Pct = ParseNullableDouble(dao.EmploymentPreviousPct),
            Employment_Tot_LA_Previous2_Pct = ParseNullableDouble(dao.EmploymentPrevious2Pct)
        };
    }

    private static EnglandDestinations? ToEnglandDestinations(DestinationTotalsDao? dao)
    {
        if (dao is null) return null;

        return new EnglandDestinations
        {
            AllDest_Tot_Eng_Current_Pct = ParseNullableDouble(dao.AllDestCurrentPct),
            AllDest_Tot_Eng_Previous_Pct = ParseNullableDouble(dao.AllDestPreviousPct),
            AllDest_Tot_Eng_Previous2_Pct = ParseNullableDouble(dao.AllDestPrevious2Pct),
            Education_Tot_Eng_Current_Pct = ParseNullableDouble(dao.EducationCurrentPct),
            Education_Tot_Eng_Previous_Pct = ParseNullableDouble(dao.EducationPreviousPct),
            Education_Tot_Eng_Previous2_Pct = ParseNullableDouble(dao.EducationPrevious2Pct),
            Employment_Tot_Eng_Current_Pct = ParseNullableDouble(dao.EmploymentCurrentPct),
            Employment_Tot_Eng_Previous_Pct = ParseNullableDouble(dao.EmploymentPreviousPct),
            Employment_Tot_Eng_Previous2_Pct = ParseNullableDouble(dao.EmploymentPrevious2Pct)
        };
    }

    private static double? ParseNullableDouble(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : null;
    }
}
