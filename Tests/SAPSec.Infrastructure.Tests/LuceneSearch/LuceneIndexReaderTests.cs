using SAPSec.Core.Model;
using SAPSec.Infrastructure.Entities;
using SAPSec.Infrastructure.LuceneSearch;

namespace SAPSec.Infrastructure.Tests.LuceneSearch;

public class LuceneIndexReaderTests
{
    private readonly LuceneIndexWriter _writer;
    private readonly LuceneShoolSearchIndexReader _sut;

    private Establishment FakeEstablishmentOne = new()
    {
        URN = "1",
        UKPRN = "2",
        LAId = "3",
        EstablishmentNumber = "4",
        EstablishmentName = "Fake School One"
    };
    private Establishment FakeEstablishmentTwo = new()
    {
        URN = "10",
        UKPRN = "20",
        LAId = "30",
        EstablishmentNumber = "40",
        EstablishmentName = "Fake School Two"
    };
    private Establishment FakeEstablishmentThree = new()
    {
        URN = "15",
        UKPRN = "25",
        LAId = "35",
        EstablishmentNumber = "45",
        EstablishmentName = "Saint Fake School Three"
    };


    public LuceneIndexReaderTests()
    {
        var ctx = new LuceneIndexContext();
        _writer = new LuceneIndexWriter(ctx);
        var tokeniser = new LuceneTokeniser(ctx);
        var hlt = new LuceneHighlighter();
        _sut = new LuceneShoolSearchIndexReader(ctx, tokeniser, hlt);
    }

    [Fact]
    public async Task SearchAsync_Finds_Items_With_Abbreviation_Expansion_And_Tokenization()
    {
        // Arrange: build an index with two schools
        _writer.BuildIndex([
            FakeEstablishmentOne,
            FakeEstablishmentTwo,
            FakeEstablishmentThree
        ]);

        // Act: search using the abbreviation 'St' that should expand to 'saint'
        var results = await _sut.SearchAsync("St Fake Three");

        // Assert
        results.Should().NotBeNull();
        results.Should().NotBeEmpty();
        results.First().urn.Should().Be(15);
        results.First().resultText.Should().Contain("*Fake* School *Three*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("    ")]
    public async Task SearchAsync_ReturnsEmpty_ForNullOrWhiteSpace(string? input)
    {
        _writer.BuildIndex([
            FakeEstablishmentOne,
            FakeEstablishmentTwo
        ]);

        var results = await _sut.SearchAsync(input!);

        results.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchAsync_PartialWords_returns_AllMatches()
    {
        const string Input = "fa";

        _writer.BuildIndex([
            FakeEstablishmentOne,
            FakeEstablishmentTwo,
        ]);

        var result = await _sut.SearchAsync(Input);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchAsync_Returns_Empty_Results_For_Out_Of_Order_SearchTerms()
    {
        const string Input = "School Fake";

        _writer.BuildIndex([
            FakeEstablishmentOne,
            FakeEstablishmentTwo,
        ]);

        var result = await _sut.SearchAsync(Input);

        result.Should().HaveCount(2);
    }
}
