using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Model.KS4.Absence;
using SAPSec.Core.Model.KS4.Destinations;
using SAPSec.Core.Interfaces.Repositories.Generic;
using SAPSec.Core.Interfaces.Repositories.KS4.Destinations;
using SAPSec.Infrastructure.Repositories.KS4.Destinations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Infrastructure.Tests.Repositories.Destinations
{
    public class EnglandDestinationsRepositoryTests
    {
        private readonly Mock<IGenericRepository<EnglandDestinations>> _mockGenericRepo;
        private readonly Mock<ILogger<EnglandDestinations>> _mockLogger;
        private readonly EnglandDestinationsRepository _sut;

        public EnglandDestinationsRepositoryTests()
        {
            _mockGenericRepo = new Mock<IGenericRepository<EnglandDestinations>>();
            _mockLogger = new Mock<ILogger<EnglandDestinations>>();
            _sut = new EnglandDestinationsRepository(_mockGenericRepo.Object, _mockLogger.Object);
        }

        [Fact]
        public void GetAllEnglandDestinations_ReturnsAllItemsFromGenericRepository()
        {
            // Arrange
            var expected = new List<EnglandDestinations>
            {
                new() {AllDest_Tot_Eng_Current_Pct= 99.99 }
            };
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns(expected);

            // Act
            var result = _sut.GetEnglandDestinations();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(99.99, result.AllDest_Tot_Eng_Current_Pct);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetAllEnglandDestinationss_ReturnsEmptyWhenGenericRepositoryReturnsNull()
        {
            // Arrange
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns((IEnumerable<EnglandDestinations>?)null);

            // Act
            var result = _sut.GetEnglandDestinations();

            // Assert
            Assert.NotNull(result);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }
    }
}
