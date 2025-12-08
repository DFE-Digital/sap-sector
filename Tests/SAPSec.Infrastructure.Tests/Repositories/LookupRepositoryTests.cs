using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Model;
using SAPSec.Core.Interfaces.Repositories.Generic;
using SAPSec.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Infrastructure.Tests.Repositories
{
    public class LookupRepositoryTests
    {
        private readonly Mock<IGenericRepository<Lookup>> _mockGenericRepo;
        private readonly Mock<ILogger<Lookup>> _mockLogger;
        private readonly LookupRepository _sut;

        public LookupRepositoryTests()
        {
            _mockGenericRepo = new Mock<IGenericRepository<Lookup>>();
            _mockLogger = new Mock<ILogger<Lookup>>();
            _sut = new LookupRepository(_mockGenericRepo.Object, _mockLogger.Object);
        }

        [Fact]
        public void GetAllLookups_ReturnsAllItemsFromGenericRepository()
        {
            // Arrange
            var expected = new List<Lookup>
            {
                new Lookup { Id = "1", Name= "One", LookupType = "Lookup" },
                new Lookup { Id = "2", Name = "Two", LookupType = "Type" }
            };
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns(expected);

            // Act
            var result = _sut.GetAllLookups();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, e => e.Id == "1");
            Assert.Contains(result, e => e.Id == "2");
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetAllLookups_ReturnsEmptyWhenGenericRepositoryReturnsNull()
        {
            // Arrange
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns((IEnumerable<Lookup>?)null);

            // Act
            var result = _sut.GetAllLookups();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetLookup_ReturnsCorrectItemWhenUrnExists()
        {
            // Arrange
            var expected = new Lookup { Id = "1", Name = "One", LookupType = "Lookup" };
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns(new[] { expected });

            // Act
            var result = _sut.GetLookup("1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("1", result.Id);
            Assert.Equal("One", result.Name);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }

        [Fact]
        public void GetLookup_ReturnsNewLookupWhenUrnDoesNotExist()
        {
            // Arrange
            _mockGenericRepo.Setup(r => r.ReadAll()).Returns(Enumerable.Empty<Lookup>());

            // Act
            var result = _sut.GetLookup("999");

            // Assert
            Assert.NotNull(result);
            // When not found the repository returns a new Lookup (defaults are empty strings / zeros)
            Assert.Equal(string.Empty, result.Id);
            _mockGenericRepo.Verify(r => r.ReadAll(), Times.Once);
        }
    }
}
