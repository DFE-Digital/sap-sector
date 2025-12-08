using Moq;
using SAPSec.Core.Model.KS4.Performance;
using SAPSec.Core.Interfaces.Repositories.KS4.Performance;
using SAPSec.Core.Services.KS4.Performance;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Tests.Services.KS4.Performance
{
    public class LAPerformanceServiceTests
    {
        private readonly Mock<ILAPerformanceRepository> _mockRepo;
        private readonly LAPerformanceService _service;

        public LAPerformanceServiceTests()
        {
            _mockRepo = new Mock<ILAPerformanceRepository>();
            _service = new LAPerformanceService(_mockRepo.Object);
        }

        [Fact]
        public void GetAllLAPerformance_ShouldReturnAllItems()
        {
            // Arrange
            var expectedDestinationss = new List<LAPerformance>
        {
            new LAPerformance { Id = "100",  Attainment8_Tot_LA_Current_Num = 99.99},
            new LAPerformance { Id = "101", Attainment8_Tot_LA_Current_Num = 90.00}
        };

            _mockRepo.Setup(r => r.GetAllLAPerformance())
                     .Returns(expectedDestinationss);

            // Act
            var result = _service.GetAllLAPerformance();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, a => a.Attainment8_Tot_LA_Current_Num == 99.99);
            Assert.Contains(result, a => a.Attainment8_Tot_LA_Current_Num == 90.00);
        }

        [Fact]
        public void GetAllLAPerformance_ShouldReturnEmpty_WhenNoData()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetAllLAPerformance())
                     .Returns(new List<LAPerformance>());

            // Act
            var result = _service.GetAllLAPerformance();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }


        [Fact]
        public void GetLAPerformance_ShouldReturnCorrectItem_WhenUrnExists()
        {
            // Arrange
            var urn = "100";
            var expectedDestinations = new LAPerformance { Id = urn, Attainment8_Tot_LA_Current_Num = 100 };

            _mockRepo.Setup(r => r.GetLAPerformance(urn))
                     .Returns(expectedDestinations);

            // Act
            var result = _service.GetLAPerformance(urn);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(urn, result.Id);
            Assert.Equal(100, result.Attainment8_Tot_LA_Current_Num);
        }

        [Fact]
        public void GetLAPerformance_ShouldReturnNull_WhenUrnDoesNotExist()
        {
            // Arrange
            var urn = "99999";
            _mockRepo.Setup(r => r.GetLAPerformance(urn))
                     .Returns(new LAPerformance());

            // Act
            var result = _service.GetLAPerformance(urn);

            // Assert
            Assert.Null(result.Attainment8_Tot_LA_Current_Num);
        }

        [Fact]
        public void GetLAPerformance_ShouldThrowException_WhenRepositoryThrows()
        {
            // Arrange
            var urn = "error";
            _mockRepo.Setup(r => r.GetLAPerformance(urn))
                     .Throws(new Exception("Database error"));

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _service.GetLAPerformance(urn));
            Assert.Equal("Database error", ex.Message);
        }
    }
}
