using Moq;
using Microsoft.Extensions.Logging;
using SAPSec.Core.Constants;
using SAPSec.Core.Model;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Services;
using System.Diagnostics.CodeAnalysis;

namespace SAPSec.Core.Tests.Services;

[ExcludeFromCodeCoverage]
public class LookupServiceTests
{
    private readonly Mock<ILookupRepository> _mockRepo;
    private readonly Mock<ILogger<LookupService>> _mockLogger;
    private readonly LookupService _service;

    public LookupServiceTests()
    {
        _mockRepo = new Mock<ILookupRepository>();
        _mockLogger = new Mock<ILogger<LookupService>>();
        _service = new LookupService(_mockRepo.Object, _mockLogger.Object);
    }
    private LookupService CreateService() =>
    new LookupService(_mockRepo.Object, _mockLogger.Object);

    #region GetAllLookupsAsync Tests

    [Fact]
    public async Task GetLookupValue_ShouldReturnCorrectName_WhenLookupExists()
    {
        // Arrange
        var lookups = new List<Lookup>
        {
            new Lookup { Id = "1", Name = "Primary", LookupType = LookupTypes.PhaseOfEducation },
            new Lookup { Id = "2", Name = "Secondary", LookupType = LookupTypes.PhaseOfEducation },
            new Lookup { Id = "1", Name = "Mixed", LookupType = LookupTypes.Gender }
        };
        _mockRepo.Setup(r => r.GetAllLookupsAsync()).ReturnsAsync(lookups);
        var service = CreateService();

        // Act
        var result = await service.GetLookupValueAsync(LookupTypes.PhaseOfEducation, "1");

        // Assert
        Assert.Equal("Primary", result);
    }

    [Fact]
    public async Task GetLookupValue_ShouldReturnEmptyString_WhenLookupDoesNotExist()
    {
        // Arrange
        var lookups = new List<Lookup>
        {
            new Lookup { Id = "1", Name = "Primary", LookupType = LookupTypes.PhaseOfEducation }
        };
        _mockRepo.Setup(r => r.GetAllLookupsAsync()).ReturnsAsync(lookups);
        var service = CreateService();

        // Act
        var result = await service.GetLookupValueAsync(LookupTypes.PhaseOfEducation, "999");

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public async Task GetLookupValue_ShouldReturnEmptyString_WhenIdIsNull()
    {
        // Arrange
        var lookups = new List<Lookup>
        {
            new Lookup { Id = "1", Name = "Primary", LookupType = LookupTypes.PhaseOfEducation }
        };
        _mockRepo.Setup(r => r.GetAllLookupsAsync()).ReturnsAsync(lookups);
        var service = CreateService();

        // Act
        var result = await service.GetLookupValueAsync(LookupTypes.PhaseOfEducation, null);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public async Task GetLookupValue_ShouldReturnEmptyString_WhenIdIsWhitespace()
    {
        // Arrange
        var lookups = new List<Lookup>
        {
            new Lookup { Id = "1", Name = "Primary", LookupType = LookupTypes.PhaseOfEducation }
        };
        _mockRepo.Setup(r => r.GetAllLookupsAsync()).ReturnsAsync(lookups);
        var service = CreateService();

        // Act
        var result = await service.GetLookupValueAsync(LookupTypes.PhaseOfEducation, "   ");

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public async Task GetLookupValue_ShouldDistinguishBetweenTypes_WithSameId()
    {
        // Arrange
        var lookups = new List<Lookup>
        {
            new Lookup { Id = "1", Name = "Primary", LookupType = LookupTypes.PhaseOfEducation },
            new Lookup { Id = "1", Name = "Mixed", LookupType = LookupTypes.Gender }
        };
        _mockRepo.Setup(r => r.GetAllLookupsAsync()).ReturnsAsync(lookups);
        var service = CreateService();

        // Act
        var phaseResult = await service.GetLookupValueAsync(LookupTypes.PhaseOfEducation, "1");
        var genderResult = await service.GetLookupValueAsync(LookupTypes.Gender, "1");

        // Assert
        Assert.Equal("Primary", phaseResult);
        Assert.Equal("Mixed", genderResult);
    }

    #endregion

    #region Caching Tests

    [Fact]
    public async Task GetLookupValue_ShouldCacheResults_AndNotCallRepositoryAgain()
    {
        // Arrange
        var lookups = new List<Lookup>
        {
            new Lookup { Id = "1", Name = "Test", LookupType = "Testing" }
        };
        _mockRepo.Setup(r => r.GetAllLookupsAsync()).ReturnsAsync(lookups);
        var service = CreateService();

        // Act
        var result1 = await service.GetLookupValueAsync("Testing", "1");
        var result2 = await service.GetLookupValueAsync("Testing", "1");

        // Assert
        _mockRepo.Verify(r => r.GetAllLookupsAsync(), Times.Once);
        Assert.Equal("Test", result1);
        Assert.Equal("Test", result2);
    }

    #endregion

    #region Exception Handling Tests

    [Fact]
    public async Task GetLookupValue_ShouldThrowException_WhenRepositoryThrows()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetAllLookupsAsync())
                 .Throws(new Exception("Database error"));
        var service = CreateService();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () => await service.GetLookupValueAsync("Testing", "1"));
        Assert.Equal("Database error", ex.Message);
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task GetLookupValue_ShouldBeThreadSafe_WhenCalledConcurrently()
    {
        // Arrange
        var lookups = new List<Lookup>
        {
            new Lookup { Id = "1", Name = "Test", LookupType = "Testing" }
        };
        _mockRepo.Setup(r => r.GetAllLookupsAsync()).ReturnsAsync(lookups);
        var service = CreateService();

        // Act
        var tasks = Enumerable.Range(0, 100)
            .Select(_ => Task.Run(async () => await service.GetLookupValueAsync("Testing", "1")))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.All(results, r => Assert.Equal("Test", r));
        _mockRepo.Verify(r => r.GetAllLookupsAsync(), Times.Once);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task GetLookupValue_ShouldHandleEmptyRepository()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetAllLookupsAsync()).ReturnsAsync(new List<Lookup>());
        var service = CreateService();

        // Act
        var result = await service.GetLookupValueAsync("Testing", "1");

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public async Task GetLookupValue_ShouldSkipLookupsWithNullTypeOrId()
    {
        // Arrange
        var lookups = new List<Lookup>
        {
            new Lookup { Id = null, Name = "BadId", LookupType = "Testing" },
            new Lookup { Id = "1", Name = "BadType", LookupType = null },
            new Lookup { Id = "2", Name = "Valid", LookupType = "Testing" }
        };
        _mockRepo.Setup(r => r.GetAllLookupsAsync()).ReturnsAsync(lookups);
        var service = CreateService();

        // Act
        var result = await service.GetLookupValueAsync("Testing", "2");

        // Assert
        Assert.Equal("Valid", result);
    }
    #endregion
}