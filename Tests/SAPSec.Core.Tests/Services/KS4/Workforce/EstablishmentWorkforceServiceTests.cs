using Moq;
using SAPSec.Core.Model.KS4.Workforce;
using SAPSec.Core.Interfaces.Repositories.KS4.Workforce;
using SAPSec.Core.Services.KS4.Workforce;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Tests.Services.KS4.Workforce
{
    [ExcludeFromCodeCoverage]
    public class EstablishmentWorkforceServiceTests
    {
        private readonly Mock<IEstablishmentWorkforceRepository> _mockRepo;
        private readonly EstablishmentWorkforceService _service;

        public EstablishmentWorkforceServiceTests()
        {
            _mockRepo = new Mock<IEstablishmentWorkforceRepository>();
            _service = new EstablishmentWorkforceService(_mockRepo.Object);
        }

        [Fact]
        public void GetAllEstablishmentWorkforce_ShouldReturnAllItems()
        {
            // Arrange
            var expectedDestinationss = new List<EstablishmentWorkforce>
        {
            new EstablishmentWorkforce { Id = "100",  Workforce_PupTeaRatio_Est_Current_Num = 99.99},
            new EstablishmentWorkforce { Id = "101", Workforce_PupTeaRatio_Est_Current_Num = 90.00}
        };

            _mockRepo.Setup(r => r.GetAllEstablishmentWorkforce())
                     .Returns(expectedDestinationss);

            // Act
            var result = _service.GetAllEstablishmentWorkforce();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, a => a.Workforce_PupTeaRatio_Est_Current_Num == 99.99);
            Assert.Contains(result, a => a.Workforce_PupTeaRatio_Est_Current_Num == 90.00);
        }

        [Fact]
        public void GetAllEstablishmentWorkforce_ShouldReturnEmpty_WhenNoData()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetAllEstablishmentWorkforce())
                     .Returns(new List<EstablishmentWorkforce>());

            // Act
            var result = _service.GetAllEstablishmentWorkforce();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }


        [Fact]
        public void GetEstablishmentWorkforce_ShouldReturnCorrectItem_WhenUrnExists()
        {
            // Arrange
            var urn = "100";
            var expectedDestinations = new EstablishmentWorkforce { Id = urn, Workforce_PupTeaRatio_Est_Current_Num = 100 };

            _mockRepo.Setup(r => r.GetEstablishmentWorkforce(urn))
                     .Returns(expectedDestinations);

            // Act
            var result = _service.GetEstablishmentWorkforce(urn);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(urn, result.Id);
            Assert.Equal(100, result.Workforce_PupTeaRatio_Est_Current_Num);
        }

        [Fact]
        public void GetEstablishmentWorkforce_ShouldReturnNull_WhenUrnDoesNotExist()
        {
            // Arrange
            var urn = "99999";
            _mockRepo.Setup(r => r.GetEstablishmentWorkforce(urn))
                     .Returns(new EstablishmentWorkforce());

            // Act
            var result = _service.GetEstablishmentWorkforce(urn);

            // Assert
            Assert.Null(result.Workforce_TotPupils_Est_Current_Num);
        }

        [Fact]
        public void GetEstablishmentWorkforce_ShouldThrowException_WhenRepositoryThrows()
        {
            // Arrange
            var urn = "error";
            _mockRepo.Setup(r => r.GetEstablishmentWorkforce(urn))
                     .Throws(new Exception("Database error"));

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _service.GetEstablishmentWorkforce(urn));
            Assert.Equal("Database error", ex.Message);
        }
    }
}
