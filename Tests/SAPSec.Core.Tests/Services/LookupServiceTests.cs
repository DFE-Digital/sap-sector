using Moq;
using SAPSec.Core.Model;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Tests.Services
{
    [ExcludeFromCodeCoverage]
    public class LookupServiceTests
    {
        private readonly Mock<ILookupRepository> _mockRepo;
        private readonly LookupService _service;

        public LookupServiceTests()
        {
            _mockRepo = new Mock<ILookupRepository>();
            _service = new LookupService(_mockRepo.Object);
        }

        [Fact]
        public void GetAllLookup_ShouldReturnAllItems()
        {
            // Arrange
            var expectedDestinationss = new List<Lookup>
        {
            new Lookup { Id = "100",  Name = "Test One", LookupType = "Testing"},
            new Lookup { Id = "101",  Name = "Test Two", LookupType = "Testing"},
        };

            _mockRepo.Setup(r => r.GetAllLookups())
                     .Returns(expectedDestinationss);

            // Act
            var result = _service.GetAllLookups();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, a => a.Name == "Test One");
            Assert.Contains(result, a => a.Name == "Test Two");
        }

        [Fact]
        public void GetAllLookup_ShouldReturnEmpty_WhenNoData()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetAllLookups())
                     .Returns(new List<Lookup>());

            // Act
            var result = _service.GetAllLookups();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }


        [Fact]
        public void GetLookup_ShouldReturnCorrectItem_WhenUrnExists()
        {
            // Arrange
            var urn = "100";
            var expectedDestinations = new Lookup { Id = urn, Name = "Test One", LookupType = "Testing" };

            _mockRepo.Setup(r => r.GetLookup(urn))
                     .Returns(expectedDestinations);

            // Act
            var result = _service.GetLookup(urn);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(urn, result.Id);
            Assert.Equal("Test One", result.Name);
            Assert.Equal("Testing", result.LookupType);
        }

        [Fact]
        public void GetLookup_ShouldReturnNull_WhenUrnDoesNotExist()
        {
            // Arrange
            var key = "99999";
            _mockRepo.Setup(r => r.GetLookup(key))
                     .Returns(new Lookup());

            // Act
            var result = _service.GetLookup(key);

            // Assert
            Assert.Equal(string.Empty, result.Name);
        }

        [Fact]
        public void GetLookup_ShouldThrowException_WhenRepositoryThrows()
        {
            // Arrange
            var urn = "error";
            _mockRepo.Setup(r => r.GetLookup(urn))
                     .Throws(new Exception("Database error"));

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _service.GetLookup(urn));
            Assert.Equal("Database error", ex.Message);
        }
    }
}
