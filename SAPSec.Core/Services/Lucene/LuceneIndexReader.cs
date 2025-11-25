using Lucene.Net.Index;
using Lucene.Net.Search;
using SAPSec.Core.Interfaces.Services.Lucene;
using SAPSec.Infrastructure.Entities;

namespace SAPSec.Core.Services.Lucene;

public class LuceneIndexReader(LuceneIndexContext context, ILuceneTokeniser luceneTokeniser, ILuceneHighlighter highlighter) : ILuceneIndexReader
{
    public async Task<IList<(int urn, string resultText)>> SearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return [];

        var tokens = luceneTokeniser.Tokenise(query).ToList();
        if (!tokens.Any()) return [];

        await Task.Yield();

        context.SearcherManager.MaybeRefresh();

        var searcher = context.SearcherManager.Acquire();

        try
        {
            // Strict query for all but the LAST token
            var must = new BooleanQuery();
            foreach (var t in tokens.Take(tokens.Count - 1))
            {
                must.Add(new TermQuery(new Term(FieldName.EstablishmentName, t)), Occur.MUST);
            }

            // Add the LAST token as a PrefixQuery for partial matching
            must.Add(new PrefixQuery(new Term(FieldName.EstablishmentName, tokens.Last())), Occur.MUST);

            //Phrase boost – original order
            var phrase = new PhraseQuery { Slop = 2, Boost = 5f };
            foreach (var t in tokens)
            {
                phrase.Add(new Term(FieldName.EstablishmentName, t));
            }

            // Exact name
            var exactName = new TermQuery(new Term(FieldName.EstablishmentName, query))
            {
                Boost = 10f
            };

            //Combine
            var finalQuery = new BooleanQuery
            {
                { must, Occur.MUST },
                { phrase, Occur.SHOULD },
                { exactName, Occur.SHOULD }
            };

            var take = 10;

            var sort = new Sort(SortField.FIELD_SCORE, new SortField(FieldName.EstablishmentNameSort, SortFieldType.STRING, reverse: false));

            var topDocs = searcher.Search(finalQuery, take, sort);

            var results = new List<(int, string)>();

            foreach (var sd in topDocs.ScoreDocs)
            {
                var doc = searcher.Doc(sd.Doc);
                var urn = int.Parse(doc.Get(FieldName.Urn));
                var establishmentName = doc.Get(FieldName.EstablishmentName);

                var highlightedText = highlighter.HighlightText(finalQuery, establishmentName, FieldName.EstablishmentName);

                results.Add((urn, highlightedText));
            }

            return results;
        }
        finally
        {
            context.SearcherManager.Release(searcher);
        }
    }
}