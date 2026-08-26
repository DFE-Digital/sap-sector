using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Features.Measures.Attendance;
using SAPSec.Core.Features.Measures.Secondary;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Data.Repositories;
using SAPSec.Web.Areas.Secondary.Controllers;
using SAPSec.Web.Constants;
using SAPSec.Web.Services;

namespace SAPSec.Web.Tests.Deprecated.Controllers;

public class SchoolControllerTests
{
    #region Fields

    private readonly Mock<ISchoolDetailsService> _schoolDetailsServiceMock;
    private readonly Mock<IAbsenceRepository> _absenceRepositoryMock;
    private readonly Mock<IEstablishmentRepository> _establishmentRepositoryMock;
    private readonly Mock<IKs4PerformanceRepository> _ks4PerformanceRepositoryMock;
    private readonly Mock<IKs4DestinationsRepository> _ks4DestinationsRepositoryMock;
    private readonly Mock<ISimilarSchoolsSecondaryRepository> _similarSchoolsRepositoryMock;
    private readonly Mock<IRequestSchoolAccessor> _requestSchoolAccessorMock;
    private readonly Mock<ILogger<SchoolController>> _loggerMock;
    private readonly SchoolController _sut;

    #endregion

    #region Constructor

    public SchoolControllerTests()
    {
        _schoolDetailsServiceMock = new Mock<ISchoolDetailsService>();
        _absenceRepositoryMock = new Mock<IAbsenceRepository>();
        _establishmentRepositoryMock = new Mock<IEstablishmentRepository>();
        _ks4PerformanceRepositoryMock = new Mock<IKs4PerformanceRepository>();
        _ks4DestinationsRepositoryMock = new Mock<IKs4DestinationsRepository>();
        _similarSchoolsRepositoryMock = new Mock<ISimilarSchoolsSecondaryRepository>();
        _requestSchoolAccessorMock = new Mock<IRequestSchoolAccessor>();
        _loggerMock = new Mock<ILogger<SchoolController>>();

        var getSchoolKs4HeadlineMeasuresUseCase = new GetSchoolKs4HeadlineMeasuresUseCase(
            _establishmentRepositoryMock.Object,
            _similarSchoolsRepositoryMock.Object,
            _ks4PerformanceRepositoryMock.Object,
            _ks4DestinationsRepositoryMock.Object);
        var getSchoolKs4CoreSubjectsUseCase = new GetSchoolKs4CoreSubjectsMeasuresUseCase(
            _establishmentRepositoryMock.Object,
            _similarSchoolsRepositoryMock.Object,
            _ks4PerformanceRepositoryMock.Object);
        var getSchoolAttendanceMeasuresUseCase = new GetSchoolAttendanceMeasuresUseCase(
            _establishmentRepositoryMock.Object,
            _absenceRepositoryMock.Object);

        _sut = new SchoolController(
            getSchoolKs4HeadlineMeasuresUseCase,
            getSchoolKs4CoreSubjectsUseCase,
            getSchoolAttendanceMeasuresUseCase,
            _requestSchoolAccessorMock.Object,
            _loggerMock.Object);
    }

    #endregion

    #region Index Tests

    [Fact]
    public async Task Index_ValidUrn_ReturnsViewWithSchoolDetails()
    {
        var urn = "123456";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");

        _requestSchoolAccessorMock
            .Setup(x => x.GetAsync(It.IsAny<HttpContext>(), urn))
            .ReturnsAsync(schoolDetails);
        _schoolDetailsServiceMock
            .Setup(x => x.GetByUrnAsync(urn))
            .ReturnsAsync(schoolDetails);

        var result = await _sut.Index(urn);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<SchoolDetails>().Subject;
        model.Urn.Should().Be(urn);
        model.Name.Should().Be("Test Academy");
    }

    [Fact]
    public async Task Index_ValidUrn_SetsBreadcrumbInViewData()
    {
        var urn = "123456";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");

        _requestSchoolAccessorMock
            .Setup(x => x.GetAsync(It.IsAny<HttpContext>(), urn))
            .ReturnsAsync(schoolDetails);

        await _sut.Index(urn);

        _sut.ViewData[ViewDataKeys.BreadcrumbNode].Should().NotBeNull();
    }

    [Fact]
    public async Task Index_ServiceCalled_WithCorrectUrn()
    {
        var urn = "123456";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");

        _requestSchoolAccessorMock
            .Setup(x => x.GetAsync(It.IsAny<HttpContext>(), urn))
            .ReturnsAsync(schoolDetails);

        await _sut.Index(urn);

        _requestSchoolAccessorMock.Verify(x => x.GetAsync(It.IsAny<HttpContext>(), urn), Times.Once);
    }

    [Fact]
    public async Task Index_ReturnsDefaultView()
    {
        var urn = "123456";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");

        _requestSchoolAccessorMock
            .Setup(x => x.GetAsync(It.IsAny<HttpContext>(), urn))
            .ReturnsAsync(schoolDetails);

        var result = await _sut.Index(urn);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().BeNull();
    }

    #endregion

    #region Test Data Builders

    private static SchoolDetails CreateTestSchoolDetails(string urn, string name)
    {
        return new SchoolDetails
        {
            Name = name,
            Urn = urn,
            DfENumber = DataWithAvailability.Available("373/1234"),
            Ukprn = DataWithAvailability.Available("10012345"),
            Address = DataWithAvailability.Available("123 Test Street, Sheffield, S1 1AA"),
            LocalAuthorityName = DataWithAvailability.Available("Sheffield"),
            LocalAuthorityCode = DataWithAvailability.Available("373"),
            Region = DataWithAvailability.Available("Yorkshire"),
            UrbanRuralDescription = DataWithAvailability.Available("Urban"),
            AgeRangeLow = DataWithAvailability.Available(11),
            AgeRangeHigh = DataWithAvailability.Available(18),
            GenderOfEntry = DataWithAvailability.Available("Mixed"),
            PhaseOfEducation = DataWithAvailability.Available("Secondary"),
            SchoolType = DataWithAvailability.Available("Academy converter"),
            AdmissionsPolicy = DataWithAvailability.Available("Non-selective"),
            ReligiousCharacter = DataWithAvailability.Available("None"),
            GovernanceStructure = DataWithAvailability.Available(GovernanceType.MultiAcademyTrust),
            AcademyTrustName = DataWithAvailability.Available("Test Trust"),
            AcademyTrustId = DataWithAvailability.Available("5001"),
            HasNurseryProvision = DataWithAvailability.Available(false),
            HasSixthForm = DataWithAvailability.Available(true),
            HasSenUnit = DataWithAvailability.Available(true),
            HasResourcedProvision = DataWithAvailability.Available(true),
            HeadteacherName = DataWithAvailability.Available("Mr John Smith"),
            Website = DataWithAvailability.Available("https://www.testacademy.org.uk"),
            Telephone = DataWithAvailability.Available("0114 123 4567"),
            Email = DataWithAvailability.NotAvailable<string>()
        };
    }

    #endregion
}
