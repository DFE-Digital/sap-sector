using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Features.Ks4HeadlineMeasures;
using SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Web.Controllers;

namespace SAPSec.Web.Tests.Controllers;

public class SchoolControllerTests
{
    private readonly Mock<ISchoolDetailsService> _schoolDetailsServiceMock;
    private readonly Mock<IKs4PerformanceRepository> _ks4PerformanceRepositoryMock;
    private readonly Mock<ILogger<SchoolController>> _loggerMock;
    private readonly SchoolController _sut;

    public SchoolControllerTests()
    {
        _schoolDetailsServiceMock = new Mock<ISchoolDetailsService>();
        _ks4PerformanceRepositoryMock = new Mock<IKs4PerformanceRepository>();
        _loggerMock = new Mock<ILogger<SchoolController>>();

        var getKs4HeadlineMeasures = new GetKs4HeadlineMeasures(
            _ks4PerformanceRepositoryMock.Object,
            _schoolDetailsServiceMock.Object);

        _sut = new SchoolController(_schoolDetailsServiceMock.Object, getKs4HeadlineMeasures, _loggerMock.Object);
    }

    #region Index Action Tests

    [Fact]
    public async Task Index_ValidUrn_ReturnsViewWithSchoolDetails()
    {
        // Arrange
        var urn = "123456";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");

        _schoolDetailsServiceMock
            .Setup(x => x.GetByUrnAsync(urn))
            .ReturnsAsync(schoolDetails);

        // Act
        var result = await _sut.Index(urn);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<SchoolDetails>().Subject;
        model.Urn.Should().Be(urn);
        model.Name.Should().Be("Test Academy");
    }

    [Fact]
    public async Task Index_ValidUrn_SetsBreadcrumbInViewData()
    {
        // Arrange
        var urn = "123456";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");

        _schoolDetailsServiceMock
            .Setup(x => x.GetByUrnAsync(urn))
            .ReturnsAsync(schoolDetails);

        // Act
        var result = await _sut.Index(urn);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        _sut.ViewData["BreadcrumbNode"].Should().NotBeNull();
    }

    [Fact]
    public async Task Index_ServiceCalled_WithCorrectUrn()
    {
        // Arrange
        var urn = "123456";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");

        _schoolDetailsServiceMock
            .Setup(x => x.GetByUrnAsync(urn))
            .ReturnsAsync(schoolDetails);

        // Act
        await _sut.Index(urn);

        // Assert
        _schoolDetailsServiceMock.Verify(x => x.GetByUrnAsync(urn), Times.Once);
    }

    [Fact]
    public async Task Index_ReturnsDefaultView()
    {
        // Arrange
        var urn = "123456";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");

        _schoolDetailsServiceMock
            .Setup(x => x.GetByUrnAsync(urn))
            .ReturnsAsync(schoolDetails);

        // Act
        var result = await _sut.Index(urn);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().BeNull(); // Default view
    }

    #endregion

    #region KS4 Headline Measures Action Tests

    [Fact]
    public async Task Ks4HeadlineMeasures_ValidUrn_ReturnsViewWithExpectedModel()
    {
        // Arrange
        var urn = "123456";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");

        _schoolDetailsServiceMock
            .Setup(x => x.GetByUrnAsync(urn))
            .ReturnsAsync(schoolDetails);

        _ks4PerformanceRepositoryMock
            .Setup(x => x.GetByUrnAsync(urn))
            .ReturnsAsync(new Ks4HeadlineMeasuresData(null, null, null, null, null, null));

        // Act
        var result = await _sut.Ks4HeadlineMeasures(urn);

        // Assert
        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<SAPSec.Web.ViewModels.Ks4HeadlineMeasuresPageViewModel>();
    }

    #endregion

    #region Helper Methods

    private static SchoolDetails CreateTestSchoolDetails(string urn, string name)
    {
        return new SchoolDetails
        {
            // Identifiers - Using new DataAvailability factory
            Name = name,
            Urn = urn,
            DfENumber = DataWithAvailability.Available("373/1234"),
            Ukprn = DataWithAvailability.Available("10012345"),

            // Location
            Address = DataWithAvailability.Available("123 Test Street, Sheffield, S1 1AA"),
            LocalAuthorityName = DataWithAvailability.Available("Sheffield"),
            LocalAuthorityCode = DataWithAvailability.Available("373"),
            Region = DataWithAvailability.Available("Yorkshire"),
            UrbanRuralDescription = DataWithAvailability.Available("Urban"),

            // School characteristics
            AgeRangeLow = DataWithAvailability.Available(11),
            AgeRangeHigh = DataWithAvailability.Available(18),
            GenderOfEntry = DataWithAvailability.Available("Mixed"),
            PhaseOfEducation = DataWithAvailability.Available("Secondary"),
            SchoolType = DataWithAvailability.Available("Academy converter"),
            AdmissionsPolicy = DataWithAvailability.Available("Non-selective"),
            ReligiousCharacter = DataWithAvailability.Available("None"),

            // Governance
            GovernanceStructure = DataWithAvailability.Available(GovernanceType.MultiAcademyTrust),
            AcademyTrustName = DataWithAvailability.Available("Test Trust"),
            AcademyTrustId = DataWithAvailability.Available("5001"),

            // Provisions
            HasNurseryProvision = DataWithAvailability.Available(false),
            HasSixthForm = DataWithAvailability.Available(true),
            HasSenUnit = DataWithAvailability.Available(false),
            HasResourcedProvision = DataWithAvailability.Available(false),

            // Contact
            HeadteacherName = DataWithAvailability.Available("Mr John Smith"),
            Website = DataWithAvailability.Available("https://www.testacademy.org.uk"),
            Telephone = DataWithAvailability.Available("0114 123 4567"),
            Email = DataWithAvailability.NotAvailable<string>()
        };
    }

    #endregion
}
