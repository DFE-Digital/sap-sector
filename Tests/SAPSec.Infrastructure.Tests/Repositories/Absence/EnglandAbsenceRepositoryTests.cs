using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Interfaces.Repositories.Generic;
using SAPSec.Core.Model.KS4.Absence;
using SAPSec.Infrastructure.Repositories.KS4.Absence;

namespace SAPSec.Infrastructure.Tests.Repositories.Absence
{
    public class EnglandAbsenceRepositoryTests
    {
        private readonly Mock<IGenericRepository<EnglandAbsence>> _mockGenericRepo;
        private readonly Mock<ILogger<EnglandAbsence>> _mockLogger;
        private readonly EnglandAbsenceRepository _sut;

        public EnglandAbsenceRepositoryTests()
        {
            _mockGenericRepo = new Mock<IGenericRepository<EnglandAbsence>>();
            _mockLogger = new Mock<ILogger<EnglandAbsence>>();
            _sut = new EnglandAbsenceRepository(_mockGenericRepo.Object, _mockLogger.Object);
        }

        [Fact]
        public void GetAllEnglandAbsence_ReturnsAllItemsFromGenericRepository()
        {
            // Arrange
            var expected = new List<EnglandAbsence>
            {
                new() {Abs_Persistent_Eng_Current_Pct= 99.99 }
            };
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns(expected);

            // Act
            var result = _sut.GetEnglandAbsence();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(99.99, result.Abs_Persistent_Eng_Current_Pct);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetAllEnglandAbsences_ReturnsEmptyWhenGenericRepositoryReturnsNull()
        {
            // Arrange
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns((IEnumerable<EnglandAbsence>?)null);

            // Act
            var result = _sut.GetEnglandAbsence();

            // Assert
            Assert.NotNull(result);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }
    }
}
