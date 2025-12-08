using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Interfaces.Repositories.Generic;
using SAPSec.Core.Model.KS4.Workforce;
using SAPSec.Infrastructure.Repositories.KS4.Workforce;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Infrastructure.Tests.Repositories.Workforce
{
    public class EstablishmentWorkforceRepositoryTests
    {
        private readonly Mock<IGenericRepository<EstablishmentWorkforce>> _mockGenericRepo;
        private readonly Mock<ILogger<EstablishmentWorkforce>> _mockLogger;
        private readonly EstablishmentWorkforceRepository _sut;

        public EstablishmentWorkforceRepositoryTests()
        {
            _mockGenericRepo = new Mock<IGenericRepository<EstablishmentWorkforce>>();
            _mockLogger = new Mock<ILogger<EstablishmentWorkforce>>();
            _sut = new EstablishmentWorkforceRepository(_mockGenericRepo.Object, _mockLogger.Object);
        }

        [Fact]
        public void GetAllEstablishmentWorkforces_ReturnsAllItemsFromGenericRepository()
        {
            // Arrange
            var expected = new List<EstablishmentWorkforce>
            {
                new EstablishmentWorkforce { Id = "1", Workforce_PupTeaRatio_Est_Current_Num= 99.99 },
                new EstablishmentWorkforce { Id = "2", Workforce_PupTeaRatio_Est_Current_Num = 88.88 }
            };
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns(expected);

            // Act
            var result = _sut.GetAllEstablishmentWorkforce();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, e => e.Id == "1");
            Assert.Contains(result, e => e.Id == "2");
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetAllEstablishmentWorkforces_ReturnsEmptyWhenGenericRepositoryReturnsNull()
        {
            // Arrange
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns((IEnumerable<EstablishmentWorkforce>?)null);

            // Act
            var result = _sut.GetAllEstablishmentWorkforce();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetEstablishmentWorkforce_ReturnsCorrectItemWhenUrnExists()
        {
            // Arrange
            var expected = new EstablishmentWorkforce { Id = "1", Workforce_PupTeaRatio_Est_Current_Num = 99.99 };
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns(new[] { expected });

            // Act
            var result = _sut.GetEstablishmentWorkforce("1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("1", result.Id);
            Assert.Equal(99.99, result.Workforce_PupTeaRatio_Est_Current_Num);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetEstablishmentWorkforce_ReturnsNewEstablishmentWorkforceWhenUrnDoesNotExist()
        {
            // Arrange
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns(Enumerable.Empty<EstablishmentWorkforce>());

            // Act
            var result = _sut.GetEstablishmentWorkforce("999");

            // Assert
            Assert.NotNull(result);
            // When not found the repository returns a new EstablishmentWorkforce (defaults are empty strings / zeros)
            Assert.Equal(string.Empty, result.Id);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }
    }
}
