using Moq;
using SAPSec.Core.Model.KS4.Suspensions;
using SAPSec.Core.Interfaces.Repositories.KS4.Suspensions;
using SAPSec.Core.Services.KS4.Suspensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Tests.Services.KS4.Suspensions
{
    public class EstablishmentSuspensionsServiceTests
    {
        private readonly Mock<IEstablishmentSuspensionsRepository> _mockRepo;
        private readonly EstablishmentSuspensionsService _service;

        public EstablishmentSuspensionsServiceTests()
        {
            _mockRepo = new Mock<IEstablishmentSuspensionsRepository>();
            _service = new EstablishmentSuspensionsService(_mockRepo.Object);
        }

        [Fact]
        public void GetAllEstablishmentSuspensions_ShouldReturnAllItems()
        {
            // Arrange
            var expectedSuspensionss = new List<EstablishmentSuspensions>
        {
            new EstablishmentSuspensions { Id = "100",  Sus_Tot_Est_Current_Pct = 99.99},
            new EstablishmentSuspensions { Id = "101", Sus_Tot_Est_Current_Pct = 90.00}
        };

            _mockRepo.Setup(r => r.GetAllEstablishmentSuspensions())
                     .Returns(expectedSuspensionss);

            // Act
            var result = _service.GetAllEstablishmentSuspensions();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, a => a.Sus_Tot_Est_Current_Pct == 99.99);
            Assert.Contains(result, a => a.Sus_Tot_Est_Current_Pct == 90.00);
        }

        [Fact]
        public void GetAllEstablishmentSuspensions_ShouldReturnEmpty_WhenNoData()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetAllEstablishmentSuspensions())
                     .Returns(new List<EstablishmentSuspensions>());

            // Act
            var result = _service.GetAllEstablishmentSuspensions();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }


        [Fact]
        public void GetEstablishmentSuspensions_ShouldReturnCorrectItem_WhenUrnExists()
        {
            // Arrange
            var urn = "100";
            var expectedSuspensions = new EstablishmentSuspensions { Id = urn, Sus_Tot_Est_Current_Pct = 100 };

            _mockRepo.Setup(r => r.GetEstablishmentSuspensions(urn))
                     .Returns(expectedSuspensions);

            // Act
            var result = _service.GetEstablishmentSuspensions(urn);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(urn, result.Id);
            Assert.Equal(100, result.Sus_Tot_Est_Current_Pct);
        }

        [Fact]
        public void GetEstablishmentSuspensions_ShouldReturnNull_WhenUrnDoesNotExist()
        {
            // Arrange
            var urn = "99999";
            _mockRepo.Setup(r => r.GetEstablishmentSuspensions(urn))
                     .Returns(new EstablishmentSuspensions());

            // Act
            var result = _service.GetEstablishmentSuspensions(urn);

            // Assert
            Assert.Null(result.Sus_Tot_Est_Current_Pct);
        }

        [Fact]
        public void GetEstablishmentSuspensions_ShouldThrowException_WhenRepositoryThrows()
        {
            // Arrange
            var urn = "error";
            _mockRepo.Setup(r => r.GetEstablishmentSuspensions(urn))
                     .Throws(new Exception("Database error"));

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _service.GetEstablishmentSuspensions(urn));
            Assert.Equal("Database error", ex.Message);
        }
    }
}
