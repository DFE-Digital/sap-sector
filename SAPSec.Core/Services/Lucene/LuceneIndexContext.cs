using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Directory = Lucene.Net.Store.Directory;

namespace SAPSec.Core.Services.Lucene;

public sealed class LuceneIndexContext : IDisposable
{
    private Directory Directory { get; }

    private const LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;
    public Analyzer Analyzer { get; }
    public IndexWriter Writer { get; }
    public SearcherManager SearcherManager { get; }

    public LuceneIndexContext()
    {
        Directory = new RAMDirectory();

        var luceneSynonymMapBuilder = new LuceneSynonymMapBuilder();

        //Enable synonyms for Write and Query
        Analyzer = new LuceneTokenAnalyzer(AppLuceneVersion, luceneSynonymMapBuilder, true);

        var indexConfig = new IndexWriterConfig(AppLuceneVersion, Analyzer)
        {
            OpenMode = OpenMode.CREATE_OR_APPEND
        };

        Writer = new IndexWriter(Directory, indexConfig);

        SearcherManager = new SearcherManager(Writer, true, null);
    }

    public void Dispose()
    {
        SearcherManager.Dispose();
        Writer.Dispose();
        Directory.Dispose();
        Analyzer.Dispose();
    }
}