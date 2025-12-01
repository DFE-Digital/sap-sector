using SAPSec.Core.Services.Lucene;

namespace SAPSec.Core.Tests.Services.Lucene;

public class LuceneTokeniserTests
{
    [Fact]
    public void Tokenise_ReturnsEmpty_ForNullOrWhitespace()
    {
        using var ctx = new LuceneIndexContext();
        var sut = new LuceneTokeniser(ctx);

        sut.Tokenise(null!).Should().BeEmpty();
        sut.Tokenise("").Should().BeEmpty();
        sut.Tokenise("   ").Should().BeEmpty();
    }

    [Fact]
    public void Tokenise_ProducesLowercase()
    {
        using var ctx = new LuceneIndexContext();
        var sut = new LuceneTokeniser(ctx);

        var tokens = sut.Tokenise("PETER").ToList();

        tokens.Should().Contain("peter");
    }

    [Fact]
    public void Tokenise_Removes_possessives()
    {
        using var ctx = new LuceneIndexContext();
        var sut = new LuceneTokeniser(ctx);

        var tokens = sut.Tokenise("Peter's").ToList();

        tokens.Should().Contain("peter");
    }
}
