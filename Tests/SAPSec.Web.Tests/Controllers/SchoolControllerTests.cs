using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
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
            // Identifiers
            Name = DataWithAvailability<string>.Available(name),
            Urn = DataWithAvailability<string>.Available(urn),
            DfENumber = DataWithAvailability<string>.Available("373/1234"),
            Ukprn = DataWithAvailability<string>.Available("10012345"),

            // Location
            Address = DataWithAvailability<string>.Available("123 Test Street, Sheffield, S1 1AA"),
            LocalAuthorityName = DataWithAvailability<string>.Available("Sheffield"),
            LocalAuthorityCode = DataWithAvailability<string>.Available("373"),
            Region = DataWithAvailability<string>.Available("Yorkshire"),
            UrbanRuralDescription = DataWithAvailability<string>.Available("Urban"),

            // School characteristics
            AgeRangeLow = DataWithAvailability<int>.Available(11),
            AgeRangeHigh = DataWithAvailability<int>.Available(18),
            GenderOfEntry = DataWithAvailability<string>.Available("Mixed"),
            PhaseOfEducation = DataWithAvailability<string>.Available("Secondary"),
            SchoolType = DataWithAvailability<string>.Available("Academy converter"),
            AdmissionsPolicy = DataWithAvailability<string>.Available("Non-selective"),
            ReligiousCharacter = DataWithAvailability<string>.Available("None"),

            // Governance
            GovernanceStructure = DataWithAvailability<GovernanceType>.Available(GovernanceType.MultiAcademyTrust),
            AcademyTrustName = DataWithAvailability<string>.Available("Test Trust"),
            AcademyTrustId = DataWithAvailability<string>.Available("5001"),

            // Provisions
            HasNurseryProvision = DataWithAvailability<bool>.Available(false),
            HasSixthForm = DataWithAvailability<bool>.Available(true),
            HasSenUnit = DataWithAvailability<bool>.Available(false),
            HasResourcedProvision = DataWithAvailability<bool>.Available(false),

            // Contact
            HeadteacherName = DataWithAvailability<string>.Available("Mr John Smith"),
            Website = DataWithAvailability<string>.Available("https://www.testacademy.org.uk"),
            Telephone = DataWithAvailability<string>.Available("0114 123 4567"),
            Email = DataWithAvailability<string>.NotAvailable()
        };
    }

    #endregion
}