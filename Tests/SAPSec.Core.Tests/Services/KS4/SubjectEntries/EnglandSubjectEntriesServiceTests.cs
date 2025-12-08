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
    public class EnglandSubjectEntriesServiceTests
    {
        private readonly Mock<IEnglandSubjectEntriesRepository> _mockRepo;
        private readonly EnglandSubjectEntriesService _service;

        public EnglandSubjectEntriesServiceTests()
        {
            _mockRepo = new Mock<IEnglandSubjectEntriesRepository>();
            _service = new EnglandSubjectEntriesService(_mockRepo.Object);
        }

        [Fact]
        public void GetAllEnglandSubjectEntries_ShouldReturnAllItems()
        {
            // Arrange
            var expectedAbsences =
            new EnglandSubjectEntries { Bio49_Boy_Eng_Current_Num = 99.99 };

            _mockRepo.Setup(r => r.GetEnglandSubjectEntries())
                         .Returns(expectedAbsences);

            // Act
            var result = _service.GetEnglandSubjectEntries();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(99.99, result.Bio49_Boy_Eng_Current_Num);
        }

        [Fact]
        public void GetAllEnglandSubjectEntries_ShouldReturnEmpty_WhenNoData()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetEnglandSubjectEntries())
                     .Returns(new EnglandSubjectEntries());

            // Act
            var result = _service.GetEnglandSubjectEntries();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetEnglandSubjectEntries_ShouldThrowException_WhenRepositoryThrows()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetEnglandSubjectEntries())
                     .Throws(new Exception("Database error"));

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _service.GetEnglandSubjectEntries());
            Assert.Equal("Database error", ex.Message);
        }

    }
}
