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
    public class EstablishmentPerformanceServiceTests
    {
        private readonly Mock<IEstablishmentPerformanceRepository> _mockRepo;
        private readonly EstablishmentPerformanceService _service;

        public EstablishmentPerformanceServiceTests()
        {
            _mockRepo = new Mock<IEstablishmentPerformanceRepository>();
            _service = new EstablishmentPerformanceService(_mockRepo.Object);
        }

        [Fact]
        public void GetAllEstablishmentPerformance_ShouldReturnAllItems()
        {
            // Arrange
            var expectedPerformances = new List<EstablishmentPerformance>
        {
            new EstablishmentPerformance { Id = "100",  Attainment8_Tot_Est_Current_Num = 99.99},
            new EstablishmentPerformance { Id = "101", Attainment8_Tot_Est_Current_Num = 90.00}
        };

            _mockRepo.Setup(r => r.GetAllEstablishmentPerformance())
                     .Returns(expectedPerformances);

            // Act
            var result = _service.GetAllEstablishmentPerformance();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, a => a.Attainment8_Tot_Est_Current_Num == 99.99);
            Assert.Contains(result, a => a.Attainment8_Tot_Est_Current_Num == 90.00);
        }

        [Fact]
        public void GetAllEstablishmentPerformance_ShouldReturnEmpty_WhenNoData()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetAllEstablishmentPerformance())
                     .Returns(new List<EstablishmentPerformance>());

            // Act
            var result = _service.GetAllEstablishmentPerformance();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }


        [Fact]
        public void GetEstablishmentPerformance_ShouldReturnCorrectItem_WhenUrnExists()
        {
            // Arrange
            var urn = "100";
            var expectedPerformance = new EstablishmentPerformance { Id = urn, Attainment8_Tot_Est_Current_Num = 100 };

            _mockRepo.Setup(r => r.GetEstablishmentPerformance(urn))
                     .Returns(expectedPerformance);

            // Act
            var result = _service.GetEstablishmentPerformance(urn);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(urn, result.Id);
            Assert.Equal(100, result.Attainment8_Tot_Est_Current_Num);
        }

        [Fact]
        public void GetEstablishmentPerformance_ShouldReturnNull_WhenUrnDoesNotExist()
        {
            // Arrange
            var urn = "99999";
            _mockRepo.Setup(r => r.GetEstablishmentPerformance(urn))
                     .Returns(new EstablishmentPerformance());

            // Act
            var result = _service.GetEstablishmentPerformance(urn);

            // Assert
            Assert.Null(result.Attainment8_Tot_Est_Current_Num);
        }

        [Fact]
        public void GetEstablishmentPerformance_ShouldThrowException_WhenRepositoryThrows()
        {
            // Arrange
            var urn = "error";
            _mockRepo.Setup(r => r.GetEstablishmentPerformance(urn))
                     .Throws(new Exception("Database error"));

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _service.GetEstablishmentPerformance(urn));
            Assert.Equal("Database error", ex.Message);
        }
    }
}
