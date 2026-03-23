using Dapper;
using Microsoft.Extensions.Logging;
using SAPSec.Core.Model.Generated;
using SAPSec.Data;
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
                "AllDest_Tot_Eng_Current_Pct",
                "AllDest_Tot_Eng_Previous_Pct",
                "AllDest_Tot_Eng_Previous2_Pct",
                "Education_Tot_Eng_Current_Pct",
                "Education_Tot_Eng_Previous_Pct",
                "Education_Tot_Eng_Previous2_Pct",
                "Employment_Tot_Eng_Current_Pct",
                "Employment_Tot_Eng_Previous_Pct",
                "Employment_Tot_Eng_Previous2_Pct"
            FROM public.v_england_destinations
            WHERE "Id" = 'National'
            LIMIT 1;
        """;

        using var results = await conn.QueryMultipleAsync(sql, new { urn });

        var establishmentInfo = await results.ReadSingleOrDefaultAsync<EstablishmentInfo>();
        var establishmentPerformance = await results.ReadSingleOrDefaultAsync<EstablishmentPerformance>();
        var englandPerformance = await results.ReadSingleOrDefaultAsync<EnglandPerformance>();
        var establishmentDestinations = await results.ReadSingleOrDefaultAsync<EstablishmentDestinations>();
        var englandDestinations = await results.ReadSingleOrDefaultAsync<EnglandDestinations>();

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
                    "AllDest_Tot_LA_Current_Pct",
                    "AllDest_Tot_LA_Previous_Pct",
                    "AllDest_Tot_LA_Previous2_Pct",
                    "Education_Tot_LA_Current_Pct",
                    "Education_Tot_LA_Previous_Pct",
                    "Education_Tot_LA_Previous2_Pct",
                    "Employment_Tot_LA_Current_Pct",
                    "Employment_Tot_LA_Previous_Pct",
                    "Employment_Tot_LA_Previous2_Pct"
                FROM public.v_la_destinations
                WHERE "Id" = @laId
                LIMIT 1;
            """;
            using var laResults = await conn.QueryMultipleAsync(laSql, new { laId = establishmentInfo!.LAId });
            localAuthorityPerformance = await laResults.ReadSingleOrDefaultAsync<LAPerformance>();
            localAuthorityDestinations = await laResults.ReadSingleOrDefaultAsync<LADestinations>();
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

    public async Task<IReadOnlyCollection<EstablishmentPerformance>> GetEstablishmentPerformanceAsync(IEnumerable<string> urns)
    {
        using var conn = await _factory.Create().OpenConnectionAsync();

        const string sql = """
            SELECT *
            FROM public.v_establishment_performance
            WHERE "Id"  = ANY(@urns);
        """;

        var result = await conn.QueryAsync<EstablishmentPerformance>(sql, new { urns = urns.ToArray() });
        return result.ToList().AsReadOnly();
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
