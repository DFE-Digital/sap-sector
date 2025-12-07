using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Model.KS4.Suspensions;
using SAPSec.Core.Interfaces.Repositories.Generic;
using SAPSec.Infrastructure.Repositories.KS4.Suspensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Infrastructure.Tests.Repositories.Suspensions
{
    public class EnglandSuspensionsRepositoryTests
    {
        private readonly Mock<IGenericRepository<EnglandSuspensions>> _mockGenericRepo;
        private readonly Mock<ILogger<EnglandSuspensions>> _mockLogger;
        private readonly EnglandSuspensionsRepository _sut;

        public EnglandSuspensionsRepositoryTests()
        {
            _mockGenericRepo = new Mock<IGenericRepository<EnglandSuspensions>>();
            _mockLogger = new Mock<ILogger<EnglandSuspensions>>();
            _sut = new EnglandSuspensionsRepository(_mockGenericRepo.Object, _mockLogger.Object);
        }

        [Fact]
        public void GetAllEnglandSuspensions_ReturnsAllItemsFromGenericRepository()
        {
            // Arrange
            var expected = new List<EnglandSuspensions>
            {
                new() {Sus_Tot_Eng_Current_Pct= 99.99 }
            };
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns(expected);

            // Act
            var result = _sut.GetEnglandSuspensions();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(99.99, result.Sus_Tot_Eng_Current_Pct);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetAllEnglandSuspensionss_ReturnsEmptyWhenGenericRepositoryReturnsNull()
        {
            // Arrange
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns((IEnumerable<EnglandSuspensions>?)null);

            // Act
            var result = _sut.GetEnglandSuspensions();

            // Assert
            Assert.NotNull(result);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }
    }
}
