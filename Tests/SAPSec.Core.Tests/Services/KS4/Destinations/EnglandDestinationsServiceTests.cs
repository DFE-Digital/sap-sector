using Moq;
using SAPSec.Core.Model.KS4.Absence;
using SAPSec.Core.Model.KS4.Destinations;
using SAPSec.Core.Interfaces.Repositories.KS4.Absence;
using SAPSec.Core.Interfaces.Repositories.KS4.Destinations;
using SAPSec.Core.Services.KS4.Absence;
using SAPSec.Core.Services.KS4.Destinations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Tests.Services.KS4.Destinations
{
    public class EnglandDestinationsServiceTests
    {
        private readonly Mock<IEnglandDestinationsRepository> _mockRepo;
        private readonly EnglandDestinationsService _service;

        public EnglandDestinationsServiceTests()
        {
            _mockRepo = new Mock<IEnglandDestinationsRepository>();
            _service = new EnglandDestinationsService(_mockRepo.Object);
        }

        [Fact]
        public void GetAllEnglandDestinations_ShouldReturnAllItems()
        {
            // Arrange
            var expectedAbsences =
            new EnglandDestinations { AllDest_Tot_Eng_Current_Pct = 99.99 };

            _mockRepo.Setup(r => r.GetEnglandDestinations())
                         .Returns(expectedAbsences);

            // Act
            var result = _service.GetEnglandDestinations();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(99.99, result.AllDest_Tot_Eng_Current_Pct);
        }

        [Fact]
        public void GetAllEnglandDestinations_ShouldReturnEmpty_WhenNoData()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetEnglandDestinations())
                     .Returns(new EnglandDestinations());

            // Act
            var result = _service.GetEnglandDestinations();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetEnglandDestinations_ShouldThrowException_WhenRepositoryThrows()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetEnglandDestinations())
                     .Throws(new Exception("Database error"));

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _service.GetEnglandDestinations());
            Assert.Equal("Database error", ex.Message);
        }

    }
}