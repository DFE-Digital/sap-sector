using Lucene.Net.Index;
using Lucene.Net.Search;
using SAPSec.Core.Features.SchoolSearch;
using SAPSec.Infrastructure.Entities;

namespace SAPSec.Infrastructure.LuceneSearch;

public class LuceneShoolSearchIndexReader(LuceneIndexContext context, LuceneTokeniser luceneTokeniser, LuceneHighlighter highlighter) : ISchoolSearchIndexReader
{
    public async Task<IList<(int urn, string resultText)>> SearchAsync(string query, int maxResults = 10)
    {
        if (string.IsNullOrWhiteSpace(query)) return [];

        var tokens = luceneTokeniser.Tokenise(query).ToList();
        if (!tokens.Any()) return [];

        await Task.Yield();

        context.SearcherManager.MaybeRefresh();

        var searcher = context.SearcherManager.Acquire();

        try
        {
            // Strict query for all but the LAST token (match on name, street, or postcode)
            var must = new BooleanQuery();
            foreach (var t in tokens.Take(tokens.Count - 1))
            {
                var tokenQuery = new BooleanQuery
                {
                    { new TermQuery(new Term(FieldName.EstablishmentName, t)), Occur.SHOULD },
                    { new TermQuery(new Term(FieldName.Street, t)), Occur.SHOULD },
                    { new TermQuery(new Term(FieldName.Postcode, t)), Occur.SHOULD }
                };
                must.Add(tokenQuery, Occur.MUST);
            }

            // Add the LAST token as a PrefixQuery for partial matching (name, street, or postcode)
            var lastTokenQuery = new BooleanQuery
            {
                { new PrefixQuery(new Term(FieldName.EstablishmentName, tokens.Last())), Occur.SHOULD },
                { new PrefixQuery(new Term(FieldName.Street, tokens.Last())), Occur.SHOULD },
                { new PrefixQuery(new Term(FieldName.Postcode, tokens.Last())), Occur.SHOULD }
            };
            must.Add(lastTokenQuery, Occur.MUST);

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

            var take = maxResults;

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
