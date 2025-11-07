using Lucene.Net.Analysis.TokenAttributes;
using SAPSec.Core.Interfaces.Services;

namespace SAPSec.Core.Services.Lucene;

public class QueryTokeniser(LuceneIndexContext context) : IQueryTokeniser
{
    public IEnumerable<string> Tokenise(string finalText)
    {
        if (string.IsNullOrWhiteSpace(finalText)) return [];

        // Tokenize the finalText using the analyzer
        var tokens = new List<string>();
        using var reader = new StringReader(finalText);
        using var tokenStream = context.Analyzer.GetTokenStream("text", reader);
        tokenStream.Reset();
        var termAttr = tokenStream.AddAttribute<ICharTermAttribute>();
        while (tokenStream.IncrementToken())
        {
            var item = termAttr.ToString();
            if (!string.IsNullOrWhiteSpace(item))
            {
                tokens.Add(item);
            }
        }
        tokenStream.End();

        return tokens;
    }
}