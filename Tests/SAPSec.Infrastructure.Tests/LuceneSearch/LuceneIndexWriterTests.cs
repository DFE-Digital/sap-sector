using Lucene.Net.Index;
using Lucene.Net.Search;
using SAPSec.Core.Model;
using SAPSec.Infrastructure.Entities;
using SAPSec.Infrastructure.LuceneSearch.Implementation;

namespace SAPSec.Infrastructure.Tests.LuceneSearch;

public class LuceneIndexWriterTests
{
    [Fact]
    public void BuildIndex_Writes_All_Repository_Items()
    {
        using var ctx = new LuceneIndexContext();
        var sut = new LuceneIndexWriter(ctx);

        sut.BuildIndex([
            new Establishment{URN = "1", UKPRN = 10, LAId = 100, EstablishmentNumber =  1000, EstablishmentName = "Saint Peter School" },
            new Establishment{URN = "2", UKPRN = 20, LAId = 200, EstablishmentNumber =  2000, EstablishmentName = "Green Lane Primary" },
            new Establishment{URN = "3", UKPRN = 30, LAId = 300, EstablishmentNumber =  3000, EstablishmentName = "Green Park High" }
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
