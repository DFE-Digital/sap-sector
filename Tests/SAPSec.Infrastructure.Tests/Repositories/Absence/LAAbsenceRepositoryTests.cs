using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Model;
using SAPSec.Core.Model.KS4.Absence;
using SAPSec.Core.Interfaces.Repositories.Generic;
using SAPSec.Infrastructure.Repositories.KS4.Absence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Infrastructure.Tests.Repositories.Absence
{
    public class LAAbsenceRepositoryTests
    {
        private readonly Mock<IGenericRepository<LAAbsence>> _mockGenericRepo;
        private readonly Mock<ILogger<LAAbsence>> _mockLogger;
        private readonly LAAbsenceRepository _sut;

        public LAAbsenceRepositoryTests()
        {
            _mockGenericRepo = new Mock<IGenericRepository<LAAbsence>>();
            _mockLogger = new Mock<ILogger<LAAbsence>>();
            _sut = new LAAbsenceRepository(_mockGenericRepo.Object, _mockLogger.Object);
        }

        [Fact]
        public void GetAllLAAbsences_ReturnsAllItemsFromGenericRepository()
        {
            // Arrange
            var expected = new List<LAAbsence>
            {
                new() { Id = "1", Abs_Persistent_LA_Current_Pct= 99.99 },
                new() { Id = "2", Abs_Persistent_LA_Current_Pct = 88.88 }
            };
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns(expected);

            // Act
            var result = _sut.GetAllLAAbsence();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, e => e.Id == "1");
            Assert.Contains(result, e => e.Id == "2");
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetAllLAAbsences_ReturnsEmptyWhenGenericRepositoryReturnsNull()
        {
            // Arrange
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns((IEnumerable<LAAbsence>?)null);

            // Act
            var result = _sut.GetAllLAAbsence();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetLAAbsence_ReturnsCorrectItemWhenUrnExists()
        {
            // Arrange
            var expected = new LAAbsence { Id = "1", Abs_Persistent_LA_Current_Pct = 99.99 };
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns(new[] { expected });

            // Act
            var result = _sut.GetLAAbsence("1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("1", result.Id);
            Assert.Equal(99.99, result.Abs_Persistent_LA_Current_Pct);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetLAAbsence_ReturnsNewLAAbsenceWhenUrnDoesNotExist()
        {
            // Arrange
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns([]);

            // Act
            var result = _sut.GetLAAbsence("999");

            // Assert
            Assert.NotNull(result);
            // When not found the repository returns a new LAAbsence (defaults are empty strings / zeros)
            Assert.Equal(string.Empty, result.Id);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }
    }
}
