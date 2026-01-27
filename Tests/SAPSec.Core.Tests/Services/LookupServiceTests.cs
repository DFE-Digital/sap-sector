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
    private readonly LookupService  _service;

    public LookupServiceTests()
    {
        _mockRepo = new Mock<ILookupRepository>();
        _mockLogger = new Mock<ILogger<LookupService>>();
        _service = new LookupService(_mockRepo.Object, _mockLogger.Object);
    }

    #region GetAllLookups Tests

    [Fact]
    public void GetAllLookups_ShouldReturnAllItems()
    {
        // Arrange
        var expectedLookups = new List<Lookup>
        {
            new Lookup { Id = "100", Name = "Test One", LookupType = "Testing" },
            new Lookup { Id = "101", Name = "Test Two", LookupType = "Testing" }
        };
        _mockRepo.Setup(r => r.GetAllLookups()).Returns(expectedLookups);

        // Act
        var result = _service.GetAllLookups();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, a => a.Name == "Test One");
        Assert.Contains(result, a => a.Name == "Test Two");
    }

    [Fact]
    public void GetAllLookups_ShouldReturnEmpty_WhenNoData()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetAllLookups()).Returns(new List<Lookup>());

        // Act
        var result = _service.GetAllLookups();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void GetAllLookups_ShouldCacheResults_AndNotCallRepositoryAgain()
    {
        // Arrange
        var expectedLookups = new List<Lookup>
        {
            new Lookup { Id = "100", Name = "Test One", LookupType = "Testing" }
        };
        _mockRepo.Setup(r => r.GetAllLookups()).Returns(expectedLookups);

        // Act
        var result1 = _service.GetAllLookups();
        var result2 = _service.GetAllLookups();

        // Assert
        _mockRepo.Verify(r => r.GetAllLookups(), Times.Once);
        Assert.Equal(result1.Count(), result2.Count());
    }

    #endregion

    #region GetLookupValue Tests

    [Fact]
    public void GetLookupValue_ShouldReturnCorrectName_WhenLookupExists()
    {
        // Arrange
        var lookups = new List<Lookup>
        {
            new Lookup { Id = "1", Name = "Primary", LookupType = LookupTypes.PhaseOfEducation },
            new Lookup { Id = "2", Name = "Secondary", LookupType = LookupTypes.PhaseOfEducation },
            new Lookup { Id = "1", Name = "Mixed", LookupType = LookupTypes.Gender }
        };
        _mockRepo.Setup(r => r.GetAllLookups()).Returns(lookups);

        // Act
        var result = _service.GetLookupValue(LookupTypes.PhaseOfEducation, "1");

        // Assert
        Assert.Equal("Primary", result);
    }

    [Fact]
    public void GetLookupValue_ShouldReturnEmptyString_WhenLookupDoesNotExist()
    {
        // Arrange
        var lookups = new List<Lookup>
        {
            new Lookup { Id = "1", Name = "Primary", LookupType = LookupTypes.PhaseOfEducation }
        };
        _mockRepo.Setup(r => r.GetAllLookups()).Returns(lookups);

        // Act
        var result = _service.GetLookupValue(LookupTypes.PhaseOfEducation, "999");

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetLookupValue_ShouldReturnEmptyString_WhenIdIsNull()
    {
        // Arrange
        var lookups = new List<Lookup>
        {
            new Lookup { Id = "1", Name = "Primary", LookupType = LookupTypes.PhaseOfEducation }
        };
        _mockRepo.Setup(r => r.GetAllLookups()).Returns(lookups);

        // Act
        var result = _service.GetLookupValue(LookupTypes.PhaseOfEducation, null);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetLookupValue_ShouldReturnEmptyString_WhenIdIsWhitespace()
    {
        // Arrange
        var lookups = new List<Lookup>
        {
            new Lookup { Id = "1", Name = "Primary", LookupType = LookupTypes.PhaseOfEducation }
        };
        _mockRepo.Setup(r => r.GetAllLookups()).Returns(lookups);

        // Act
        var result = _service.GetLookupValue(LookupTypes.PhaseOfEducation, "   ");

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void GetLookupValue_ShouldDistinguishBetweenTypes_WithSameId()
    {
        // Arrange
        var lookups = new List<Lookup>
        {
            new Lookup { Id = "1", Name = "Primary", LookupType = LookupTypes.PhaseOfEducation },
            new Lookup { Id = "1", Name = "Mixed", LookupType = LookupTypes.Gender }
        };
        _mockRepo.Setup(r => r.GetAllLookups()).Returns(lookups);

        // Act
        var phaseResult = _service.GetLookupValue(LookupTypes.PhaseOfEducation, "1");
        var genderResult = _service.GetLookupValue(LookupTypes.Gender, "1");

        // Assert
        Assert.Equal("Primary", phaseResult);
        Assert.Equal("Mixed", genderResult);
    }

    #endregion

    #region GetLookupsByType Tests

    [Fact]
    public void GetLookupsByType_ShouldReturnAllLookupsOfType()
    {
        // Arrange
        var lookups = new List<Lookup>
        {
            new Lookup { Id = "1", Name = "Primary", LookupType = LookupTypes.PhaseOfEducation },
            new Lookup { Id = "2", Name = "Secondary", LookupType = LookupTypes.PhaseOfEducation },
            new Lookup { Id = "1", Name = "Mixed", LookupType = LookupTypes.Gender }
        };
        _mockRepo.Setup(r => r.GetAllLookups()).Returns(lookups);

        // Act
        var result = _service.GetLookupsByType(LookupTypes.PhaseOfEducation);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, l => Assert.Equal(LookupTypes.PhaseOfEducation, l.LookupType));
    }

    [Fact]
    public void GetLookupsByType_ShouldReturnEmptyList_WhenTypeDoesNotExist()
    {
        // Arrange
        var lookups = new List<Lookup>
        {
            new Lookup { Id = "1", Name = "Primary", LookupType = LookupTypes.PhaseOfEducation }
        };
        _mockRepo.Setup(r => r.GetAllLookups()).Returns(lookups);

        // Act
        var result = _service.GetLookupsByType("NonExistentType");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion

    #region ClearCache Tests

    [Fact]
    public void ClearCache_ShouldForceReloadOnNextAccess()
    {
        // Arrange
        var initialLookups = new List<Lookup>
        {
            new Lookup { Id = "1", Name = "Initial", LookupType = "Testing" }
        };
        var updatedLookups = new List<Lookup>
        {
            new Lookup { Id = "1", Name = "Updated", LookupType = "Testing" }
        };

        _mockRepo.SetupSequence(r => r.GetAllLookups())
                 .Returns(initialLookups)
                 .Returns(updatedLookups);

        // Act
        var initialResult = _service.GetLookupValue("Testing", "1");
        _service.ClearCache();
        var updatedResult = _service.GetLookupValue("Testing", "1");

        // Assert
        Assert.Equal("Initial", initialResult);
        Assert.Equal("Updated", updatedResult);
        _mockRepo.Verify(r => r.GetAllLookups(), Times.Exactly(2));
    }

    #endregion

    #region Exception Handling Tests

    [Fact]
    public void GetLookupValue_ShouldThrowException_WhenRepositoryThrows()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetAllLookups())
                 .Throws(new Exception("Database error"));

        // Act & Assert
        var ex = Assert.Throws<Exception>(() => _service.GetLookupValue("Testing", "1"));
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
        _mockRepo.Setup(r => r.GetAllLookups()).Returns(lookups);

        // Act
        var tasks = Enumerable.Range(0, 100)
            .Select(_ => Task.Run(() => _service.GetLookupValue("Testing", "1")))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.All(results, r => Assert.Equal("Test", r));
        _mockRepo.Verify(r => r.GetAllLookups(), Times.Once);
    }

    #endregion
}