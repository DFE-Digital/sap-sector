using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Features.Attendance;
using SAPSec.Core.Features.Attendance.UseCases;
using SAPSec.Core.Features.Ks4HeadlineMeasures;
using SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Core.Model.Generated;
using SAPSec.Web.Controllers;

namespace SAPSec.Web.Tests.Controllers;

public class SchoolControllerTests
{
    #region Fields

    private readonly Mock<ISchoolDetailsService> _schoolDetailsServiceMock;
    private readonly Mock<IAbsenceRepository> _absenceRepositoryMock;
    private readonly Mock<IEstablishmentRepository> _establishmentRepositoryMock;
    private readonly Mock<IKs4PerformanceRepository> _ks4PerformanceRepositoryMock;
    private readonly Mock<IKs4DestinationsRepository> _ks4DestinationsRepositoryMock;
    private readonly Mock<ISimilarSchoolsSecondaryRepository> _similarSchoolsRepositoryMock;
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
        _loggerMock = new Mock<ILogger<SchoolController>>();

        var getAttendanceMeasures = new GetAttendanceMeasures(
            _absenceRepositoryMock.Object,
            _establishmentRepositoryMock.Object);
        var getSchoolKs4HeadlineMeasures = new GetSchoolKs4HeadlineMeasures(
            _ks4PerformanceRepositoryMock.Object,
            _ks4DestinationsRepositoryMock.Object,
            _schoolDetailsServiceMock.Object,
            _establishmentRepositoryMock.Object,
            _similarSchoolsRepositoryMock.Object);

        _sut = new SchoolController(
            _schoolDetailsServiceMock.Object,
            getSchoolKs4HeadlineMeasures,
            getAttendanceMeasures,
            _loggerMock.Object);
    }

    #endregion

    #region Index Tests

    [Fact]
    public async Task Index_ValidUrn_ReturnsViewWithSchoolDetails()
    {
        var urn = "123456";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");

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

        _schoolDetailsServiceMock
            .Setup(x => x.GetByUrnAsync(urn))
            .ReturnsAsync(schoolDetails);

        await _sut.Index(urn);

        _sut.ViewData["BreadcrumbNode"].Should().NotBeNull();
    }

    [Fact]
    public async Task Index_ServiceCalled_WithCorrectUrn()
    {
        var urn = "123456";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");

        _schoolDetailsServiceMock
            .Setup(x => x.GetByUrnAsync(urn))
            .ReturnsAsync(schoolDetails);

        await _sut.Index(urn);

        _schoolDetailsServiceMock.Verify(x => x.GetByUrnAsync(urn), Times.Once);
    }

    [Fact]
    public async Task Index_ReturnsDefaultView()
    {
        var urn = "123456";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");

        _schoolDetailsServiceMock
            .Setup(x => x.GetByUrnAsync(urn))
            .ReturnsAsync(schoolDetails);

        var result = await _sut.Index(urn);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.ViewName.Should().BeNull();
    }

    #endregion

    #region KS4 Headline Measures Tests

    [Fact]
    public async Task Ks4HeadlineMeasures_ValidUrn_ReturnsViewWithExpectedModel()
    {
        var urn = "123456";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");

        _schoolDetailsServiceMock
            .Setup(x => x.GetByUrnAsync(urn))
            .ReturnsAsync(schoolDetails);

        _similarSchoolsRepositoryMock
            .Setup(x => x.GetSimilarSchoolsGroupAsync(urn))
            .ReturnsAsync(Array.Empty<SimilarSchoolsSecondaryGroupsEntry>());

        _ks4PerformanceRepositoryMock
            .Setup(x => x.GetByUrnAsync(urn))
            .ReturnsAsync(new Ks4PerformanceData(urn, null, null, null));

        _ks4DestinationsRepositoryMock
            .Setup(x => x.GetByUrnAsync(urn))
            .ReturnsAsync(new Ks4DestinationsData(urn, null, null, null));

        var result = await _sut.Ks4HeadlineMeasures(urn);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<SAPSec.Web.ViewModels.Ks4HeadlineMeasuresPageViewModel>();
    }

    #endregion

    #region Attendance Tests

    [Fact]
    public async Task Attendance_ValidUrn_ReturnsViewWithExpectedModel()
    {
        var urn = "123456";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");

        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentAsync(urn))
            .ReturnsAsync(new Establishment { URN = urn, LAId = "373" });
        _schoolDetailsServiceMock
            .Setup(x => x.GetByUrnAsync(urn))
            .ReturnsAsync(schoolDetails);
        _absenceRepositoryMock
            .Setup(x => x.GetByUrnAsync(urn))
            .ReturnsAsync(new AbsenceData(
                urn,
                new EstablishmentAbsence(),
                new LAAbsence(),
                new EnglandAbsence()));

        var result = await _sut.Attendance(urn);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<SAPSec.Web.ViewModels.SchoolAttendancePageViewModel>().Subject;
        model.SchoolDetails.Urn.Should().Be(urn);
        model.SchoolDetails.Name.Should().Be("Test Academy");
    }

    [Fact]
    public async Task AttendanceData_ValidUrn_ReturnsPayload()
    {
        var urn = "123456";

        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentAsync(urn))
            .ReturnsAsync(new Establishment { URN = urn, LAId = "373" });
        _absenceRepositoryMock
            .Setup(x => x.GetByUrnAsync(urn))
            .ReturnsAsync(new AbsenceData(
                urn,
                new EstablishmentAbsence
                {
                    Abs_Tot_Est_Current_Pct = "5.1",
                    Abs_Tot_Est_Previous_Pct = "5.0",
                    Abs_Tot_Est_Previous2_Pct = "4.9",
                    Abs_Persistent_Est_Current_Pct = "16.2",
                    Abs_Persistent_Est_Previous_Pct = "16.0",
                    Abs_Persistent_Est_Previous2_Pct = "15.8"
                },
                new LAAbsence
                {
                    Abs_Tot_LA_Current_Pct = "4.8",
                    Abs_Tot_LA_Previous_Pct = "4.7",
                    Abs_Tot_LA_Previous2_Pct = "4.6",
                    Abs_Persistent_LA_Current_Pct = "15.2",
                    Abs_Persistent_LA_Previous_Pct = "15.0",
                    Abs_Persistent_LA_Previous2_Pct = "14.8"
                },
                new EnglandAbsence
                {
                    Abs_Tot_Eng_Current_Pct = "4.7",
                    Abs_Tot_Eng_Previous_Pct = "4.6",
                    Abs_Tot_Eng_Previous2_Pct = "4.5",
                    Abs_Persistent_Eng_Current_Pct = "15.1",
                    Abs_Persistent_Eng_Previous_Pct = "14.9",
                    Abs_Persistent_Eng_Previous2_Pct = "14.7"
                }));

        var result = await _sut.AttendanceData(urn, "overall");

        var json = result.Should().BeOfType<JsonResult>().Subject;
        json.Value.Should().NotBeNull();
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
            HasSenUnit = DataWithAvailability.Available(false),
            HasResourcedProvision = DataWithAvailability.Available(false),
            HeadteacherName = DataWithAvailability.Available("Mr John Smith"),
            Website = DataWithAvailability.Available("https://www.testacademy.org.uk"),
            Telephone = DataWithAvailability.Available("0114 123 4567"),
            Email = DataWithAvailability.NotAvailable<string>()
        };
    }

    #endregion
}
