using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Features.Attendance;
using SAPSec.Core.Features.Attendance.UseCases;
using SAPSec.Core.Features.Ks4CoreSubjects.UseCases;
using SAPSec.Core.Features.Ks4HeadlineMeasures;
using SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Core.Model.Generated;
using SAPSec.Web.Controllers;
using System.Text.Json;

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
            _establishmentRepositoryMock.Object,
            _similarSchoolsRepositoryMock.Object);
        var getSchoolKs4HeadlineMeasures = new GetSchoolKs4HeadlineMeasures(
            _ks4PerformanceRepositoryMock.Object,
            _ks4DestinationsRepositoryMock.Object,
            _schoolDetailsServiceMock.Object,
            _establishmentRepositoryMock.Object,
            _similarSchoolsRepositoryMock.Object);
        var getSchoolKs4CoreSubjects = new GetSchoolKs4CoreSubjects(
            _ks4PerformanceRepositoryMock.Object,
            _schoolDetailsServiceMock.Object,
            _establishmentRepositoryMock.Object,
            _similarSchoolsRepositoryMock.Object);
        var getFilteredSchoolKs4CoreSubject = new GetFilteredSchoolKs4CoreSubject(
            _ks4PerformanceRepositoryMock.Object,
            _schoolDetailsServiceMock.Object,
            _establishmentRepositoryMock.Object,
            _similarSchoolsRepositoryMock.Object);

        _sut = new SchoolController(
            _schoolDetailsServiceMock.Object,
            getSchoolKs4HeadlineMeasures,
            getSchoolKs4CoreSubjects,
            getFilteredSchoolKs4CoreSubject,
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

    #region KS4 Core Subjects Tests

    [Fact]
    public async Task Ks4CoreSubjects_ValidUrn_ReturnsViewWithExpectedModel()
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
            .ReturnsAsync(new Ks4PerformanceData(urn, new EstablishmentPerformance(), new LAPerformance(), new EnglandPerformance()));

        var result = await _sut.Ks4CoreSubjects(urn);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<SAPSec.Web.ViewModels.Ks4CoreSubjectsPageViewModel>();
    }

    [Fact]
    public async Task Ks4CoreSubjectsData_ValidUrn_ReturnsPayload()
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
            .ReturnsAsync(new Ks4PerformanceData(
                urn,
                new EstablishmentPerformance
                {
                    EngLang49_Sum_Est_Current_Pct = "52",
                    EngLang49_Sum_Est_Previous_Pct = "51",
                    EngLang49_Sum_Est_Previous2_Pct = "50"
                },
                new LAPerformance
                {
                    EngLang49_Tot_LA_Current_Pct = "60",
                    EngLang49_Tot_LA_Previous_Pct = "59",
                    EngLang49_Tot_LA_Previous2_Pct = "58"
                },
                new EnglandPerformance
                {
                    EngLang49_Tot_Eng_Current_Pct = "61",
                    EngLang49_Tot_Eng_Previous_Pct = "60",
                    EngLang49_Tot_Eng_Previous2_Pct = "59"
                }));

        var result = await _sut.Ks4CoreSubjectsData(urn, "english-language", "4");

        var json = result.Should().BeOfType<JsonResult>().Subject;
        json.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Ks4CoreSubjectsData_InvalidFilter_ReturnsBadRequest()
    {
        var result = await _sut.Ks4CoreSubjectsData("123456", "unknown-subject", "4");

        result.Should().BeOfType<BadRequestObjectResult>();
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
        _similarSchoolsRepositoryMock
            .Setup(x => x.GetSimilarSchoolsGroupAsync(urn))
            .ReturnsAsync(Array.Empty<SimilarSchoolsSecondaryGroupsEntry>());

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
        _similarSchoolsRepositoryMock
            .Setup(x => x.GetSimilarSchoolsGroupAsync(urn))
            .ReturnsAsync(
            [
                new SimilarSchoolsSecondaryGroupsEntry { URN = urn, NeighbourURN = "200001" },
                new SimilarSchoolsSecondaryGroupsEntry { URN = urn, NeighbourURN = "200002" }
            ]);
        _absenceRepositoryMock
            .Setup(x => x.GetByUrnsAsync(It.Is<IEnumerable<string>>(urns => urns.SequenceEqual(new[] { "200001", "200002" }))))
            .ReturnsAsync(
            [
                new AbsenceData(
                    "200001",
                    new EstablishmentAbsence
                    {
                        Abs_Tot_Est_Current_Pct = "5.5",
                        Abs_Tot_Est_Previous_Pct = "5.4",
                        Abs_Tot_Est_Previous2_Pct = "5.3",
                        Abs_Persistent_Est_Current_Pct = "16.5",
                        Abs_Persistent_Est_Previous_Pct = "16.3",
                        Abs_Persistent_Est_Previous2_Pct = "16.1"
                    },
                    null,
                    null),
                new AbsenceData(
                    "200002",
                    new EstablishmentAbsence
                    {
                        Abs_Tot_Est_Current_Pct = "5.3",
                        Abs_Tot_Est_Previous_Pct = "5.2",
                        Abs_Tot_Est_Previous2_Pct = "5.1",
                        Abs_Persistent_Est_Current_Pct = "16.1",
                        Abs_Persistent_Est_Previous_Pct = "15.9",
                        Abs_Persistent_Est_Previous2_Pct = "15.7"
                    },
                    null,
                    null)
            ]);

        var result = await _sut.AttendanceData(urn, "overall");

        var json = result.Should().BeOfType<JsonResult>().Subject;
        var payload = JsonSerializer.Serialize(json.Value);
        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;

        root.GetProperty("bar").GetArrayLength().Should().Be(4);
        root.GetProperty("line").GetProperty("similarSchools").GetArrayLength().Should().Be(3);
        root.GetProperty("table").GetProperty("similarSchools").GetArrayLength().Should().Be(4);
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
