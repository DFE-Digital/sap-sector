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
    public class EstablishmentSuspensionsRepositoryTests
    {
        private readonly Mock<IGenericRepository<EstablishmentSuspensions>> _mockGenericRepo;
        private readonly Mock<ILogger<EstablishmentSuspensions>> _mockLogger;
        private readonly EstablishmentSuspensionsRepository _sut;

        public EstablishmentSuspensionsRepositoryTests()
        {
            _mockGenericRepo = new Mock<IGenericRepository<EstablishmentSuspensions>>();
            _mockLogger = new Mock<ILogger<EstablishmentSuspensions>>();
            _sut = new EstablishmentSuspensionsRepository(_mockGenericRepo.Object, _mockLogger.Object);
        }

        [Fact]
        public void GetAllEstablishmentSuspensionss_ReturnsAllItemsFromGenericRepository()
        {
            // Arrange
            var expected = new List<EstablishmentSuspensions>
            {
                new EstablishmentSuspensions { Id = "1", Sus_Tot_Est_Current_Pct= 99.99 },
                new EstablishmentSuspensions { Id = "2", Sus_Tot_Est_Current_Pct = 88.88 }
            };
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns(expected);

            // Act
            var result = _sut.GetAllEstablishmentSuspensions();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, e => e.Id == "1");
            Assert.Contains(result, e => e.Id == "2");
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetAllEstablishmentSuspensionss_ReturnsEmptyWhenGenericRepositoryReturnsNull()
        {
            // Arrange
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns((IEnumerable<EstablishmentSuspensions>?)null);

            // Act
            var result = _sut.GetAllEstablishmentSuspensions();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetEstablishmentSuspensions_ReturnsCorrectItemWhenUrnExists()
        {
            // Arrange
            var expected = new EstablishmentSuspensions { Id = "1", Sus_Tot_Est_Current_Pct = 99.99 };
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns(new[] { expected });

            // Act
            var result = _sut.GetEstablishmentSuspensions("1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("1", result.Id);
            Assert.Equal(99.99, result.Sus_Tot_Est_Current_Pct);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetEstablishmentSuspensions_ReturnsNewEstablishmentSuspensionsWhenUrnDoesNotExist()
        {
            // Arrange
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns(Enumerable.Empty<EstablishmentSuspensions>());

            // Act
            var result = _sut.GetEstablishmentSuspensions("999");

            // Assert
            Assert.NotNull(result);
            // When not found the repository returns a new EstablishmentSuspensions (defaults are empty strings / zeros)
            Assert.Equal(string.Empty, result.Id);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }
    }
}
