using SAPSec.Core.Features.SchoolSearch.Extensions;
using SAPSec.Data.Dto;
using SAPSec.Data.Repositories;
using System.Text.RegularExpressions;

namespace SAPSec.Core.Features.SchoolSearch;

/// <summary>
/// LINQ stand-in for the Postgres full text search, used by the JSON and in-memory repositories
/// (UI / integration / accessibility fixtures that have no database). Postgres is authoritative:
/// keep the synonym groups in step with SAPData/Program.cs (SearchSynonymGroups).
/// </summary>
public static class InMemoryEstablishmentSearch
{
    private static readonly string[][] SynonymGroups =
    [
        ["st", "saint"],
        ["ss", "saints"],
        ["cofe", "church of england"],
        ["rc", "rm", "roman catholic", "catholic"],
        ["ave", "aven", "avenue"],
        ["rd", "road"],
        ["ln", "lane"],
        ["dr", "drive"],
        ["mt", "mount"],
        ["ct", "court"],
        ["pl", "place"],
        ["blvd", "boulevard"],
        ["sq", "square"]
    ];

    public static IReadOnlyCollection<EstablishmentSearchHit> Search(
        IEnumerable<Establishment> establishments, string query, int maxResults)
    {
        var tokens = SearchTokeniser.Tokenise(query);
        if (tokens.Count == 0)
        {
            return [];
        }

        var head = CollapsePhrases(string.Join(' ', tokens.Take(tokens.Count - 1)));
        var last = tokens[^1];

        return establishments
            .Where(e => e.CanIndexForSearch())
            .Select(e => (Establishment: e, Doc: BuildDocument(e)))
            .Where(x => head.All(t => MatchesWord(x.Doc, t)) && x.Doc.Words.Any(w => w.StartsWith(last, StringComparison.Ordinal)))
            .OrderBy(x => x.Establishment.EstablishmentName, StringComparer.OrdinalIgnoreCase)
            .Take(maxResults)
            .Select(x => new EstablishmentSearchHit(x.Establishment.URN, x.Establishment.EstablishmentName ?? string.Empty))
            .ToList();
    }

    // Multi-word synonyms ("church of england") are collapsed to the group's first member ("cofe") so they match as one unit.
    private static List<string> CollapsePhrases(string headText)
    {
        foreach (var group in SynonymGroups)
        {
            foreach (var phrase in group.Where(g => g.Contains(' ')))
            {
                headText = Regex.Replace(headText, $@"\b{Regex.Escape(phrase)}\b", group[0]);
            }
        }

        return headText.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    private static (string Text, HashSet<string> Words) BuildDocument(Establishment e)
    {
        var postcode = e.Postcode ?? string.Empty;
        var text = string.Join(' ', SearchTokeniser.Tokenise($"{e.EstablishmentName} {e.Street} {postcode} {postcode.Replace(" ", string.Empty)}"));

        return (text, text.Split(' ').ToHashSet());
    }

    private static bool MatchesWord((string Text, HashSet<string> Words) doc, string token)
    {
        var variants = SynonymGroups.FirstOrDefault(g => g.Contains(token)) ?? [token];

        return variants.Any(v => Regex.IsMatch(doc.Text, $@"\b{Regex.Escape(v)}\b"));
    }
}
