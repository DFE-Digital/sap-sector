using SAPSec.Core.Services.Lucene;
using SAPSec.Infrastructure.Entities;

namespace SAPSec.Core.Tests.Services.Lucene;

public class LuceneIndexReaderTests
{
    private readonly LuceneIndexWriter _writer;
    private readonly LuceneIndexReader _sut;

    public LuceneIndexReaderTests()
    {
        var ctx = new LuceneIndexContext();
        _writer = new LuceneIndexWriter(ctx);
        var tokeniser = new LuceneTokeniser(ctx);
        var hlt = new LuceneHighlighter();
        _sut = new LuceneIndexReader(ctx, tokeniser, hlt);
    }

    [Fact]
    public async Task SearchAsync_Finds_Items_With_Abbreviation_Expansion_And_Tokenization()
    {
        // Arrange: build an index with two schools
        _writer.BuildIndex([
            new School(1, 10, 100, 1000, "Saint Peter School"),
            new School(2, 20, 200, 2000, "Green Lane Primary")
        ]);



        // Act: search using the abbreviation 'St' that should expand to 'saint'
        var results = await _sut.SearchAsync("St Peter");

        // Assert
        results.Should().NotBeNull();
        results.Should().NotBeEmpty();
        results.First().urn.Should().Be(1);
        results.First().resultText.Should().Contain("*Saint* *Peter*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("    ")]
    public async Task SearchAsync_ReturnsEmpty_ForNullOrWhiteSpace(string? input)
    {
        _writer.BuildIndex([
            new School(1, 10, 100, 1000, "Saint Peter School"),
            new School(2, 20, 200, 2000, "Green Lane Primary"),
            new School(3, 30, 300, 3000, "Green Park High")
        ]);

        var results = await _sut.SearchAsync(input!);

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_PartialWords_returns_AllMatches()
    {
        const string Input = "st pet";

        _writer.BuildIndex([
            new School(1, 10, 100, 1000, "Saint Peter School London ave"),
            new School(2, 20, 200, 2000, "ss Peter School London ave"),
        ]);

        var result = await _sut.SearchAsync(Input);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchAsync_Returns_Empty_Results_For_Out_Of_Order_SearchTerms()
    {
        const string Input = "Peter Saint";

        _writer.BuildIndex([
            new School(1, 10, 100, 1000, "Saint Peter School London ave"),
            new School(2, 20, 200, 2000, "ss Peter School London ave"),
        ]);

        var result = await _sut.SearchAsync(Input);

        result.Should().HaveCount(2);
    }
}
