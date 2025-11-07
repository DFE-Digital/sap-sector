using Lucene.Net.Index;
using Lucene.Net.Search;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Infrastructure.Entities;

namespace SAPSec.Core.Services.Lucene;

public class LuceneSearchIndexReaderService(LuceneIndexContext context, IAbbreviationExpander expander, IQueryTokeniser queryTokeniser) : ISearchService
{
    private const string FieldId = "id";
    private const string FieldName = "name";

    public async Task<IReadOnlyList<SearchResult>> SearchAsync(string query, int take = 10)
    {
        if (string.IsNullOrWhiteSpace(query)) return [];

        var expanded = expander.ExpandTerms(query);

        var tokenised =  queryTokeniser.Tokenise(expanded);

        await Task.Yield();

        context.SearcherManager.MaybeRefresh();

        var searcher = context.SearcherManager.Acquire();

        try
        {
            var booleanQuery = new BooleanQuery();
            foreach (var token in tokenised)
            {
                var phraseQuery = new PhraseQuery { new Term(FieldName, token) };
                booleanQuery.Add(phraseQuery, Occur.SHOULD);

                //Enable to expand the query with fuzzy matching
                // var fuzzy = new FuzzyQuery(new Term(FieldName, token), maxEdits: 2);
                // booleanQuery.Add(fuzzy, Occur.SHOULD);
            }

            var topDocs = searcher.Search(booleanQuery, take);

            var results = new List<SearchResult>(Math.Min(take, topDocs.ScoreDocs.Length));

            foreach (var sd in topDocs.ScoreDocs)
            {
                var doc = searcher.Doc(sd.Doc);
                var id = int.Parse(doc.Get(FieldId));
                var name = doc.Get(FieldName);
                results.Add(new SearchResult(new Establishment(id, name), sd.Score));
            }

            return results;
        }
        finally
        {
            context.SearcherManager.Release(searcher);
        }
    }

    public async Task<IReadOnlyList<string>> SuggestAsync(string prefix, int take = 10)
    {
        //TODO: investigate if we can use the Lucene.Net.Suggest here

        var result = await SearchAsync(prefix, take);

        return result.Select(s => s.Establishment.EstablishmentName).ToList();
    }
}