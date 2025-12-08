using Moq;
using SAPSec.Core.Model.KS4.Absence;
using SAPSec.Core.Interfaces.Repositories.KS4.Absence;
using SAPSec.Core.Services.KS4.Absence;

namespace SAPSec.Core.Tests.Services.KS4.Absence
{

    public class EstablishmentAbsenceServiceTests
    {
        private readonly Mock<IEstablishmentAbsenceRepository> _mockRepo;
        private readonly EstablishmentAbsenceService _service;

        public EstablishmentAbsenceServiceTests()
        {
            _mockRepo = new Mock<IEstablishmentAbsenceRepository>();
            _service = new EstablishmentAbsenceService(_mockRepo.Object);
        }

        [Fact]
        public void GetAllEstablishmentAbsence_ShouldReturnAllItems()
        {
            // Arrange
            var expectedAbsences = new List<EstablishmentAbsence>
        {
            new EstablishmentAbsence { Id = "100",  Abs_Tot_Est_Current_Pct = 99.99},
            new EstablishmentAbsence { Id = "101", Abs_Tot_Est_Current_Pct = 90.00}
        };

            _mockRepo.Setup(r => r.GetAllEstablishmentAbsence())
                     .Returns(expectedAbsences);

            // Act
            var result = _service.GetAllEstablishmentAbsence();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, a => a.Abs_Tot_Est_Current_Pct == 99.99);
            Assert.Contains(result, a => a.Abs_Tot_Est_Current_Pct == 90.00);
        }

        [Fact]
        public void GetAllEstablishmentAbsence_ShouldReturnEmpty_WhenNoData()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetAllEstablishmentAbsence())
                     .Returns(new List<EstablishmentAbsence>());

            // Act
            var result = _service.GetAllEstablishmentAbsence();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }


        [Fact]
        public void GetEstablishmentAbsence_ShouldReturnCorrectItem_WhenUrnExists()
        {
            // Arrange
            var urn = "100";
            var expectedAbsence = new EstablishmentAbsence { Id = urn, Abs_Tot_Est_Current_Pct = 100 };

            _mockRepo.Setup(r => r.GetEstablishmentAbsence(urn))
                     .Returns(expectedAbsence);

            // Act
            var result = _service.GetEstablishmentAbsence(urn);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(urn, result.Id);
            Assert.Equal(100, result.Abs_Tot_Est_Current_Pct);
        }

        [Fact]
        public void GetEstablishmentAbsence_ShouldReturnNull_WhenUrnDoesNotExist()
        {
            // Arrange
            var urn = "99999";
            _mockRepo.Setup(r => r.GetEstablishmentAbsence(urn))
                     .Returns(new EstablishmentAbsence());

            // Act
            var result = _service.GetEstablishmentAbsence(urn);

            // Assert
            Assert.Null(result.Abs_Persistent_Est_Current_Pct);
        }

        [Fact]
        public void GetEstablishmentAbsence_ShouldThrowException_WhenRepositoryThrows()
        {
            // Arrange
            var urn = "error";
            _mockRepo.Setup(r => r.GetEstablishmentAbsence(urn))
                     .Throws(new System.Exception("Database error"));

            // Act & Assert
            var ex = Assert.Throws<System.Exception>(() => _service.GetEstablishmentAbsence(urn));
            Assert.Equal("Database error", ex.Message);
        }


    }
}
