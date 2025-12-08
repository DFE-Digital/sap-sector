using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Interfaces.Repositories.Generic;
using SAPSec.Core.Model.KS4.SubjectEntries;
using SAPSec.Infrastructure.Repositories.KS4.SubjectEntries;

namespace SAPSec.Infrastructure.Tests.Repositories.SubjectEntries
{
    public class LASubjectEntriesRepositoryTests
    {
        private readonly Mock<IGenericRepository<LASubjectEntries>> _mockGenericRepo;
        private readonly Mock<ILogger<LASubjectEntries>> _mockLogger;
        private readonly LASubjectEntriesRepository _sut;

        public LASubjectEntriesRepositoryTests()
        {
            _mockGenericRepo = new Mock<IGenericRepository<LASubjectEntries>>();
            _mockLogger = new Mock<ILogger<LASubjectEntries>>();
            _sut = new LASubjectEntriesRepository(_mockGenericRepo.Object, _mockLogger.Object);
        }

        [Fact]
        public void GetAllLASubjectEntriess_ReturnsAllItemsFromGenericRepository()
        {
            // Arrange
            var expected = new List<LASubjectEntries>
            {
                new() { Id = "1", Bio49_Boy_LA_Current_Num= 99.99 },
                new() { Id = "2", Bio49_Boy_LA_Current_Num = 88.88 }
            };
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns(expected);

            // Act
            var result = _sut.GetAllLASubjectEntries();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, e => e.Id == "1");
            Assert.Contains(result, e => e.Id == "2");
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetAllLASubjectEntriess_ReturnsEmptyWhenGenericRepositoryReturnsNull()
        {
            // Arrange
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns((IEnumerable<LASubjectEntries>?)null);

            // Act
            var result = _sut.GetAllLASubjectEntries();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetLASubjectEntries_ReturnsCorrectItemWhenUrnExists()
        {
            // Arrange
            var expected = new LASubjectEntries { Id = "1", Bio49_Boy_LA_Current_Num = 99.99 };
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns(new[] { expected });

            // Act
            var result = _sut.GetLASubjectEntries("1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("1", result.Id);
            Assert.Equal(99.99, result.Bio49_Boy_LA_Current_Num);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetLASubjectEntries_ReturnsNewLASubjectEntriesWhenUrnDoesNotExist()
        {
            // Arrange
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns(Enumerable.Empty<LASubjectEntries>());

            // Act
            var result = _sut.GetLASubjectEntries("999");

            // Assert
            Assert.NotNull(result);
            // When not found the repository returns a new LASubjectEntries (defaults are empty strings / zeros)
            Assert.Equal(string.Empty, result.Id);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }
    }
}
