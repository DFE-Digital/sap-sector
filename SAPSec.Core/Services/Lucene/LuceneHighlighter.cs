using Lucene.Net.Search;
using Lucene.Net.Search.Highlight;
using Lucene.Net.Util;
using SAPSec.Core.Interfaces.Services.Lucene;

namespace SAPSec.Core.Services.Lucene;

public class LuceneHighlighter : ILuceneHighlighter
{
    private const string PreTag = "*";
    private const string PostTag = "*";

    private readonly SimpleHTMLFormatter _formatter = new SimpleHTMLFormatter(PreTag, PostTag);

    //Disable synonyms for Highlight
    private readonly LuceneTokenAnalyzer _analyzer = new LuceneTokenAnalyzer(LuceneVersion.LUCENE_48, new LuceneSynonymMapBuilder(), false);

    public string HighlightText(Query query, string resultText, string fieldName)
    {
        var scorer = new QueryScorer(query, fieldName);

        var highlighter = new Highlighter(_formatter, scorer)
        {
            // Don't fragment, return the entire resultText
            TextFragmenter = new NullFragmenter()
        };

        using var tokenStream = _analyzer.GetTokenStream(fieldName, resultText);
        var highlightedText = highlighter.GetBestFragment(tokenStream, resultText);
        return highlightedText ?? resultText;
    }
}