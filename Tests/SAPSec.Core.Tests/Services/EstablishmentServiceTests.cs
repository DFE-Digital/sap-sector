using Moq;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Core.Services;
using System.Diagnostics.CodeAnalysis;

namespace SAPSec.Core.Tests.Services
{
    [ExcludeFromCodeCoverage]
    public class EstablishmentServiceTests
    {
        private readonly Mock<IEstablishmentRepository> _mockRepo;
        private readonly Mock<ILookupService> _mockLookupRepo;
        private readonly EstablishmentService _service;

        public EstablishmentServiceTests()
        {
            _mockRepo = new Mock<IEstablishmentRepository>();
            _mockLookupRepo = new Mock<ILookupService>();
            _service = new EstablishmentService(_mockRepo.Object, _mockLookupRepo.Object);
        }

        private readonly Establishment FakeEstablishmentOne = new Establishment
        {
            URN = "123456",
            EstablishmentName = "Test Establishment One",
            PhaseOfEducationName = "Primary School"
        };
        private readonly Establishment FakeEstablishmentTwo = new Establishment
        {
            URN = "456789",
            EstablishmentName = "Test Establishment Two",
            PhaseOfEducationName = "Secondary School"
        };

        [Fact]
        public async Task GetAllEstablishments_ShouldReturnAllItems()
        {
            // Arrange
            var expectedDestinations = new List<Establishment>
            {
                FakeEstablishmentOne,
                FakeEstablishmentTwo
            };

            _mockRepo.Setup(r => r.GetAllEstablishmentsAsync())
                     .ReturnsAsync(expectedDestinations);

            // Act
            var result = await _service.GetAllEstablishmentsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, a => a.URN == "123456");
            Assert.Contains(result, a => a.URN == "456789");
        }

        [Fact]
        public async Task GetAllEstablishments_ShouldReturnEmpty_WhenNoData()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetAllEstablishmentsAsync())
                     .ReturnsAsync(new List<Establishment>());

            // Act
            var result = await _service.GetAllEstablishmentsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetEstablishments_ShouldReturnSubset()
        {
            // Arrange
            var expectedDestinations = new List<Establishment>
            {
                FakeEstablishmentOne,
                FakeEstablishmentTwo
            };

            IEnumerable<string> urns = ["123456"];
            _mockRepo.Setup(r => r.GetEstablishmentsAsync(urns))
                     .ReturnsAsync(expectedDestinations);

            // Act
            var result = await _service.GetEstablishmentsAsync(urns);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Count());
            Assert.Contains(result, a => a.URN == "123456");
        }

        [Fact]
        public async Task GetEstablishment_ShouldReturnEmpty_WhenNoData()
        {
            // Arrange
            IEnumerable<string> urns = ["123456"];
            _mockRepo.Setup(r => r.GetEstablishmentsAsync(urns))
                     .ReturnsAsync(new List<Establishment>());

            // Act
            var result = await _service.GetEstablishmentsAsync(urns);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetEstablishment_ShouldReturnCorrectItem_WhenUrnExists()
        {
            // Arrange
            var urn = "123456";
            var expectedDestinations = FakeEstablishmentOne;

            _mockRepo.Setup(r => r.GetEstablishmentAsync(urn))
                     .ReturnsAsync(expectedDestinations);

            // Act
            var result = await _service.GetEstablishmentAsync(urn);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(urn, result.URN);
            Assert.Equal(FakeEstablishmentOne.EstablishmentName, result.EstablishmentName);
            Assert.Equal(FakeEstablishmentOne.PhaseOfEducationName, result.PhaseOfEducationName);
        }

        [Fact]
        public async Task GetEstablishment_ShouldThrowError_WhenUrnDoesNotExist()
        {
            // Arrange
            var urn = "99999";
            _mockRepo.Setup(r => r.GetEstablishmentAsync(urn))
                     .Throws(new Exception("Error in GetEstablishmentAsync"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(async () => await _service.GetEstablishmentAsync(urn));
            Assert.Equal("Error in GetEstablishmentAsync", ex.Message);
        }

        [Fact]
        public async Task GetEstablishment_ShouldThrowException_WhenRepositoryThrows()
        {
            // Arrange
            var urn = "error";
            _mockRepo.Setup(r => r.GetEstablishmentAsync(urn))
                     .Throws(new Exception("Database error"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(async () => await _service.GetEstablishmentAsync(urn));
            Assert.Equal("Database error", ex.Message);
        }
    }
}
