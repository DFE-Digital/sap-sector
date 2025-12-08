using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Interfaces.Repositories.Generic;
using SAPSec.Core.Model.KS4.Suspensions;
using SAPSec.Infrastructure.Repositories.KS4.Suspensions;

namespace SAPSec.Infrastructure.Tests.Repositories.Suspensions
{
    public class LASuspensionsRepositoryTests
    {
        private readonly Mock<IGenericRepository<LASuspensions>> _mockGenericRepo;
        private readonly Mock<ILogger<LASuspensions>> _mockLogger;
        private readonly LASuspensionsRepository _sut;

        public LASuspensionsRepositoryTests()
        {
            _mockGenericRepo = new Mock<IGenericRepository<LASuspensions>>();
            _mockLogger = new Mock<ILogger<LASuspensions>>();
            _sut = new LASuspensionsRepository(_mockGenericRepo.Object, _mockLogger.Object);
        }

        [Fact]
        public void GetAllLASuspensionss_ReturnsAllItemsFromGenericRepository()
        {
            // Arrange
            var expected = new List<LASuspensions>
            {
                new() { Id = "1", Sus_Tot_LA_Current_Pct= 99.99 },
                new() { Id = "2", Sus_Tot_LA_Current_Pct = 88.88 }
            };
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns(expected);

            // Act
            var result = _sut.GetAllLASuspensions();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, e => e.Id == "1");
            Assert.Contains(result, e => e.Id == "2");
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetAllLASuspensionss_ReturnsEmptyWhenGenericRepositoryReturnsNull()
        {
            // Arrange
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns((IEnumerable<LASuspensions>?)null);

            // Act
            var result = _sut.GetAllLASuspensions();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetLASuspensions_ReturnsCorrectItemWhenUrnExists()
        {
            // Arrange
            var expected = new LASuspensions { Id = "1", Sus_Tot_LA_Current_Pct = 99.99 };
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns(new[] { expected });

            // Act
            var result = _sut.GetLASuspensions("1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("1", result.Id);
            Assert.Equal(99.99, result.Sus_Tot_LA_Current_Pct);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetLASuspensions_ReturnsNewLASuspensionsWhenUrnDoesNotExist()
        {
            // Arrange
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns(Enumerable.Empty<LASuspensions>());

            // Act
            var result = _sut.GetLASuspensions("999");

            // Assert
            Assert.NotNull(result);
            // When not found the repository returns a new LASuspensions (defaults are empty strings / zeros)
            Assert.Equal(string.Empty, result.Id);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }
    }
}
