using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Model;
using SAPSec.Infrastructure.Repositories.Json;

namespace SAPSec.Infrastructure.Tests.Repositories
{
    public class LookupRepositoryTests
    {
        private readonly Mock<IJsonFile<Lookup>> _mockGenericRepo;
        private readonly Mock<ILogger<Lookup>> _mockLogger;
        private readonly LookupRepository _sut;

        public LookupRepositoryTests()
        {
            _mockGenericRepo = new Mock<IJsonFile<Lookup>>();
            _mockLogger = new Mock<ILogger<Lookup>>();
            _sut = new LookupRepository(_mockGenericRepo.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAllLookups_ReturnsAllItemsFromGenericRepository()
        {
            // Arrange
            var expected = new List<Lookup>
            {
                new Lookup { Id = "1", Name= "One", LookupType = "Lookup" },
                new Lookup { Id = "2", Name = "Two", LookupType = "Type" }
            };
            _mockGenericRepo.Setup(r => r.ReadAllAsync()).ReturnsAsync(expected);

            // Act
            var result = await _sut.GetAllLookupsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, e => e.Id == "1");
            Assert.Contains(result, e => e.Id == "2");
            _mockGenericRepo.Verify(r => r.ReadAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllLookups_ReturnsEmptyWhenGenericRepositoryReturnsNull()
        {
            // Arrange
            _mockGenericRepo.Setup(r => r.ReadAllAsync()).ReturnsAsync((IEnumerable<Lookup>?)null);

            // Act
            var result = await _sut.GetAllLookupsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockGenericRepo.Verify(r => r.ReadAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetLookup_ReturnsCorrectItemWhenUrnExists()
        {
            // Arrange
            var expected = new Lookup { Id = "1", Name = "One", LookupType = "Lookup" };
            _mockGenericRepo.Setup(r => r.ReadAllAsync()).ReturnsAsync(new[] { expected });

            // Act
            var result = await _sut.GetLookupAsync("1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("1", result.Id);
            Assert.Equal("One", result.Name);
            _mockGenericRepo.Verify(r => r.ReadAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetLookup_ReturnsNewLookupWhenUrnDoesNotExist()
        {
            // Arrange
            _mockGenericRepo.Setup(r => r.ReadAllAsync()).ReturnsAsync(Enumerable.Empty<Lookup>());

            // Act
            var result = await _sut.GetLookupAsync("999");

            // Assert
            Assert.NotNull(result);
            // When not found the repository returns a new Lookup (defaults are empty strings / zeros)
            Assert.Equal(string.Empty, result.Id);
            _mockGenericRepo.Verify(r => r.ReadAllAsync(), Times.Once);
        }
    }
}
