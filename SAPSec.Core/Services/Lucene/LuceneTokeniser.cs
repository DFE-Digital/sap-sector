using Lucene.Net.Analysis.TokenAttributes;
using SAPSec.Core.Interfaces.Services.Lucene;
using SAPSec.Infrastructure.Entities;

namespace SAPSec.Core.Services.Lucene;

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