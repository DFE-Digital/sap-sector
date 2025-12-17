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
    public class EnglandSubjectEntriesRepositoryTests
    {
        private readonly Mock<IGenericRepository<EnglandSubjectEntries>> _mockGenericRepo;
        private readonly Mock<ILogger<EnglandSubjectEntries>> _mockLogger;
        private readonly EnglandSubjectEntriesRepository _sut;

        public EnglandSubjectEntriesRepositoryTests()
        {
            _mockGenericRepo = new Mock<IGenericRepository<EnglandSubjectEntries>>();
            _mockLogger = new Mock<ILogger<EnglandSubjectEntries>>();
            _sut = new EnglandSubjectEntriesRepository(_mockGenericRepo.Object, _mockLogger.Object);
        }

        [Fact]
        public void GetAllEnglandSubjectEntries_ReturnsAllItemsFromGenericRepository()
        {
            // Arrange
            var expected = new List<EnglandSubjectEntries>
            {
                new() {Bio49_Boy_Eng_Current_Num= 99.99 }
            };
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns(expected);

            // Act
            var result = _sut.GetEnglandSubjectEntries();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(99.99, result.Bio49_Boy_Eng_Current_Num);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetAllEnglandSubjectEntriess_ReturnsEmptyWhenGenericRepositoryReturnsNull()
        {
            // Arrange
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns((IEnumerable<EnglandSubjectEntries>?)null);

            // Act
            var result = _sut.GetEnglandSubjectEntries();

            // Assert
            Assert.NotNull(result);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }
    }
}
