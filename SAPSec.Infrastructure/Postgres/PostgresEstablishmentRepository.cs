using Dapper;
using Microsoft.Extensions.Logging;
using SAPSec.Core.Features.SchoolSearch;
using SAPSec.Data.Dto;
using SAPSec.Data.Repositories;

namespace SAPSec.Infrastructure.Postgres;

public class PostgresEstablishmentRepository : IEstablishmentRepository
{
    private readonly ILogger<PostgresEstablishmentRepository> _logger;
    private readonly NpgsqlDataSourceFactory _factory;

    public PostgresEstablishmentRepository(ILogger<PostgresEstablishmentRepository> logger, NpgsqlDataSourceFactory factory)
    {
        _logger = logger;
        _factory = factory;
    }

    public async Task<IReadOnlyCollection<Establishment>> GetAllEstablishmentsAsync()
    {
        using var conn = await _factory.Create().OpenConnectionAsync();

        const string sql = """
            SELECT "URN",
                   "EstablishmentName",
                   "PhaseOfEducationId",
                   "PhaseOfEducationName",
                   "EstablishmentStatusId",
                   "EstablishmentStatusName",
                   "Street",
                   "Postcode"
            FROM public.v_establishment;
        """;

        var result = await conn.QueryAsync<Establishment>(sql);
        return result.ToList();
    }

    public async Task<IReadOnlyCollection<Establishment>> GetEstablishmentsAsync(IEnumerable<string> urns)
    {
        using var conn = await _factory.Create().OpenConnectionAsync();

        const string sql = """
            SELECT *
            FROM public.v_establishment
            WHERE "URN" = ANY(@urns);
        """;

        var result = await conn.QueryAsync<Establishment>(sql, new { urns = urns.ToArray() });
        return result.ToList();
    }

    public async Task<IReadOnlyCollection<EstablishmentSearchHit>> SearchEstablishmentsAsync(string query, int maxResults)
    {
        var tokens = SearchTokeniser.Tokenise(query);
        if (tokens.Count == 0)
        {
            return [];
        }

        // Tokens contain letters/digits only, so they are safe inside a tsquery.
        // All but the last token are synonym-normalised and ANDed; the last is a prefix match.
        var head = string.Join(' ', tokens.Take(tokens.Count - 1));
        var last = tokens[^1];

        using var conn = await _factory.Create().OpenConnectionAsync();

        // Mirrors EstablishmentExtensions.CanIndexForSearch (phase id 2/4/7, or legacy phase name).
        const string sql = """
            WITH q AS (
                SELECT to_tsquery('simple', concat_ws(' & ',
                    NULLIF(array_to_string(array_remove(regexp_split_to_array(trim(normalize_text(@head)), '\s+'), ''), ' & '), ''),
                    @last || ':*')) AS tsq
            )
            SELECT e."URN", e."EstablishmentName"
            FROM public.v_establishment e
            JOIN public.v_establishment_search s ON s."URN" = e."URN"
            CROSS JOIN q
            WHERE s."SearchFTS" @@ q.tsq
              AND (trim(e."PhaseOfEducationId") IN ('2', '4', '7')
                   OR lower(trim(e."PhaseOfEducationName")) IN ('primary', 'secondary', 'all-through'))
            ORDER BY ts_rank_cd(s."SearchFTS", q.tsq) DESC, e."EstablishmentName"
            LIMIT @maxResults;
        """;

        var result = await conn.QueryAsync<EstablishmentSearchHit>(sql, new { head, last, maxResults });
        return result.ToList();
    }

    public async Task<Establishment?> GetEstablishmentAsync(string urn)
    {
        if (string.IsNullOrWhiteSpace(urn))
            return null;

        using var conn = await _factory.Create().OpenConnectionAsync();

        const string sql = """
            SELECT *
            FROM public.v_establishment
            WHERE "URN" = @urn
            LIMIT 1;
        """;

        var result = await conn.QuerySingleOrDefaultAsync<Establishment>(sql, new { urn });

        return result;
    }

    public async Task<Establishment?> GetEstablishmentByAnyNumberAsync(string number)
    {
        if (string.IsNullOrWhiteSpace(number))
            return null;

        using var conn = await _factory.Create().OpenConnectionAsync();

        const string sql = """
            SELECT *
            FROM public.v_establishment
            WHERE "URN" = @number
                OR "UKPRN" = @number
                OR "LAESTAB" = @number
            LIMIT 1;
        """;

        var result = await conn.QuerySingleOrDefaultAsync<Establishment>(sql, new { number });

        return result;
    }

    public async Task<EstablishmentEmail?> GetEstablishmentEmailAsync(string urn)
    {
        if (string.IsNullOrWhiteSpace(urn))
            return null;

        using var conn = await _factory.Create().OpenConnectionAsync();

        const string sql = """
            SELECT *
            FROM public.v_establishment_email
            WHERE "URN" = @urn
            LIMIT 1;
        """;

        var result = await conn.QuerySingleOrDefaultAsync<EstablishmentEmail>(sql, new { urn });

        return result;
    }

    public async Task<IReadOnlyCollection<EstablishmentLinks>> GetEstablishmentLinksAsync(string urn)
    {
        if (string.IsNullOrWhiteSpace(urn))
            return Array.Empty<EstablishmentLinks>();

        using var conn = await _factory.Create().OpenConnectionAsync();

        const string sql = """
            SELECT *
            FROM public.v_establishment_links
            WHERE urn = @urn;
        """;

        var result = await conn.QueryAsync<EstablishmentLinks>(sql, new { urn });

        return result.ToList();
    }
}
