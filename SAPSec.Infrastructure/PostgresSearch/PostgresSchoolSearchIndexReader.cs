using SAPSec.Core.Features.SchoolSearch;
using SAPSec.Data.Repositories;
using System.Text.RegularExpressions;

namespace SAPSec.Infrastructure.PostgresSearch;

/// <summary>
/// School search backed by Postgres full text search (see v_establishment_search in the SAPData pipeline).
/// Matched words in the establishment name are wrapped in '*' for the front end to render as bold.
/// </summary>
public class PostgresSchoolSearchIndexReader(IEstablishmentRepository establishmentRepository) : ISchoolSearchIndexReader
{
    private static readonly Regex Word = new(@"[\p{L}\p{N}]+", RegexOptions.Compiled);

    public async Task<IList<(int urn, string resultText)>> SearchAsync(string query, int maxResults = 10)
    {
        var tokens = SearchTokeniser.Tokenise(query);
        if (tokens.Count == 0)
        {
            return [];
        }

        // A query made only of very common words ("school") matches nearly every row; cap it to protect the database.
        var limit = BroadSearchTerms.IsBroadQuery(tokens)
            ? Math.Min(maxResults, BroadSearchTerms.BroadQueryLimit)
            : maxResults;

        var hits = await establishmentRepository.SearchEstablishmentsAsync(query, limit);

        var results = new List<(int, string)>();
        foreach (var hit in hits)
        {
            if (!int.TryParse(hit.URN, out var urn))
            {
                continue;
            }

            results.Add((urn, Highlight(hit.EstablishmentName, tokens)));
        }

        return results;
    }

    private static string Highlight(string name, IReadOnlyList<string> tokens)
    {
        var head = tokens.Take(tokens.Count - 1).ToHashSet();
        var last = tokens[^1];

        return Word.Replace(name, m =>
        {
            var word = m.Value.ToLowerInvariant();

            return head.Contains(word) || word.StartsWith(last, StringComparison.Ordinal)
                ? $"*{m.Value}*"
                : m.Value;
        });
    }
}
