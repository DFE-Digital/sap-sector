using Moq;
using SAPSec.Core.Model.KS4.Absence;
using SAPSec.Core.Interfaces.Repositories.KS4.Absence;
using SAPSec.Core.Services.KS4.Absence;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAPSec.Core.Interfaces.Services.KS4.Absence;

namespace SAPSec.Core.Tests.Services.KS4.Absence
{
    public class LAAbsenceServiceTests
    {
        private readonly Mock<ILAAbsenceRepository> _mockRepo;
        private readonly LAAbsenceService _service;

        public LAAbsenceServiceTests()
        {
            _mockRepo = new Mock<ILAAbsenceRepository>();
            _service = new LAAbsenceService(_mockRepo.Object);
        }

        [Fact]
        public void GetAllLAAbsence_ShouldReturnAllItems()
        {
            // Arrange
            var expectedAbsences = new List<LAAbsence>
        {
            new LAAbsence { Id = "100",  Abs_Persistent_LA_Current_Pct = 99.99},
            new LAAbsence { Id = "101", Abs_Persistent_LA_Current_Pct = 90.00}
        };

            _mockRepo.Setup(r => r.GetAllLAAbsence())
                     .Returns(expectedAbsences);

            // Act
            var result = _service.GetAllLAAbsence();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, a => a.Abs_Persistent_LA_Current_Pct == 99.99);
            Assert.Contains(result, a => a.Abs_Persistent_LA_Current_Pct == 90.00);
        }

        [Fact]
        public void GetAllLAAbsence_ShouldReturnEmpty_WhenNoData()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetAllLAAbsence())
                     .Returns(new List<LAAbsence>());

            // Act
            var result = _service.GetAllLAAbsence();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }


        [Fact]
        public void GetLAAbsence_ShouldReturnCorrectItem_WhenUrnExists()
        {
            // Arrange
            var urn = "100";
            var expectedAbsence = new LAAbsence { Id = urn, Abs_Persistent_LA_Current_Pct = 100 };

            _mockRepo.Setup(r => r.GetLAAbsence(urn))
                     .Returns(expectedAbsence);

            // Act
            var result = _service.GetLAAbsence(urn);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(urn, result.Id);
            Assert.Equal(100, result.Abs_Persistent_LA_Current_Pct);
        }

        [Fact]
        public void GetLAAbsence_ShouldReturnNull_WhenUrnDoesNotExist()
        {
            // Arrange
            var urn = "99999";
            _mockRepo.Setup(r => r.GetLAAbsence(urn))
                     .Returns(new LAAbsence());

            // Act
            var result = _service.GetLAAbsence(urn);

            // Assert
            Assert.Null(result.Abs_Persistent_LA_Current_Pct);
        }

        [Fact]
        public void GetLAAbsence_ShouldThrowException_WhenRepositoryThrows()
        {
            // Arrange
            var urn = "error";
            _mockRepo.Setup(r => r.GetLAAbsence(urn))
                     .Throws(new Exception("Database error"));

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _service.GetLAAbsence(urn));
            Assert.Equal("Database error", ex.Message);
        }
    }
}
