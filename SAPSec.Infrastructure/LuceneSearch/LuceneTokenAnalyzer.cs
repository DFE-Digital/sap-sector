using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.En;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.Synonym;
using Lucene.Net.Util;

namespace SAPSec.Infrastructure.LuceneSearch;

public class LuceneTokenAnalyzer(LuceneVersion version, LuceneSynonymMapBuilder luceneSynonymMapBuilder, bool enableSynonym) : Analyzer
{
    private readonly SynonymMap _synonymMap = luceneSynonymMapBuilder.BuildSynonymMap();

    protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
    {
        var source = new StandardTokenizer(version, reader);

        //Lowercase all tokens
        TokenStream tokenStream = new LowerCaseFilter(version, source);

        // Remove possessives: "Peter's" → "Peter"
        tokenStream = new EnglishPossessiveFilter(version, tokenStream);

        //expand synonyms at index time only
        if (enableSynonym) tokenStream = new SynonymFilter(tokenStream, _synonymMap, true);

        return new TokenStreamComponents(source, tokenStream);
    }
}