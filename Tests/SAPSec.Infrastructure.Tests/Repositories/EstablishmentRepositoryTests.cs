using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Model;
using SAPSec.Infrastructure.Repositories.Json;

namespace SAPSec.Infrastructure.Tests.Repositories
{
    public class EstablishmentRepositoryTests
    {
        private readonly Mock<IJsonFile<Establishment>> _mockGenericRepo;
        private readonly Mock<ILogger<Establishment>> _mockLogger;
        private readonly JsonEstablishmentRepository _sut;

        public EstablishmentRepositoryTests()
        {
            _mockGenericRepo = new Mock<IJsonFile<Establishment>>();
            _mockLogger = new Mock<ILogger<Establishment>>();
            _sut = new JsonEstablishmentRepository(_mockGenericRepo.Object, _mockLogger.Object);
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
            _mockGenericRepo.Setup(r => r.ReadAllAsync()).ReturnsAsync(expected);

            // Act
            var result = await _sut.GetAllEstablishmentsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, e => e.URN == "1");
            Assert.Contains(result, e => e.URN == "2");
            _mockGenericRepo.Verify(r => r.ReadAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllEstablishments_ReturnsEmptyWhenGenericRepositoryReturnsEmpty()
        {
            // Arrange
            _mockGenericRepo.Setup(r => r.ReadAllAsync()).ReturnsAsync([]);

            // Act
            var result = await _sut.GetAllEstablishmentsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockGenericRepo.Verify(r => r.ReadAllAsync(), Times.Once);
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
            _mockGenericRepo.Setup(r => r.ReadAllAsync()).ReturnsAsync(expected);

            // Act
            IEnumerable<string> urns = ["1"];
            var result = await _sut.GetEstablishmentsAsync(urns);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Count());
            Assert.Contains(result, e => e.URN == "1");
            _mockGenericRepo.Verify(r => r.ReadAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetEstablishments_ReturnsEmptyWhenGenericRepositoryIsEmpty()
        {
            // Arrange
            _mockGenericRepo.Setup(r => r.ReadAllAsync()).ReturnsAsync([]);

            // Act
            IEnumerable<string> urns = ["1"];
            var result = await _sut.GetEstablishmentsAsync(urns);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockGenericRepo.Verify(r => r.ReadAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetEstablishment_ReturnsCorrectItemWhenUrnExists()
        {
            // Arrange
            var expected = new Establishment { URN = "123", EstablishmentName = "Found" };
            _mockGenericRepo.Setup(r => r.ReadAllAsync()).ReturnsAsync(new[] { expected });

            // Act
            var result = await _sut.GetEstablishmentAsync("123");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("123", result.URN);
            Assert.Equal("Found", result.EstablishmentName);
            _mockGenericRepo.Verify(r => r.ReadAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetEstablishment_ReturnsNewEstablishmentWhenUrnDoesNotExist()
        {
            // Arrange
            _mockGenericRepo.Setup(r => r.ReadAllAsync()).ReturnsAsync(Enumerable.Empty<Establishment>());

            // Act
            var result = await _sut.GetEstablishmentAsync("999");

            // Assert
            Assert.NotNull(result);
            // When not found the repository returns a new Establishment (defaults are empty strings / zeros)
            Assert.Equal(string.Empty, result.URN);
            _mockGenericRepo.Verify(r => r.ReadAllAsync(), Times.Once);
        }
    }
}