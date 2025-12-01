using SAPSec.Core.Services.Lucene;
using SAPSec.Infrastructure.Entities;

namespace SAPSec.Core.Tests.Services.Lucene;

public class LuceneSynonymMapTests
{
    private readonly LuceneIndexWriter _writer;
    private readonly LuceneIndexReader _sut;

    public LuceneSynonymMapTests()
    {
        var ctx = new LuceneIndexContext();
        _writer = new LuceneIndexWriter(ctx);
        var tokeniser = new LuceneTokeniser(ctx);
        var hlt = new LuceneHighlighter();
        _sut = new LuceneIndexReader(ctx, tokeniser, hlt);
    }

    [Theory]
    [InlineData("st")]
    [InlineData("ss")]
    [InlineData("saint")]
    public async Task SearchAsync_Expands_Saint_Synonym(string input)
    {
        _writer.BuildIndex([
            new School(1, 10, 100, 1000, "Saint Peter School"),
            new School(2, 20, 200, 2000, "ss helan Primary"),
            new School(3, 30, 300, 3000, "st park High")
        ]);

        var results = await _sut.SearchAsync(input);

        results.Should().HaveCount(3);
    }

    [Theory]
    [InlineData("cofe")]
    [InlineData("Church of England")]
    public async Task SearchAsync_Expands_cofe_Synonym(string input)
    {
        _writer.BuildIndex([
            new School(1, 10, 100, 1000, "Saint cofe School"),
            new School(2, 20, 200, 2000, "ss helan Church of England Primary"),
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
            new School(1, 10, 100, 1000, "Saint rm School"),
            new School(2, 20, 200, 2000, "ss helan roman catholic Primary"),
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
            new School(1, 10, 100, 1000, "School London ave"),
            new School(2, 20, 200, 2000, "Primary School London ave."),
            new School(2, 20, 200, 2000, "Primary School London aven"),
            new School(2, 20, 200, 2000, "Primary School London avenue"),
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
            new School(1, 10, 100, 1000, $"School London {synonym}")
        ]);

        var results = await _sut.SearchAsync(input);

        results.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchAsync_Expands_Multiple_Abbreviations()
    {
        const string Input = "st aven";

        _writer.BuildIndex([
            new School(1, 10, 100, 1000, "Saint Peter School London ave"),
            new School(2, 20, 200, 2000, "ss Primary School London avenue"),
        ]);

        var result = await _sut.SearchAsync(Input);

        result.Should().HaveCount(2);
    }
}