using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Model;
using SAPSec.Core.Interfaces.Repositories.Generic;
using SAPSec.Infrastructure.Repositories;

namespace SAPSec.Infrastructure.Tests.Repositories
{
    public class EstablishmentRepositoryTests
    {
        private readonly Mock<IGenericRepository<Establishment>> _mockGenericRepo;
        private readonly Mock<ILogger<Establishment>> _mockLogger;
        private readonly EstablishmentRepository _sut;

        public EstablishmentRepositoryTests()
        {
            _mockGenericRepo = new Mock<IGenericRepository<Establishment>>();
            _mockLogger = new Mock<ILogger<Establishment>>();
            _sut = new EstablishmentRepository(_mockGenericRepo.Object, _mockLogger.Object);
        }

        [Fact]
        public void GetAllEstablishments_ReturnsAllItemsFromGenericRepository()
        {
            // Arrange
            var expected = new List<Establishment>
            {
                new Establishment { URN = "1", EstablishmentName = "One" },
                new Establishment { URN = "2", EstablishmentName = "Two" }
            };
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns(expected);

            // Act
            var result = _sut.GetAllEstablishments();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, e => e.URN == "1");
            Assert.Contains(result, e => e.URN == "2");
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetAllEstablishments_ReturnsEmptyWhenGenericRepositoryReturnsNull()
        {
            // Arrange
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns((IEnumerable<Establishment>?)null);

            // Act
            var result = _sut.GetAllEstablishments();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetEstablishment_ReturnsCorrectItemWhenUrnExists()
        {
            // Arrange
            var expected = new Establishment { URN = "123", EstablishmentName = "Found" };
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns(new[] { expected });

            // Act
            var result = _sut.GetEstablishment("123");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("123", result.URN);
            Assert.Equal("Found", result.EstablishmentName);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetEstablishment_ReturnsNewEstablishmentWhenUrnDoesNotExist()
        {
            // Arrange
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns(Enumerable.Empty<Establishment>());

            // Act
            var result = _sut.GetEstablishment("999");

            // Assert
            Assert.NotNull(result);
            // When not found the repository returns a new Establishment (defaults are empty strings / zeros)
            Assert.Equal(string.Empty, result.URN);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }
    }
}