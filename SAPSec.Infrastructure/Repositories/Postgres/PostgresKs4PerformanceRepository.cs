using Dapper;
using Microsoft.Extensions.Logging;
using SAPSec.Core.Features.Ks4HeadlineMeasures;
using SAPSec.Core.Model.KS4.Performance;
using SAPSec.Infrastructure.Factories;

namespace SAPSec.Infrastructure.Repositories.Postgres;

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
            SELECT "LAId", "TotalPupils"
            FROM public.v_establishment
            WHERE "URN" = @urn
            LIMIT 1;

            SELECT SUM("TotalPupils")::int AS "EnglandTotalPupils"
            FROM public.v_establishment;

            SELECT *
            FROM public.v_establishment_performance
            WHERE "Id" = @urn;

            SELECT *
            FROM public.v_england_performance
            WHERE "Id" = 'National'
            LIMIT 1;
        """;

        using var results = await conn.QueryMultipleAsync(sql, new { urn });

        var establishmentInfo = await results.ReadSingleOrDefaultAsync<EstablishmentInfo>();
        var englandTotalPupils = await results.ReadSingleOrDefaultAsync<int?>();
        var establishmentPerformance = await results.ReadSingleOrDefaultAsync<EstablishmentPerformance>();
        var englandPerformance = await results.ReadSingleOrDefaultAsync<EnglandPerformance>();

        LAPerformance? localAuthorityPerformance = null;
        if (!string.IsNullOrWhiteSpace(establishmentInfo?.LAId))
        {
            const string laSql = """
                SELECT *
                FROM public.v_la_performance
                WHERE "Id" = @laId
                LIMIT 1;
            """;

            localAuthorityPerformance = await conn.QuerySingleOrDefaultAsync<LAPerformance>(laSql, new { laId = establishmentInfo!.LAId });
        }
        else
        {
            _logger.LogWarning("No LAId found for URN {Urn} when loading KS4 performance.", urn);
        }

        if (establishmentPerformance is null && localAuthorityPerformance is null && englandPerformance is null)
        {
            return null;
        }

        return new(
            establishmentPerformance,
            localAuthorityPerformance,
            englandPerformance,
            establishmentInfo?.TotalPupils,
            englandTotalPupils);
    }

    private sealed class EstablishmentInfo
    {
        public string? LAId { get; set; }
        public int? TotalPupils { get; set; }
    }
}
