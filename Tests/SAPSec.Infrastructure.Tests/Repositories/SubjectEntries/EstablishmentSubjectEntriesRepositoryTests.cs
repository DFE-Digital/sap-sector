using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Model.KS4.SubjectEntries;
using SAPSec.Core.Interfaces.Repositories.Generic;
using SAPSec.Infrastructure.Repositories.KS4.SubjectEntries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Infrastructure.Tests.Repositories.SubjectEntries
{
    public class EstablishmentSubjectEntriesRepositoryTests
    {
        private readonly Mock<IGenericRepository<EstablishmentSubjectEntries>> _mockGenericRepo;
        private readonly Mock<ILogger<EstablishmentSubjectEntries>> _mockLogger;
        private readonly EstablishmentSubjectEntriesRepository _sut;

        public EstablishmentSubjectEntriesRepositoryTests()
        {
            _mockGenericRepo = new Mock<IGenericRepository<EstablishmentSubjectEntries>>();
            _mockLogger = new Mock<ILogger<EstablishmentSubjectEntries>>();
            _sut = new EstablishmentSubjectEntriesRepository(_mockGenericRepo.Object, _mockLogger.Object);
        }

        [Fact]
        public void GetAllEstablishmentSubjectEntriess_ReturnsAllItemsFromGenericRepository()
        {
            // Arrange
            var expected = new List<EstablishmentSubjectEntries>
            {
                new EstablishmentSubjectEntries { Id = "1", Bio49_Sum_Est_Current_Num= 99.99 },
                new EstablishmentSubjectEntries { Id = "2", Bio49_Sum_Est_Current_Num = 88.88 }
            };
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns(expected);

            // Act
            var result = _sut.GetAllEstablishmentSubjectEntries();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, e => e.Id == "1");
            Assert.Contains(result, e => e.Id == "2");
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetAllEstablishmentSubjectEntriess_ReturnsEmptyWhenGenericRepositoryReturnsNull()
        {
            // Arrange
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns((IEnumerable<EstablishmentSubjectEntries>?)null);

            // Act
            var result = _sut.GetAllEstablishmentSubjectEntries();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetEstablishmentSubjectEntries_ReturnsCorrectItemWhenUrnExists()
        {
            // Arrange
            var expected = new EstablishmentSubjectEntries { Id = "1", Bio49_Sum_Est_Current_Num = 99.99 };
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns(new[] { expected });

            // Act
            var result = _sut.GetEstablishmentSubjectEntries("1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("1", result.Id);
            Assert.Equal(99.99, result.Bio49_Sum_Est_Current_Num);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetEstablishmentSubjectEntries_ReturnsNewEstablishmentSubjectEntriesWhenUrnDoesNotExist()
        {
            // Arrange
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns(Enumerable.Empty<EstablishmentSubjectEntries>());

            // Act
            var result = _sut.GetEstablishmentSubjectEntries("999");

            // Assert
            Assert.NotNull(result);
            // When not found the repository returns a new EstablishmentSubjectEntries (defaults are empty strings / zeros)
            Assert.Equal(string.Empty, result.Id);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }
    }
}
