using FluentAssertions;
using Moq;
using SAPSec.Data.Repositories;
using SAPSec.Infrastructure.PostgresSearch;
using Xunit;

namespace SAPSec.Infrastructure.Tests.PostgresSearch;

public class PostgresSchoolSearchIndexReaderTests
{
    private readonly Mock<IEstablishmentRepository> _repo = new();

    private PostgresSchoolSearchIndexReader Reader(params EstablishmentSearchHit[] hits)
    {
        _repo.Setup(r => r.SearchEstablishmentsAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(hits);

        return new PostgresSchoolSearchIndexReader(_repo.Object);
    }

    [Fact]
    public async Task BlankQuery_ReturnsEmptyWithoutQueryingRepository()
    {
        var results = await Reader().SearchAsync("   ");

        results.Should().BeEmpty();
        _repo.Verify(r => r.SearchEstablishmentsAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Highlights_MatchedWordsAndPrefixOfLastToken()
    {
        var results = await Reader(new EstablishmentSearchHit("123", "Fake School Three")).SearchAsync("fa thr");

        results.Should().ContainSingle().Which.Should().Be((123, "Fake School *Three*"));
    }

    [Theory]
    [InlineData("school", 1000, 100)]
    [InlineData("The Academy", 1000, 100)]
    [InlineData("school", 10, 10)]
    [InlineData("sch", 1000, 1000)]
    [InlineData("st marys school", 1000, 1000)]
    public async Task BroadQuery_IsCappedButPartialsAndSpecificQueriesAreNot(string query, int requested, int expectedLimit)
    {
        await Reader().SearchAsync(query, requested);

        _repo.Verify(r => r.SearchEstablishmentsAsync(It.IsAny<string>(), expectedLimit), Times.Once);
    }

    [Fact]
    public async Task Highlights_ExactEarlierToken()
    {
        var results = await Reader(new EstablishmentSearchHit("123", "Fake School Three")).SearchAsync("school fa");

        results.Single().resultText.Should().Be("*Fake* *School* Three");
    }
}
