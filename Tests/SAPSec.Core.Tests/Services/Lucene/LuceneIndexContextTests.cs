using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using SAPSec.Core.Services.Lucene;

namespace SAPSec.Core.Tests.Services.Lucene;

public class LuceneIndexContextTests
{
    [Fact]
    public void Can_Write_And_Search_Document_InMemory()
    {
        using var ctx = new LuceneIndexContext();

        var writer = ctx.Writer;
        var doc = new Document
        {
            new StringField("id", "1", Field.Store.YES),
            new TextField("name", "Saint Peter School", Field.Store.YES)
        };
        writer.AddDocument(doc);
        writer.Flush(triggerMerge: true, applyAllDeletes: true);
        ctx.SearcherManager.MaybeRefreshBlocking();

        var searcher = ctx.SearcherManager.Acquire();
        try
        {
            var query = new TermQuery(new Term("name", "saint"));
            var hits = searcher.Search(query, 10);
            hits.ScoreDocs.Length.Should().BeGreaterThan(0);
        }
        finally
        {
            ctx.SearcherManager.Release(searcher);
        }
    }
}
