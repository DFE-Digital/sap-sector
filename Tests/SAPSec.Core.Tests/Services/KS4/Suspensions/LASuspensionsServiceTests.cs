using Moq;
using SAPSec.Core.Model.KS4.Suspensions;
using SAPSec.Core.Interfaces.Repositories.KS4.Suspensions;
using SAPSec.Core.Services.KS4.Suspensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAPSec.Core.Tests.Services.KS4.Suspensions
{
    public class LASuspensionsServiceTests
    {
        private readonly Mock<ILASuspensionsRepository> _mockRepo;
        private readonly LASuspensionsService _service;

        public LASuspensionsServiceTests()
        {
            _mockRepo = new Mock<ILASuspensionsRepository>();
            _service = new LASuspensionsService(_mockRepo.Object);
        }

        [Fact]
        public void GetAllLASuspensions_ShouldReturnAllItems()
        {
            // Arrange
            var expectedDestinationss = new List<LASuspensions>
        {
            new LASuspensions { Id = "100",  Sus_Tot_LA_Current_Pct = 99.99},
            new LASuspensions { Id = "101", Sus_Tot_LA_Current_Pct = 90.00}
        };

            _mockRepo.Setup(r => r.GetAllLASuspensions())
                     .Returns(expectedDestinationss);

            // Act
            var result = _service.GetAllLASuspensions();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, a => a.Sus_Tot_LA_Current_Pct == 99.99);
            Assert.Contains(result, a => a.Sus_Tot_LA_Current_Pct == 90.00);
        }

        [Fact]
        public void GetAllLASuspensions_ShouldReturnEmpty_WhenNoData()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetAllLASuspensions())
                     .Returns(new List<LASuspensions>());

            // Act
            var result = _service.GetAllLASuspensions();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }


        [Fact]
        public void GetLASuspensions_ShouldReturnCorrectItem_WhenUrnExists()
        {
            // Arrange
            var urn = "100";
            var expectedDestinations = new LASuspensions { Id = urn, Sus_Tot_LA_Current_Pct = 100 };

            _mockRepo.Setup(r => r.GetLASuspensions(urn))
                     .Returns(expectedDestinations);

            // Act
            var result = _service.GetLASuspensions(urn);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(urn, result.Id);
            Assert.Equal(100, result.Sus_Tot_LA_Current_Pct);
        }

        [Fact]
        public void GetLASuspensions_ShouldReturnNull_WhenUrnDoesNotExist()
        {
            // Arrange
            var urn = "99999";
            _mockRepo.Setup(r => r.GetLASuspensions(urn))
                     .Returns(new LASuspensions());

            // Act
            var result = _service.GetLASuspensions(urn);

            // Assert
            Assert.Null(result.Sus_Tot_LA_Current_Pct);
        }

        [Fact]
        public void GetLASuspensions_ShouldThrowException_WhenRepositoryThrows()
        {
            // Arrange
            var urn = "error";
            _mockRepo.Setup(r => r.GetLASuspensions(urn))
                     .Throws(new Exception("Database error"));

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _service.GetLASuspensions(urn));
            Assert.Equal("Database error", ex.Message);
        }
    }
}
