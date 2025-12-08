using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Model.KS4.Performance;
using SAPSec.Core.Interfaces.Repositories.Generic;
using SAPSec.Infrastructure.Repositories.KS4.Performance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Infrastructure.Tests.Repositories.Performance
{
    public class EnglandPerformanceRepositoryTests
    {
        private readonly Mock<IGenericRepository<EnglandPerformance>> _mockGenericRepo;
        private readonly Mock<ILogger<EnglandPerformance>> _mockLogger;
        private readonly EnglandPerformanceRepository _sut;

        public EnglandPerformanceRepositoryTests()
        {
            _mockGenericRepo = new Mock<IGenericRepository<EnglandPerformance>>();
            _mockLogger = new Mock<ILogger<EnglandPerformance>>();
            _sut = new EnglandPerformanceRepository(_mockGenericRepo.Object, _mockLogger.Object);
        }

        [Fact]
        public void GetAllEnglandPerformance_ReturnsAllItemsFromGenericRepository()
        {
            // Arrange
            var expected = new List<EnglandPerformance>
            {
                new() {Attainment8_Tot_Eng_Current_Num= 99.99 }
            };
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns(expected);

            // Act
            var result = _sut.GetEnglandPerformance();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(99.99, result.Attainment8_Tot_Eng_Current_Num);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetAllEnglandPerformances_ReturnsEmptyWhenGenericRepositoryReturnsNull()
        {
            // Arrange
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns((IEnumerable<EnglandPerformance>?)null);

            // Act
            var result = _sut.GetEnglandPerformance();

            // Assert
            Assert.NotNull(result);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }
    }
}
