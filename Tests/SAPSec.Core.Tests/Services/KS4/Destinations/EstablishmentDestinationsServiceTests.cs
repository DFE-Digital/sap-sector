using Moq;
using SAPSec.Core.Model.KS4.Destinations;
using SAPSec.Core.Interfaces.Repositories.KS4.Destinations;
using SAPSec.Core.Services.KS4.Destinations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Tests.Services.KS4.Destinations
{
    public class EstablishmentDestinationsServiceTests
    {
        private readonly Mock<IEstablishmentDestinationsRepository> _mockRepo;
        private readonly EstablishmentDestinationsService _service;

        public EstablishmentDestinationsServiceTests()
        {
            _mockRepo = new Mock<IEstablishmentDestinationsRepository>();
            _service = new EstablishmentDestinationsService(_mockRepo.Object);
        }

        [Fact]
        public void GetAllEstablishmentDestinations_ShouldReturnAllItems()
        {
            // Arrange
            var expectedDestinationss = new List<EstablishmentDestinations>
        {
            new EstablishmentDestinations { Id = "100",  AllDest_Tot_Est_Current_Pct = 99.99},
            new EstablishmentDestinations { Id = "101", AllDest_Tot_Est_Current_Pct = 90.00}
        };

            _mockRepo.Setup(r => r.GetAllEstablishmentDestinations())
                     .Returns(expectedDestinationss);

            // Act
            var result = _service.GetAllEstablishmentDestinations();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, a => a.AllDest_Tot_Est_Current_Pct == 99.99);
            Assert.Contains(result, a => a.AllDest_Tot_Est_Current_Pct == 90.00);
        }

        [Fact]
        public void GetAllEstablishmentDestinations_ShouldReturnEmpty_WhenNoData()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetAllEstablishmentDestinations())
                     .Returns(new List<EstablishmentDestinations>());

            // Act
            var result = _service.GetAllEstablishmentDestinations();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }


        [Fact]
        public void GetEstablishmentDestinations_ShouldReturnCorrectItem_WhenUrnExists()
        {
            // Arrange
            var urn = "100";
            var expectedDestinations = new EstablishmentDestinations { Id = urn, AllDest_Tot_Est_Current_Pct = 100 };

            _mockRepo.Setup(r => r.GetEstablishmentDestinations(urn))
                     .Returns(expectedDestinations);

            // Act
            var result = _service.GetEstablishmentDestinations(urn);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(urn, result.Id);
            Assert.Equal(100, result.AllDest_Tot_Est_Current_Pct);
        }

        [Fact]
        public void GetEstablishmentDestinations_ShouldReturnNull_WhenUrnDoesNotExist()
        {
            // Arrange
            var urn = "99999";
            _mockRepo.Setup(r => r.GetEstablishmentDestinations(urn))
                     .Returns(new EstablishmentDestinations());

            // Act
            var result = _service.GetEstablishmentDestinations(urn);

            // Assert
            Assert.Null(result.AllDest_Tot_Est_Previous2_Pct);
        }

        [Fact]
        public void GetEstablishmentDestinations_ShouldThrowException_WhenRepositoryThrows()
        {
            // Arrange
            var urn = "error";
            _mockRepo.Setup(r => r.GetEstablishmentDestinations(urn))
                     .Throws(new Exception("Database error"));

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _service.GetEstablishmentDestinations(urn));
            Assert.Equal("Database error", ex.Message);
        }
    }
}
