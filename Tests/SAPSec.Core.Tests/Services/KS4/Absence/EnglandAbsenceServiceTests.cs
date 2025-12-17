using Moq;
using SAPSec.Core.Model.KS4.Absence;
using SAPSec.Core.Interfaces.Repositories.KS4.Absence;
using SAPSec.Core.Interfaces.Services.KS4.Absence;
using SAPSec.Core.Services.KS4.Absence;

namespace SAPSec.Core.Tests.Services.KS4.Absence
{
    public class EnglandAbsenceServiceTests
    {
        private readonly Mock<IEnglandAbsenceRepository> _mockRepo;
        private readonly EnglandAbsenceService _service;

        public EnglandAbsenceServiceTests()
        {
            _mockRepo = new Mock<IEnglandAbsenceRepository>();
            _service = new EnglandAbsenceService(_mockRepo.Object);
        }

        [Fact]
        public void GetAllEnglandAbsence_ShouldReturnAllItems()
        {
            // Arrange
            var expectedAbsences =
            new EnglandAbsence { Abs_Persistent_Eng_Current_Pct = 99.99 };

            _mockRepo.Setup(r => r.GetEnglandAbsence())
                         .Returns(expectedAbsences);

            // Act
            var result = _service.GetEnglandAbsence();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(99.99, result.Abs_Persistent_Eng_Current_Pct);
        }

        [Fact]
        public void GetAllEnglandAbsence_ShouldReturnEmpty_WhenNoData()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetEnglandAbsence())
                     .Returns(new EnglandAbsence());

            // Act
            var result = _service.GetEnglandAbsence();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetEnglandAbsence_ShouldThrowException_WhenRepositoryThrows()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetEnglandAbsence())
                     .Throws(new Exception("Database error"));

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _service.GetEnglandAbsence());
            Assert.Equal("Database error", ex.Message);
        }

    }
}