using Castle.Core.Logging;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Core.Model.Search;
using SAPSec.Core.Services;
using SAPSec.Infrastructure.Entities;
using SAPSec.Web.Controllers;
using Xunit;

namespace SAPSec.Web.Tests.Controllers;

public class SchoolControllerTests
{
    private readonly Mock<ISchoolDetailsService> _schoolDetailsServiceMock;
    private readonly Mock<ILogger<SchoolController>> _loggerMock;
    private readonly SchoolController _sut;

    public SchoolControllerTests()
    {
        _schoolDetailsServiceMock = new Mock<ISchoolDetailsService>();
        _loggerMock = new Mock<ILogger<SchoolController>>();
        _sut = new SchoolController(_schoolDetailsServiceMock.Object, _loggerMock.Object);
    }

    #region Index Action Tests

    [Fact]
    public void Index_ValidUrn_ReturnsViewWithSchoolDetails()
    {
        // Arrange
        var urn = "123456";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");

        _schoolDetailsServiceMock
            .Setup(x => x.TryGetByUrn(urn))
            .Returns(schoolDetails);

        // Act
        var result = _sut.Index(urn);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<SchoolDetails>().Subject;
        model.Urn.Value.Should().Be(urn);
        model.Name.Value.Should().Be("Test Academy");
    }

    [Fact]
    public void Index_ValidUrn_SetsBreadcrumbInViewData()
    {
        // Arrange
        var urn = "123456";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");

        _schoolDetailsServiceMock
            .Setup(x => x.TryGetByUrn(urn))
            .Returns(schoolDetails);

        // Act
        var result = _sut.Index(urn);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        _sut.ViewData["BreadcrumbNode"].Should().NotBeNull();
    }

    [Fact]
    public void Index_SchoolNotFound_ReturnsNotFound()
    {
        // Arrange
        var urn = "999999";

        _schoolDetailsServiceMock
            .Setup(x => x.TryGetByUrn(urn))
            .Returns((SchoolDetails?)null);

        // Act
        var result = _sut.Index(urn);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public void Index_SchoolNotFound_LogsInformation()
    {
        // Arrange
        var urn = "999999";

        _schoolDetailsServiceMock
            .Setup(x => x.TryGetByUrn(urn))
            .Returns((SchoolDetails?)null);

        // Act
        _sut.Index(urn);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(urn)),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Index_InvalidUrn_ReturnsNotFound(string? urn)
    {
        // Arrange
        _schoolDetailsServiceMock
            .Setup(x => x.TryGetByUrn(It.IsAny<string>()))
            .Returns((SchoolDetails?)null);

        // Act
        var result = _sut.Index(urn!);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public void Index_ServiceCalled_WithCorrectUrn()
    {
        // Arrange
        var urn = "123456";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");

        _schoolDetailsServiceMock
            .Setup(x => x.TryGetByUrn(urn))
            .Returns(schoolDetails);

        // Act
        _sut.Index(urn);

        // Assert
        _schoolDetailsServiceMock.Verify(x => x.TryGetByUrn(urn), Times.Once);
    }

    [Fact]
    public void Index_ReturnsDefaultView()
    {
        // Arrange
        var urn = "123456";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");

        _schoolDetailsServiceMock
            .Setup(x => x.TryGetByUrn(urn))
            .Returns(schoolDetails);

        // Act
        var result = _sut.Index(urn);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().BeNull(); // Default view
    }

    #endregion

    #region Helper Methods

    private static SchoolDetails CreateTestSchoolDetails(string urn, string name)
    {
        return new SchoolDetails
        {
            // Identifiers - Using new DataAvailability factory
            Name = DataAvailability.Available(name),
            Urn = DataAvailability.Available(urn),
            DfENumber = DataAvailability.Available("373/1234"),
            Ukprn = DataAvailability.Available("10012345"),

            // Location
            Address = DataAvailability.Available("123 Test Street, Sheffield, S1 1AA"),
            LocalAuthorityName = DataAvailability.Available("Sheffield"),
            LocalAuthorityCode = DataAvailability.Available("373"),
            Region = DataAvailability.Available("Yorkshire"),
            UrbanRuralDescription = DataAvailability.Available("Urban"),

            // School characteristics
            AgeRangeLow = DataAvailability.Available(11),
            AgeRangeHigh = DataAvailability.Available(18),
            GenderOfEntry = DataAvailability.Available("Mixed"),
            PhaseOfEducation = DataAvailability.Available("Secondary"),
            SchoolType = DataAvailability.Available("Academy converter"),
            AdmissionsPolicy = DataAvailability.Available("Non-selective"),
            ReligiousCharacter = DataAvailability.Available("None"),

            // Governance
            GovernanceStructure = DataAvailability.Available(GovernanceType.MultiAcademyTrust),
            AcademyTrustName = DataAvailability.Available("Test Trust"),
            AcademyTrustId = DataAvailability.Available("5001"),

            // Provisions
            HasNurseryProvision = DataAvailability.Available(false),
            HasSixthForm = DataAvailability.Available(true),
            HasSenUnit = DataAvailability.Available(false),
            HasResourcedProvision = DataAvailability.Available(false),

            // Contact
            HeadteacherName = DataAvailability.Available("Mr John Smith"),
            Website = DataAvailability.Available("https://www.testacademy.org.uk"),
            Telephone = DataAvailability.Available("0114 123 4567"),
            Email = DataAvailability.NotAvailable<string>()
        };
    }

    #endregion
}