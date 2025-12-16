using Moq;
using SAPSec.Core.Model.KS4.SubjectEntries;
using SAPSec.Core.Interfaces.Repositories.KS4.SubjectEntries;
using SAPSec.Core.Services.KS4.SubjectEntries;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Tests.Services.KS4.SubjectEntries
{
    public class EstablishmentSubjectEntriesServiceTests
    {
        private readonly Mock<IEstablishmentSubjectEntriesRepository> _mockRepo;
        private readonly EstablishmentSubjectEntriesService _service;

        public EstablishmentSubjectEntriesServiceTests()
        {
            _mockRepo = new Mock<IEstablishmentSubjectEntriesRepository>();
            _service = new EstablishmentSubjectEntriesService(_mockRepo.Object);
        }

        [Fact]
        public void GetAllEstablishmentSubjectEntries_ShouldReturnAllItems()
        {
            // Arrange
            var expectedSubjectEntriess = new List<EstablishmentSubjectEntries>
        {
            new EstablishmentSubjectEntries { Id = "100",  Bio4_Sum_Est_Current_Num = 99.99},
            new EstablishmentSubjectEntries { Id = "101", Bio4_Sum_Est_Current_Num = 90.00}
        };

            _mockRepo.Setup(r => r.GetAllEstablishmentSubjectEntries())
                     .Returns(expectedSubjectEntriess);

            // Act
            var result = _service.GetAllEstablishmentSubjectEntries();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, a => a.Bio4_Sum_Est_Current_Num == 99.99);
            Assert.Contains(result, a => a.Bio4_Sum_Est_Current_Num == 90.00);
        }

        [Fact]
        public void GetAllEstablishmentSubjectEntries_ShouldReturnEmpty_WhenNoData()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetAllEstablishmentSubjectEntries())
                     .Returns(new List<EstablishmentSubjectEntries>());

            // Act
            var result = _service.GetAllEstablishmentSubjectEntries();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }


        [Fact]
        public void GetEstablishmentSubjectEntries_ShouldReturnCorrectItem_WhenUrnExists()
        {
            // Arrange
            var urn = "100";
            var expectedSubjectEntries = new EstablishmentSubjectEntries { Id = urn, Bio4_Sum_Est_Current_Num = 100 };

            _mockRepo.Setup(r => r.GetEstablishmentSubjectEntries(urn))
                     .Returns(expectedSubjectEntries);

            // Act
            var result = _service.GetEstablishmentSubjectEntries(urn);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(urn, result.Id);
            Assert.Equal(100, result.Bio4_Sum_Est_Current_Num);
        }

        [Fact]
        public void GetEstablishmentSubjectEntries_ShouldReturnNull_WhenUrnDoesNotExist()
        {
            // Arrange
            var urn = "99999";
            _mockRepo.Setup(r => r.GetEstablishmentSubjectEntries(urn))
                     .Returns(new EstablishmentSubjectEntries());

            // Act
            var result = _service.GetEstablishmentSubjectEntries(urn);

            // Assert
            Assert.Null(result.Bio4_Sum_Est_Current_Num);
        }

        [Fact]
        public void GetEstablishmentSubjectEntries_ShouldThrowException_WhenRepositoryThrows()
        {
            // Arrange
            var urn = "error";
            _mockRepo.Setup(r => r.GetEstablishmentSubjectEntries(urn))
                     .Throws(new Exception("Database error"));

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _service.GetEstablishmentSubjectEntries(urn));
            Assert.Equal("Database error", ex.Message);
        }
    }
}
