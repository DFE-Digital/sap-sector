using Moq;
using SAPSec.Core.Model.KS4.Suspensions;
using SAPSec.Core.Interfaces.Repositories.KS4.Suspensions;
using SAPSec.Core.Services.KS4.Suspensions;

namespace SAPSec.Core.Tests.Services.KS4.Suspensions
{
    public class EnglandSuspensionsServiceTests
    {
        private readonly Mock<IEnglandSuspensionsRepository> _mockRepo;
        private readonly EnglandSuspensionsService _service;

        public EnglandSuspensionsServiceTests()
        {
            _mockRepo = new Mock<IEnglandSuspensionsRepository>();
            _service = new EnglandSuspensionsService(_mockRepo.Object);
        }

        [Fact]
        public void GetAllEnglandSuspensions_ShouldReturnAllItems()
        {
            // Arrange
            var expectedAbsences =
            new EnglandSuspensions { Sus_Tot_Eng_Current_Pct = 99.99 };

            _mockRepo.Setup(r => r.GetEnglandSuspensions())
                         .Returns(expectedAbsences);

            // Act
            var result = _service.GetEnglandSuspensions();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(99.99, result.Sus_Tot_Eng_Current_Pct);
        }

        [Fact]
        public void GetAllEnglandSuspensions_ShouldReturnEmpty_WhenNoData()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetEnglandSuspensions())
                     .Returns(new EnglandSuspensions());

            // Act
            var result = _service.GetEnglandSuspensions();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void GetEnglandSuspensions_ShouldThrowException_WhenRepositoryThrows()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetEnglandSuspensions())
                     .Throws(new Exception("Database error"));

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _service.GetEnglandSuspensions());
            Assert.Equal("Database error", ex.Message);
        }

    }
}
