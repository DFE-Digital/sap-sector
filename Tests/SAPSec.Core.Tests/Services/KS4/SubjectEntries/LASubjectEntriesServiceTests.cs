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
    public class LASubjectEntriesServiceTests
    {
        private readonly Mock<ILASubjectEntriesRepository> _mockRepo;
        private readonly LASubjectEntriesService _service;

        public LASubjectEntriesServiceTests()
        {
            _mockRepo = new Mock<ILASubjectEntriesRepository>();
            _service = new LASubjectEntriesService(_mockRepo.Object);
        }

        [Fact]
        public void GetAllLASubjectEntries_ShouldReturnAllItems()
        {
            // Arrange
            var expectedDestinationss = new List<LASubjectEntries>
        {
            new LASubjectEntries { Id = "100",  Bio49_Boy_LA_Current_Num = 99.99},
            new LASubjectEntries { Id = "101", Bio49_Boy_LA_Current_Num = 90.00}
        };

            _mockRepo.Setup(r => r.GetAllLASubjectEntries())
                     .Returns(expectedDestinationss);

            // Act
            var result = _service.GetAllLASubjectEntries();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, a => a.Bio49_Boy_LA_Current_Num == 99.99);
            Assert.Contains(result, a => a.Bio49_Boy_LA_Current_Num == 90.00);
        }

        [Fact]
        public void GetAllLASubjectEntries_ShouldReturnEmpty_WhenNoData()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetAllLASubjectEntries())
                     .Returns(new List<LASubjectEntries>());

            // Act
            var result = _service.GetAllLASubjectEntries();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }


        [Fact]
        public void GetLASubjectEntries_ShouldReturnCorrectItem_WhenUrnExists()
        {
            // Arrange
            var urn = "100";
            var expectedDestinations = new LASubjectEntries { Id = urn, Bio49_Boy_LA_Current_Num = 100 };

            _mockRepo.Setup(r => r.GetLASubjectEntries(urn))
                     .Returns(expectedDestinations);

            // Act
            var result = _service.GetLASubjectEntries(urn);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(urn, result.Id);
            Assert.Equal(100, result.Bio49_Boy_LA_Current_Num);
        }

        [Fact]
        public void GetLASubjectEntries_ShouldReturnNull_WhenUrnDoesNotExist()
        {
            // Arrange
            var urn = "99999";
            _mockRepo.Setup(r => r.GetLASubjectEntries(urn))
                     .Returns(new LASubjectEntries());

            // Act
            var result = _service.GetLASubjectEntries(urn);

            // Assert
            Assert.Null(result.Bio49_Boy_LA_Current_Num);
        }

        [Fact]
        public void GetLASubjectEntries_ShouldThrowException_WhenRepositoryThrows()
        {
            // Arrange
            var urn = "error";
            _mockRepo.Setup(r => r.GetLASubjectEntries(urn))
                     .Throws(new Exception("Database error"));

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _service.GetLASubjectEntries(urn));
            Assert.Equal("Database error", ex.Message);
        }
    }
}
