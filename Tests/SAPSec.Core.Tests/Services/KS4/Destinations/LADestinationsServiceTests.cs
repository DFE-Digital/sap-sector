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
    public class LADestinationsServiceTests
    {
        private readonly Mock<ILADestinationsRepository> _mockRepo;
        private readonly LADestinationsService _service;

        public LADestinationsServiceTests()
        {
            _mockRepo = new Mock<ILADestinationsRepository>();
            _service = new LADestinationsService(_mockRepo.Object);
        }

        [Fact]
        public void GetAllLADestinations_ShouldReturnAllItems()
        {
            // Arrange
            var expectedDestinationss = new List<LADestinations>
        {
            new LADestinations { Id = "100",  AllDest_Tot_LA_Current_Pct = 99.99},
            new LADestinations { Id = "101", AllDest_Tot_LA_Current_Pct = 90.00}
        };

            _mockRepo.Setup(r => r.GetAllLADestinations())
                     .Returns(expectedDestinationss);

            // Act
            var result = _service.GetAllLADestinations();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, a => a.AllDest_Tot_LA_Current_Pct == 99.99);
            Assert.Contains(result, a => a.AllDest_Tot_LA_Current_Pct == 90.00);
        }

        [Fact]
        public void GetAllLADestinations_ShouldReturnEmpty_WhenNoData()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetAllLADestinations())
                     .Returns(new List<LADestinations>());

            // Act
            var result = _service.GetAllLADestinations();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }


        [Fact]
        public void GetLADestinations_ShouldReturnCorrectItem_WhenUrnExists()
        {
            // Arrange
            var urn = "100";
            var expectedDestinations = new LADestinations { Id = urn, AllDest_Tot_LA_Current_Pct = 100 };

            _mockRepo.Setup(r => r.GetLADestinations(urn))
                     .Returns(expectedDestinations);

            // Act
            var result = _service.GetLADestinations(urn);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(urn, result.Id);
            Assert.Equal(100, result.AllDest_Tot_LA_Current_Pct);
        }

        [Fact]
        public void GetLADestinations_ShouldReturnNull_WhenUrnDoesNotExist()
        {
            // Arrange
            var urn = "99999";
            _mockRepo.Setup(r => r.GetLADestinations(urn))
                     .Returns(new LADestinations());

            // Act
            var result = _service.GetLADestinations(urn);

            // Assert
            Assert.Null(result.AllDest_Tot_LA_Previous2_Pct);
        }

        [Fact]
        public void GetLADestinations_ShouldThrowException_WhenRepositoryThrows()
        {
            // Arrange
            var urn = "error";
            _mockRepo.Setup(r => r.GetLADestinations(urn))
                     .Throws(new Exception("Database error"));

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _service.GetLADestinations(urn));
            Assert.Equal("Database error", ex.Message);
        }
    }
}
