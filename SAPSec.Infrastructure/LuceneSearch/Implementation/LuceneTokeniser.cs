using Lucene.Net.Analysis.TokenAttributes;
using SAPSec.Infrastructure.Entities;
using SAPSec.Infrastructure.LuceneSearch.Interfaces;

namespace SAPSec.Infrastructure.LuceneSearch.Implementation;

public class LuceneTokeniser(LuceneIndexContext context) : ILuceneTokeniser
{
    public IEnumerable<string> Tokenise(string finalText)
    {
        if (string.IsNullOrWhiteSpace(finalText)) return [];

        var tokens = new List<string>();

        using var tokenStream = context.Analyzer.GetTokenStream(FieldName.EstablishmentName, finalText);
        var termAttr = tokenStream.AddAttribute<ICharTermAttribute>();
        tokenStream.Reset();

        while (tokenStream.IncrementToken())
        {
            tokens.Add(termAttr.ToString());
        }

        return tokens;
    }
}