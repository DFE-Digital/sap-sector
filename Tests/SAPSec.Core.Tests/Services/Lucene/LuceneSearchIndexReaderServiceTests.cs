using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Services.Lucene;
using SAPSec.Infrastructure.Entities;

namespace SAPSec.Core.Tests.Services.Lucene;

public class LuceneSearchIndexReaderServiceTests
{
    [Fact]
    public async Task SearchAsync_Finds_Items_With_Abbreviation_Expansion_And_Tokenisation()
    {
        // Arrange: build an index with two schools
        using var ctx = new LuceneIndexContext();
        var writer = new LuceneSearchIndexWriterService(new TestRepo([
            new Establishment(1, "Saint Peter School"),
            new Establishment(2, "Green Lane Primary")
        ]), ctx);
        writer.BuildIndex();

        var expander = new AbbreviationExpander();
        var tokeniser = new QueryTokeniser(ctx);
        ISearchService sut = new LuceneSearchIndexReaderService(ctx, expander, tokeniser);

        // Act: search using the abbreviation 'St' that should expand to 'saint'
        var results = await sut.SearchAsync("St Peter");

        // Assert
        results.Should().NotBeNull();
        results.Should().NotBeEmpty();
        results.Select(r => r.Establishment.EstablishmentName)
               .Should().Contain(name => name.Contains("Saint Peter"));
    }

    [Fact]
    public async Task SuggestAsync_Returns_Top_Matching_Names()
    {
        using var ctx = new LuceneIndexContext();
        var writer = new LuceneSearchIndexWriterService(new TestRepo([
            new Establishment(1, "Saint Peter School"),
            new Establishment(2, "Green Lane Primary"),
            new Establishment(3, "Green Park High")
        ]), ctx);
        writer.BuildIndex();

        var expander = new AbbreviationExpander();
        var tokeniser = new QueryTokeniser(ctx);
        ISearchService sut = new LuceneSearchIndexReaderService(ctx, expander, tokeniser);

        var suggestions = await sut.SuggestAsync("Green");

        suggestions.Should().Contain(s => s.StartsWith("Green"));
        suggestions.Should().HaveCountGreaterThan(0);
    }

    private sealed record TestRepo(IEnumerable<Establishment> Items) : SAPSec.Infrastructure.Interfaces.IEstablishmentRepository
    {
        public IEnumerable<Establishment> GetAll() => Items;
    }
}
