using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Model.Generated;
using SAPSec.Infrastructure.Json;

namespace SAPSec.Infrastructure.Tests.Repositories;

public class EstablishmentRepositoryTests
{
    private readonly Mock<IJsonFile<Establishment>> _mockEstablishmentJsonFile;
    private readonly Mock<IJsonFile<EstablishmentEmail>> _mockEstablishmentEmailJsonFile;
    private readonly Mock<ILogger<JsonEstablishmentRepository>> _mockLogger;
    private readonly JsonEstablishmentRepository _sut;

    public EstablishmentRepositoryTests()
    {
        _mockEstablishmentJsonFile = new Mock<IJsonFile<Establishment>>();
        _mockEstablishmentEmailJsonFile = new Mock<IJsonFile<EstablishmentEmail>>();
        _mockLogger = new Mock<ILogger<JsonEstablishmentRepository>>();
        _sut = new JsonEstablishmentRepository(_mockEstablishmentJsonFile.Object, _mockEstablishmentEmailJsonFile.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllEstablishments_ReturnsAllItemsFromGenericRepository()
    {
        // Arrange
        var expected = new List<Establishment>
        {
            new Establishment { URN = "1", EstablishmentName = "One" },
            new Establishment { URN = "2", EstablishmentName = "Two" }
        };
        _mockEstablishmentJsonFile.Setup(r => r.ReadAllAsync()).ReturnsAsync(expected);

        // Act
        var result = await _sut.GetAllEstablishmentsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, e => e.URN == "1");
        Assert.Contains(result, e => e.URN == "2");
        _mockEstablishmentJsonFile.Verify(r => r.ReadAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllEstablishments_ReturnsEmptyWhenGenericRepositoryReturnsEmpty()
    {
        // Arrange
        _mockEstablishmentJsonFile.Setup(r => r.ReadAllAsync()).ReturnsAsync([]);

        // Act
        var result = await _sut.GetAllEstablishmentsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockEstablishmentJsonFile.Verify(r => r.ReadAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetEstablishments_ReturnsSubsetOfItemsFromGenericRepository()
    {
        // Arrange
        var expected = new List<Establishment>
        {
            new Establishment { URN = "1", EstablishmentName = "One" },
            new Establishment { URN = "2", EstablishmentName = "Two" }
        };
        _mockEstablishmentJsonFile.Setup(r => r.ReadAllAsync()).ReturnsAsync(expected);

        // Act
        IEnumerable<string> urns = ["1"];
        var result = await _sut.GetEstablishmentsAsync(urns);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Count());
        Assert.Contains(result, e => e.URN == "1");
        _mockEstablishmentJsonFile.Verify(r => r.ReadAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetEstablishments_ReturnsEmptyWhenGenericRepositoryIsEmpty()
    {
        // Arrange
        _mockEstablishmentJsonFile.Setup(r => r.ReadAllAsync()).ReturnsAsync([]);

        // Act
        IEnumerable<string> urns = ["1"];
        var result = await _sut.GetEstablishmentsAsync(urns);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockEstablishmentJsonFile.Verify(r => r.ReadAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetEstablishment_ReturnsCorrectItemWhenUrnExists()
    {
        // Arrange
        var expected = new Establishment { URN = "123", EstablishmentName = "Found" };
        _mockEstablishmentJsonFile.Setup(r => r.ReadAllAsync()).ReturnsAsync(new[] { expected });

        // Act
        var result = await _sut.GetEstablishmentAsync("123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("123", result.URN);
        Assert.Equal("Found", result.EstablishmentName);
        _mockEstablishmentJsonFile.Verify(r => r.ReadAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetEstablishment_ReturnsNullWhenUrnDoesNotExist()
    {
        // Arrange
        _mockEstablishmentJsonFile.Setup(r => r.ReadAllAsync()).ReturnsAsync(Enumerable.Empty<Establishment>());

        // Act
        var result = await _sut.GetEstablishmentAsync("999");

        // Assert
        Assert.Null(result);
        _mockEstablishmentJsonFile.Verify(r => r.ReadAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetEstablishmentEmail_ReturnsCorrectItemWhenUrnExists()
    {
        // Arrange
        var expected = new EstablishmentEmail { URN = "123", MainEmail = "establishment@email.com" };
        _mockEstablishmentEmailJsonFile.Setup(r => r.ReadAllAsync()).ReturnsAsync(new[] { expected });

        // Act
        var result = await _sut.GetEstablishmentEmailAsync("123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("123", result.URN);
        Assert.Equal("establishment@email.com", result.MainEmail);
        _mockEstablishmentEmailJsonFile.Verify(r => r.ReadAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetEstablishmentEmail_ReturnsNullWhenUrnDoesNotExist()
    {
        // Arrange
        _mockEstablishmentEmailJsonFile.Setup(r => r.ReadAllAsync()).ReturnsAsync(Enumerable.Empty<EstablishmentEmail>());

        // Act
        var result = await _sut.GetEstablishmentEmailAsync("999");

        // Assert
        Assert.Null(result);
        _mockEstablishmentEmailJsonFile.Verify(r => r.ReadAllAsync(), Times.Once);
    }
}