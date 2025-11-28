using Lucene.Net.Index;
using Lucene.Net.Search;
using SAPSec.Core.Services.Lucene;
using SAPSec.Infrastructure.Entities;

namespace SAPSec.Core.Tests.Services.Lucene;

public class LuceneIndexWriterTests
{
    [Fact]
    public void BuildIndex_Writes_All_Repository_Items()
    {
        using var ctx = new LuceneIndexContext();
        var sut = new LuceneIndexWriter(ctx);

        sut.BuildIndex([
            new School(1, 10, 100, 1000, "Saint Peter School"),
            new School(2, 20, 200, 2000, "Green Lane Primary"),
            new School(3, 30, 300, 3000, "Green Park High")
        ]);

        ctx.SearcherManager.MaybeRefreshBlocking();
        var searcher = ctx.SearcherManager.Acquire();
        try
        {
            var totalHits = 0;
            foreach (var term in new[] { "saint", "green", "lane", "Park" })
            {
                var hits = searcher.Search(new TermQuery(new Term(FieldName.EstablishmentName, term)), 10);
                totalHits += hits.TotalHits;
            }
            totalHits.Should().BeGreaterThan(0);
        }
        finally
        {
            ctx.SearcherManager.Release(searcher);
        }
    }
}
