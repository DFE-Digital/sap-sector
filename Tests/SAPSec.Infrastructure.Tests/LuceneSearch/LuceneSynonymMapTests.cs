using SAPSec.Core.Model;
using SAPSec.Infrastructure.LuceneSearch;

namespace SAPSec.Infrastructure.Tests.LuceneSearch;

public class LuceneSynonymMapTests
{
    private readonly LuceneIndexWriter _writer;
    private readonly LuceneShoolSearchIndexReader _sut;

    public LuceneSynonymMapTests()
    {
        var ctx = new LuceneIndexContext();
        _writer = new LuceneIndexWriter(ctx);
        var tokeniser = new LuceneTokeniser(ctx);
        var hlt = new LuceneHighlighter();
        _sut = new LuceneShoolSearchIndexReader(ctx, tokeniser, hlt);
    }

    [Theory]
    [InlineData("st")]
    [InlineData("saint")]
    public async Task SearchAsync_Expands_Saint_Synonym(string input)
    {
        _writer.BuildIndex([
            new Establishment(){URN = "1",UKPRN = "10",LAId = "100",EstablishmentNumber = "1000",EstablishmentName = "Saint Peter School"},
            new Establishment(){URN = "2",UKPRN = "20",LAId = "200",EstablishmentNumber = "2000",EstablishmentName = "ss helan Primary"},
            new Establishment(){URN = "3",UKPRN = "30",LAId = "300",EstablishmentNumber = "3000",EstablishmentName = "st park High"}
        ]);

        var results = await _sut.SearchAsync(input);

        results.Should().HaveCount(2);
    }

    [Theory]
    [InlineData("ss")]
    public async Task SearchAsync_Expands_Saints_Synonym(string input)
    {
        _writer.BuildIndex([
            new Establishment(){URN = "1",UKPRN = "10",LAId = "100",EstablishmentNumber = "1000",EstablishmentName = "Saint Peter School"},
            new Establishment(){URN = "2",UKPRN = "20",LAId = "200",EstablishmentNumber = "2000",EstablishmentName = "ss helan Primary"},
            new Establishment(){URN = "3",UKPRN = "20",LAId = "200",EstablishmentNumber = "2000",EstablishmentName = "st park High"}
        ]);

        var results = await _sut.SearchAsync(input);

        results.Should().HaveCount(1);
    }

    [Theory]
    [InlineData("cofe")]
    [InlineData("Church of England")]
    public async Task SearchAsync_Expands_cofe_Synonym(string input)
    {
        _writer.BuildIndex([
           new Establishment(){URN = "1",UKPRN = "10",LAId = "100",EstablishmentNumber = "1000",EstablishmentName = "Saint Peter Cofe School"},
            new Establishment(){URN = "2",UKPRN = "20",LAId = "200",EstablishmentNumber = "2000",EstablishmentName = "ss helan Primary Church of England"}
       ]);

        var results = await _sut.SearchAsync(input);

        results.Should().HaveCount(2);
    }

    [Theory]
    [InlineData("rm")]
    [InlineData("roman catholic")]
    public async Task SearchAsync_Expands_rm_Synonym(string input)
    {
        _writer.BuildIndex([
           new Establishment(){URN = "1",UKPRN = "10",LAId = "100",EstablishmentNumber = "1000",EstablishmentName = "Saint Peter RM School"},
            new Establishment(){URN = "2",UKPRN = "20",LAId = "200",EstablishmentNumber = "2000",EstablishmentName = "ss helan Primary roman Catholic"}
           ]);

        var results = await _sut.SearchAsync(input);

        results.Should().HaveCount(2);
    }

    [Theory]
    [InlineData("ave")]
    [InlineData("ave.")]
    [InlineData("aven")]
    [InlineData("avenue")]
    public async Task SearchAsync_Expands_ave_Synonym(string input)
    {

        _writer.BuildIndex([
            new Establishment(){URN = "1",UKPRN = "10",LAId = "100",EstablishmentNumber = "1000",EstablishmentName = "Saint Peter ave School"},
            new Establishment(){URN = "2",UKPRN = "20",LAId = "200",EstablishmentNumber = "2000",EstablishmentName = "Primary Ave. School"},
            new Establishment(){URN = "3",UKPRN = "30",LAId = "300",EstablishmentNumber = "3000",EstablishmentName = "Primary Aven School"},
            new Establishment(){URN = "4",UKPRN = "40",LAId = "400",EstablishmentNumber = "4000",EstablishmentName = "Primary avenue School"}
        ]);

        var results = await _sut.SearchAsync(input);

        results.Should().HaveCount(4);
    }

    [Theory]
    [InlineData("rd", "road")]
    [InlineData("road", "rd")]
    [InlineData("ln", "lane")]
    [InlineData("lane", "ln")]
    [InlineData("dr", "drive")]
    [InlineData("drive", "dr")]
    [InlineData("mt", "mount")]
    [InlineData("mount", "mt")]
    [InlineData("ct", "court")]
    [InlineData("court", "ct")]
    [InlineData("pl", "place")]
    [InlineData("place", "pl")]
    [InlineData("blvd", "boulevard")]
    [InlineData("boulevard", "blvd")]
    [InlineData("sq", "square")]
    [InlineData("square", "sq")]
    public async Task SearchAsync_Expands_Synonym(string input, string synonym)
    {
        _writer.BuildIndex([
            new Establishment(){URN = "1",UKPRN = "10",LAId = "100",EstablishmentNumber = "1000",EstablishmentName = $"Saint Peter {synonym} School"},
        ]);

        var results = await _sut.SearchAsync(input);

        results.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchAsync_Expands_Multiple_Abbreviations()
    {
        const string Input = "st aven";

        _writer.BuildIndex([
            new Establishment(){URN = "1",UKPRN = "10",LAId = "100",EstablishmentNumber = "1000",EstablishmentName = "Saint Peter st ave School"},
            new Establishment(){URN = "2",UKPRN = "20",LAId = "200",EstablishmentNumber = "2000",EstablishmentName = "Primary st Avenue School"},
        ]);

        var result = await _sut.SearchAsync(Input);

        result.Should().HaveCount(2);
    }
}