using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Infrastructure.LuceneSearch;

namespace SAPSec.Infrastructure.Tests.LuceneSearch;

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

    private sealed class DummyRepo : IEstablishmentService
    {
        public Task<IReadOnlyCollection<Establishment>> GetAllEstablishmentsAsync()
        {
            return Task.FromResult<IReadOnlyCollection<Establishment>>([]);
        }

        public Task<IReadOnlyCollection<Establishment>> GetEstablishmentsAsync(IEnumerable<string> urns)
        {
            return Task.FromResult<IReadOnlyCollection<Establishment>>([]);
        }

        public Task<Establishment> GetEstablishmentAsync(string urn)
        {
            return Task.FromResult<Establishment>(null!);
        }

        public Task<Establishment> GetEstablishmentByAnyNumberAsync(string number)
        {
            return Task.FromResult<Establishment>(null!);
        }
    }
}
