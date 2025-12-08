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
    public class EnglandPerformanceServiceTests
    {
        private readonly Mock<IEnglandPerformanceRepository> _mockRepo;
        private readonly EnglandPerformanceService _service;

        public EnglandPerformanceServiceTests()
        {
            _mockRepo = new Mock<IEnglandPerformanceRepository>();
            _service = new EnglandPerformanceService(_mockRepo.Object);
        }

        [Fact]
        public void GetAllEnglandPerformance_ShouldReturnAllItems()
        {
            // Arrange
            var expectedAbsences =
            new EnglandPerformance { Attainment8_Tot_Eng_Current_Num = 99.99 };

            _mockRepo.Setup(r => r.GetEnglandPerformance())
                         .Returns(expectedAbsences);

            // Act
            var result = _service.GetEnglandPerformance();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(99.99, result.Attainment8_Tot_Eng_Current_Num);
        }

        [Fact]
        public void GetAllEnglandPerformance_ShouldReturnEmpty_WhenNoData()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetEnglandPerformance())
                     .Returns(new EnglandPerformance());

            // Act
            var result = _service.GetEnglandPerformance();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetEnglandPerformance_ShouldThrowException_WhenRepositoryThrows()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetEnglandPerformance())
                     .Throws(new Exception("Database error"));

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _service.GetEnglandPerformance());
            Assert.Equal("Database error", ex.Message);
        }

    }
}
