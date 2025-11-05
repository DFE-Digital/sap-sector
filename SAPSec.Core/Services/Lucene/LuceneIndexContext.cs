using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Analysis.Phonetic;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Util;

namespace SAPSec.Core.Services.Lucene;

public sealed class LuceneIndexContext : IDisposable
{
    private global::Lucene.Net.Store.Directory Directory { get; }

    private const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;
    public Analyzer Analyzer { get; }
    public IndexWriter Writer { get; }
    public SearcherManager SearcherManager { get; }

    public LuceneIndexContext() : this(new global::Lucene.Net.Store.RAMDirectory())
    {
    }

    public LuceneIndexContext(global::Lucene.Net.Store.Directory directory)
    {
        Directory = directory;

        Analyzer = new PhoneticInjectingAnalyzer(AppLuceneVersion);
        var indexConfig = new IndexWriterConfig(AppLuceneVersion, Analyzer)
        {
            OpenMode = OpenMode.CREATE_OR_APPEND
        };
        Writer = new IndexWriter(Directory, indexConfig);
        SearcherManager = new SearcherManager(Writer, true, null);
    }

    private sealed class PhoneticInjectingAnalyzer(LuceneVersion version, int maxCodeLen = 4, bool inject = true) : Analyzer
    {
        protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
        {
            // StandardTokenizer -> LowerCase -> ASCIIFolding -> DoubleMetaphoneFilter (inject phonetic tokens)
            var src = new StandardTokenizer(version, reader);

            TokenStream tok = new LowerCaseFilter(version, src);

            // ASCII folding helps match accents etc.
            tok = new ASCIIFoldingFilter(tok);

            // DoubleMetaphoneFilter comes from Lucene.Net.Analysis.Phonetic
            tok = new DoubleMetaphoneFilter(tok, maxCodeLen, inject);

            return new TokenStreamComponents(src, tok);
        }
    }

    public void Dispose()
    {
        SearcherManager.Dispose();
        Writer.Dispose();
        Directory.Dispose();
        Analyzer.Dispose();
    }
}
