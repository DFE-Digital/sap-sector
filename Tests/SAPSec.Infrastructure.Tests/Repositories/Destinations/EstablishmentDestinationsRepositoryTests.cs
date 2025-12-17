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
    public class EstablishmentDestinationsRepositoryTests
    {
        private readonly Mock<IGenericRepository<EstablishmentDestinations>> _mockGenericRepo;
        private readonly Mock<ILogger<EstablishmentDestinations>> _mockLogger;
        private readonly EstablishmentDestinationsRepository _sut;

        public EstablishmentDestinationsRepositoryTests()
        {
            _mockGenericRepo = new Mock<IGenericRepository<EstablishmentDestinations>>();
            _mockLogger = new Mock<ILogger<EstablishmentDestinations>>();
            _sut = new EstablishmentDestinationsRepository(_mockGenericRepo.Object, _mockLogger.Object);
        }

        [Fact]
        public void GetAllEstablishmentDestinationss_ReturnsAllItemsFromGenericRepository()
        {
            // Arrange
            var expected = new List<EstablishmentDestinations>
            {
                new() { Id = "1", AllDest_Tot_Est_Current_Pct= 99.99 },
                new() { Id = "2", AllDest_Tot_Est_Current_Pct = 88.88 }
            };
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns(expected);

            // Act
            var result = _sut.GetAllEstablishmentDestinations();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, e => e.Id == "1");
            Assert.Contains(result, e => e.Id == "2");
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetAllEstablishmentDestinationss_ReturnsEmptyWhenGenericRepositoryReturnsNull()
        {
            // Arrange
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns((IEnumerable<EstablishmentDestinations>?)null);

            // Act
            var result = _sut.GetAllEstablishmentDestinations();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetEstablishmentDestinations_ReturnsCorrectItemWhenUrnExists()
        {
            // Arrange
            var expected = new EstablishmentDestinations { Id = "1", AllDest_Tot_Est_Current_Pct = 99.99 };
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns(new[] { expected });

            // Act
            var result = _sut.GetEstablishmentDestinations("1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("1", result.Id);
            Assert.Equal(99.99, result.AllDest_Tot_Est_Current_Pct);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetEstablishmentDestinations_ReturnsNewEstablishmentDestinationsWhenUrnDoesNotExist()
        {
            // Arrange
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns(Enumerable.Empty<EstablishmentDestinations>());

            // Act
            var result = _sut.GetEstablishmentDestinations("999");

            // Assert
            Assert.NotNull(result);
            // When not found the repository returns a new EstablishmentDestinations (defaults are empty strings / zeros)
            Assert.Equal(string.Empty, result.Id);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }
    }
}
