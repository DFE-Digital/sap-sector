using SAPSec.Core.Services.Lucene;

namespace SAPSec.Core.Tests.Services.Lucene;

public class QueryTokeniserTests
{
    [Fact]
    public void Tokenise_ReturnsEmpty_ForNullOrWhitespace()
    {
        using var ctx = new LuceneIndexContext();
        var sut = new QueryTokeniser(ctx);

        sut.Tokenise(null!).Should().BeEmpty();
        sut.Tokenise("").Should().BeEmpty();
        sut.Tokenise("   ").Should().BeEmpty();
    }

    [Fact]
    public void Tokenise_ProducesLowercaseAndAsciiFoldedTokens()
    {
        using var ctx = new LuceneIndexContext();
        var sut = new QueryTokeniser(ctx);

        var tokens = sut.Tokenise("Ésa Saint").ToList();

        tokens.Should().Contain("esa");
        tokens.Should().Contain("saint");
    }

    [Fact]
    public void Tokenise_IncludesPhoneticInjections()
    {
        using var ctx = new LuceneIndexContext();
        var sut = new QueryTokeniser(ctx);

        var tokens = sut.Tokenise("Steven").ToList();

        // DoubleMetaphone injection will add a code-like token; ensure we have more than one token
        tokens.Should().HaveCountGreaterThan(1);
        tokens.Should().Contain("steven");
    }
}
