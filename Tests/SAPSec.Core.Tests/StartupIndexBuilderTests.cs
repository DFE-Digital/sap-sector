using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Services.Lucene;
using SAPSec.Infrastructure.Entities;
using SAPSec.Infrastructure.Interfaces;

namespace SAPSec.Core.Tests;

public class StartupIndexBuilderTests
{
    [Fact]
    public async Task StartAsync_BuildsIndex_And_Completes()
    {
        // Arrange
        var logger = new Mock<ILogger<StartupIndexBuilder>>();
        var repo = new DummyRepo();
        using var ctx = new LuceneIndexContext();
        var writer = new LuceneIndexWriter(ctx);
        var sut = new StartupIndexBuilder(logger.Object, writer, repo);

        // Act
        var act = async () => await sut.StartAsync(CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        await sut.StopAsync(CancellationToken.None);
    }

    private sealed class DummyRepo : ISchoolRepository
    {
        public IList<School> GetAll() => [];

        public School GetSchoolByUrn(int schoolNumber)
        {
            return null!;
        }

        public School GetSchoolByNumber(int schoolNumber)
        {
            return null!;
        }
    }
}
