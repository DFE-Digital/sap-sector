using Moq;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
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
    public class EstablishmentServiceTests
    {
        private readonly Mock<IEstablishmentRepository> _mockRepo;
        private readonly Mock<ILookupService> _mockLookupRepo;
        private readonly EstablishmentService _service;

        public EstablishmentServiceTests()
        {
            _mockRepo = new Mock<IEstablishmentRepository>();
            _mockLookupRepo = new Mock<ILookupService>();
            _service = new EstablishmentService(_mockRepo.Object, _mockLookupRepo.Object);
        }

        private readonly Establishment FakeEstablishmentOne = new Establishment
        {
            URN = "123456",
            EstablishmentName = "Test Establishment One",
            PhaseOfEducationName = "Primary School"
        };
        private readonly Establishment FakeEstablishmentTwo = new Establishment
        {
            URN = "456789",
            EstablishmentName = "Test Establishment Two",
            PhaseOfEducationName = "Secondary School"
        };



        [Fact]
        public void GetAllEstablishment_ShouldReturnAllItems()
        {
            // Arrange
            var expectedDestinations = new List<Establishment>
            {
                FakeEstablishmentOne,
                FakeEstablishmentTwo
            };

            _mockRepo.Setup(r => r.GetAllEstablishments())
                     .Returns(expectedDestinations);

            // Act
            var result = _service.GetAllEstablishments();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, a => a.URN == "123456");
            Assert.Contains(result, a => a.URN == "456789");
        }

        [Fact]
        public void GetAllEstablishment_ShouldReturnEmpty_WhenNoData()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetAllEstablishments())
                     .Returns(new List<Establishment>());

            // Act
            var result = _service.GetAllEstablishments();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }


        [Fact]
        public void GetEstablishment_ShouldReturnCorrectItem_WhenUrnExists()
        {
            // Arrange
            var urn = "123456";
            var expectedDestinations = FakeEstablishmentOne;

            _mockRepo.Setup(r => r.GetEstablishment(urn))
                     .Returns(expectedDestinations);

            // Act
            var result = _service.GetEstablishment(urn);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(urn, result.URN);
            Assert.Equal(FakeEstablishmentOne.EstablishmentName, result.EstablishmentName);
            Assert.Equal(FakeEstablishmentOne.PhaseOfEducationName, result.PhaseOfEducationName);
        }

        [Fact]
        public void GetEstablishment_ShouldThrowError_WhenUrnDoesNotExist()
        {
            // Arrange
            var urn = "99999";
            _mockRepo.Setup(r => r.GetEstablishment(urn))
                     .Throws(new Exception("Error in GetEstablishment"));

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _service.GetEstablishment(urn));
            Assert.Equal("Error in GetEstablishment", ex.Message);
        }

        [Fact]
        public void GetEstablishment_ShouldThrowException_WhenRepositoryThrows()
        {
            // Arrange
            var urn = "error";
            _mockRepo.Setup(r => r.GetEstablishment(urn))
                     .Throws(new Exception("Database error"));

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _service.GetEstablishment(urn));
            Assert.Equal("Database error", ex.Message);
        }
    }
}
