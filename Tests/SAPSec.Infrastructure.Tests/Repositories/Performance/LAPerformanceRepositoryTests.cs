using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Interfaces.Repositories.Generic;
using SAPSec.Core.Model.KS4.Performance;
using SAPSec.Infrastructure.Repositories.KS4.Performance;

namespace SAPSec.Infrastructure.Tests.Repositories.Performance
{
    public class LAPerformanceRepositoryTests
    {
        private readonly Mock<IGenericRepository<LAPerformance>> _mockGenericRepo;
        private readonly Mock<ILogger<LAPerformance>> _mockLogger;
        private readonly LAPerformanceRepository _sut;

        public LAPerformanceRepositoryTests()
        {
            _mockGenericRepo = new Mock<IGenericRepository<LAPerformance>>();
            _mockLogger = new Mock<ILogger<LAPerformance>>();
            _sut = new LAPerformanceRepository(_mockGenericRepo.Object, _mockLogger.Object);
        }

        [Fact]
        public void GetAllLAPerformances_ReturnsAllItemsFromGenericRepository()
        {
            // Arrange
            var expected = new List<LAPerformance>
            {
                new() { Id = "1", Attainment8_Tot_LA_Current_Num= 99.99 },
                new() { Id = "2", Attainment8_Tot_LA_Current_Num = 88.88 }
            };
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns(expected);

            // Act
            var result = _sut.GetAllLAPerformance();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, e => e.Id == "1");
            Assert.Contains(result, e => e.Id == "2");
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetAllLAPerformances_ReturnsEmptyWhenGenericRepositoryReturnsNull()
        {
            // Arrange
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns((IEnumerable<LAPerformance>?)null);

            // Act
            var result = _sut.GetAllLAPerformance();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetLAPerformance_ReturnsCorrectItemWhenUrnExists()
        {
            // Arrange
            var expected = new LAPerformance { Id = "1", Attainment8_Tot_LA_Current_Num = 99.99 };
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns(new[] { expected });

            // Act
            var result = _sut.GetLAPerformance("1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("1", result.Id);
            Assert.Equal(99.99, result.Attainment8_Tot_LA_Current_Num);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetLAPerformance_ReturnsNewLAPerformanceWhenUrnDoesNotExist()
        {
            // Arrange
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns(Enumerable.Empty<LAPerformance>());

            // Act
            var result = _sut.GetLAPerformance("999");

            // Assert
            Assert.NotNull(result);
            // When not found the repository returns a new LAPerformance (defaults are empty strings / zeros)
            Assert.Equal(string.Empty, result.Id);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }
    }
}
