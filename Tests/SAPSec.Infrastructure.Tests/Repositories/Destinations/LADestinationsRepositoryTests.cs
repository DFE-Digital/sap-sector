using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Model.KS4.Destinations;
using SAPSec.Core.Interfaces.Repositories.Generic;
using SAPSec.Infrastructure.Repositories.KS4.Destinations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Infrastructure.Tests.Repositories.Destinations
{
    public class LADestinationsRepositoryTests
    {
        private readonly Mock<IGenericRepository<LADestinations>> _mockGenericRepo;
        private readonly Mock<ILogger<LADestinations>> _mockLogger;
        private readonly LADestinationsRepository _sut;

        public LADestinationsRepositoryTests()
        {
            _mockGenericRepo = new Mock<IGenericRepository<LADestinations>>();
            _mockLogger = new Mock<ILogger<LADestinations>>();
            _sut = new LADestinationsRepository(_mockGenericRepo.Object, _mockLogger.Object);
        }

        [Fact]
        public void GetAllLADestinationss_ReturnsAllItemsFromGenericRepository()
        {
            // Arrange
            var expected = new List<LADestinations>
            {
                new() { Id = "1", AllDest_Tot_LA_Current_Pct= 99.99 },
                new() { Id = "2", AllDest_Tot_LA_Current_Pct = 88.88 }
            };
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns(expected);

            // Act
            var result = _sut.GetAllLADestinations();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, e => e.Id == "1");
            Assert.Contains(result, e => e.Id == "2");
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetAllLADestinationss_ReturnsEmptyWhenGenericRepositoryReturnsNull()
        {
            // Arrange
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns((IEnumerable<LADestinations>?)null);

            // Act
            var result = _sut.GetAllLADestinations();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetLADestinations_ReturnsCorrectItemWhenUrnExists()
        {
            // Arrange
            var expected = new LADestinations { Id = "1", AllDest_Tot_LA_Current_Pct = 99.99 };
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns(new[] { expected });

            // Act
            var result = _sut.GetLADestinations("1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("1", result.Id);
            Assert.Equal(99.99, result.AllDest_Tot_LA_Current_Pct);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetLADestinations_ReturnsNewLADestinationsWhenUrnDoesNotExist()
        {
            // Arrange
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns(Enumerable.Empty<LADestinations>());

            // Act
            var result = _sut.GetLADestinations("999");

            // Assert
            Assert.NotNull(result);
            // When not found the repository returns a new LADestinations (defaults are empty strings / zeros)
            Assert.Equal(string.Empty, result.Id);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }
    }
}
