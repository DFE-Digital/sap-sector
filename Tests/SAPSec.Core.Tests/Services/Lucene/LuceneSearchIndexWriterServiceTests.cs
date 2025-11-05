using Lucene.Net.Index;
using Lucene.Net.Search;
using Moq;
using SAPSec.Core.Services.Lucene;
using SAPSec.Infrastructure.Entities;
using SAPSec.Infrastructure.Interfaces;

namespace SAPSec.Core.Tests.Services.Lucene;

public class LuceneSearchIndexWriterServiceTests
{
    [Fact]
    public void BuildIndex_Writes_All_Repository_Items()
    {
        // Arrange
        var repo = new Mock<IEstablishmentRepository>();
        repo.Setup(r => r.GetAll()).Returns([
            new Establishment(1, "Saint Peter School"),
            new Establishment(2, "Green Lane Primary")
        ]);
        using var ctx = new LuceneIndexContext();
        var sut = new LuceneSearchIndexWriterService(repo.Object, ctx);

        // Act
        sut.BuildIndex();

        // Assert: the index should contain two docs, verifiable via search on common tokens
        ctx.SearcherManager.MaybeRefreshBlocking();
        var searcher = ctx.SearcherManager.Acquire();
        try
        {
            var totalHits = 0;
            foreach (var term in new[] { "saint", "green", "lane", "primary" })
            {
                var hits = searcher.Search(new TermQuery(new Term("name", term)), 10);
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
