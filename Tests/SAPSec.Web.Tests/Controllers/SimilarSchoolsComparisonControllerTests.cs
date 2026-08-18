using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Features.Attendance.UseCases;
using SAPSec.Core.Features.Geography;
using SAPSec.Core.Features.Secondary.Ks4CoreSubjects.UseCases;
using SAPSec.Core.Features.Secondary.Ks4HeadlineMeasures_Old.UseCases;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Services;
using SAPSec.Data.Dto;
using SAPSec.Data.Dto.Absence;
using SAPSec.Data.Dto.SimilarSchools.Secondary;
using SAPSec.Data.Repositories;
using SAPSec.Web.Areas.Secondary.Controllers;
using SAPSec.Web.Constants;
using SAPSec.Web.Formatters;
using SAPSec.Web.ViewModels;
using System.Text.Json;

namespace SAPSec.Web.Tests.Deprecated.Controllers;

public class SimilarSchoolsComparisonControllerTests
{
    private readonly Mock<IEstablishmentRepository> _establishmentRepositoryMock = new();
    private readonly Mock<ISimilarSchoolsSecondaryRepository> _repoMock = new();
    private readonly Mock<IAbsenceRepository> _absenceRepositoryMock = new();
    private readonly Mock<IKs4PerformanceRepository> _ks4PerformanceRepositoryMock = new();
    private readonly Mock<IKs4DestinationsRepository> _ks4DestinationsRepositoryMock = new();
    private readonly Mock<ILogger<SimilarSchoolsComparisonController>> _loggerMock = new();
    private readonly SimilarSchoolsComparisonController _sut;

    public SimilarSchoolsComparisonControllerTests()
    {
        var schoolDetailsService = new SchoolDetailsService(
            _establishmentRepositoryMock.Object,
            new Mock<ILogger<SchoolDetailsService>>().Object);
        var getSimilarSchoolDetails = new GetSimilarSchoolDetails(
            _establishmentRepositoryMock.Object,
            _repoMock.Object,
            schoolDetailsService,
            _ks4PerformanceRepositoryMock.Object,
            _absenceRepositoryMock.Object);
        var ks4UseCase = new GetKs4HeadlineMeasures(
            _ks4PerformanceRepositoryMock.Object,
            _ks4DestinationsRepositoryMock.Object,
            schoolDetailsService);
        var ks4CoreSubjectsUseCase = new GetSchoolKs4CoreSubjects(
            _ks4PerformanceRepositoryMock.Object,
            schoolDetailsService,
            _establishmentRepositoryMock.Object,
            _repoMock.Object);
        var filteredKs4CoreSubjectsUseCase = new GetFilteredSchoolKs4CoreSubject(
            _ks4PerformanceRepositoryMock.Object,
            schoolDetailsService,
            _establishmentRepositoryMock.Object,
            _repoMock.Object);
        var attendanceUseCase = new GetAttendanceMeasures(
            _absenceRepositoryMock.Object,
            _establishmentRepositoryMock.Object,
            _repoMock.Object);

        var getCharacteristicsComparison = new GetCharacteristicsComparison(
            _repoMock.Object);

        _repoMock
            .Setup(r => r.GetGroupAsync(It.IsAny<string>()))
            .ReturnsAsync(Array.Empty<SimilarSchoolsSecondaryGroupsEntry>());

        var characteristicsFormatter = new CharacteristicsComparisonFormatter();

        _sut = new SimilarSchoolsComparisonController(
            getSimilarSchoolDetails,
            attendanceUseCase,
            ks4CoreSubjectsUseCase,
            filteredKs4CoreSubjectsUseCase,
            ks4UseCase,
            getCharacteristicsComparison,
            characteristicsFormatter,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Index_ReturnsView_WithMappedModel_AndSetsViewData()
    {
        var urn = "145327";
        var similarUrn = "142075";

        var currentSchool = CreateSchool(urn, "Main School",
            new BNGCoordinates(Easting: 430000, Northing: 380000));

        var similarSchool = CreateSchool(similarUrn, "Similar School",
            new BNGCoordinates(Easting: 431000, Northing: 381000));

        //var similarDetails = CreateSchoolDetails(similarUrn, "Similar School");

        SetupBaseDependencies(urn, similarUrn, currentSchool, similarSchool);
        SetupSecondaryValues(urn, similarUrn);
        SetupAbsence();
        SetupPerfomance();

        var result = await _sut.Index(urn, similarUrn);

        var view = result.Should().BeOfType<ViewResult>().Subject;
        var model = view.Model.Should().BeOfType<SimilarSchoolsComparisonViewModel>().Subject;

        model.Urn.Should().Be(urn);
        model.SimilarSchoolUrn.Should().Be(similarUrn);
        model.Name.Should().Be(currentSchool.EstablishmentName);
        model.SimilarSchoolName.Should().Be(similarSchool.EstablishmentName);

        _sut.ViewData[ViewDataKeys.BreadcrumbNode].Should().NotBeNull();
        _sut.ViewData["ComparisonSchool"].Should().BeSameAs(model);
    }

    [Fact]
    public async Task SchoolDetails_ReturnsFullMappedModel()
    {
        var urn = "145327";
        var similarUrn = "142075";

        var currentSchool = CreateSchool(urn, "Main School",
            new BNGCoordinates(Easting: 430000, Northing: 380000));

        var similarSchool = CreateSchool(similarUrn, "Similar School",
            new BNGCoordinates(Easting: 431000, Northing: 381000));

        //var similarDetails = CreateSchoolDetails(similarUrn, "Similar School");

        SetupBaseDependencies(urn, similarUrn, currentSchool, similarSchool);
        SetupSecondaryValues(urn, similarUrn);
        SetupAbsence();
        SetupPerfomance();

        var result = await _sut.SchoolDetails(urn, similarUrn);

        var view = result.Should().BeOfType<ViewResult>().Subject;
        view.ViewName.Should().Be("~/Views/Shared/SimilarSchoolsComparison/SchoolDetails.cshtml");

        var model = view.Model.Should().BeOfType<SimilarSchoolDetailsViewModel>().Subject;

        model.Urn.Should().Be(urn);
        model.SimilarSchoolUrn.Should().Be(similarUrn);
        model.Distance.Should().BeGreaterThan(0);
        model.SimilarSchoolDetails.Should().NotBeNull();
        model.SimilarSchoolDetails!.Urn.Should().Be(similarUrn);

        _sut.ViewData["ComparisonSchool"].Should().BeOfType<SimilarSchoolsComparisonViewModel>();
    }

    [Fact]
    public async Task Similarity_ReturnsView_WithCharacteristicsRows()
    {
        var urn = "145327";
        var similarUrn = "142075";

        var currentSchool = CreateSchool(urn, "Main School",
            new BNGCoordinates(Easting: 430000, Northing: 380000));

        var similarSchool = CreateSchool(similarUrn, "Similar School",
            new BNGCoordinates(Easting: 431000, Northing: 381000));

        //var similarDetails = CreateSchoolDetails(similarUrn, "Similar School");

        SetupBaseDependencies(urn, similarUrn, currentSchool, similarSchool);
        SetupSecondaryValues(urn, similarUrn);
        SetupAbsence();
        SetupPerfomance();

        var result = await _sut.Similarity(urn, similarUrn);

        var view = result.Should().BeOfType<ViewResult>().Subject;
        var model = view.Model.Should().BeOfType<SimilarSchoolsComparisonViewModel>().Subject;

        model.CharacteristicsRows.Should().NotBeNull();
        model.CharacteristicsRows.Should().HaveCount(9);

        model.CharacteristicsRows[0].Characteristic.Should().Be("Average KS2 reading and maths score");
        model.CharacteristicsRows[0].CurrentSchoolValue.Should().NotBeNullOrWhiteSpace();
        model.CharacteristicsRows[0].SimilarSchoolValue.Should().NotBeNullOrWhiteSpace();

        model.CharacteristicsRows.All(row => row.Similarity is null).Should().BeTrue();
    }

    [Fact]
    public async Task AttendanceData_ReturnsDefaultPayloadShape()
    {
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentAsync(It.IsAny<string>()))
            .ReturnsAsync(new Establishment { URN = "145327", LAId = "373" });

        _absenceRepositoryMock
            .Setup(x => x.GetByUrnAsync(It.IsAny<string>()))
            .ReturnsAsync(new AbsenceData(
                "145327",
                new EstablishmentAbsence
                {
                    Abs_Tot_Est_Current_Pct = "5.0",
                    Abs_Tot_Est_Previous_Pct = "5.2",
                    Abs_Tot_Est_Previous2_Pct = "5.4",
                    Abs_Persistent_Est_Current_Pct = "16.0",
                    Abs_Persistent_Est_Previous_Pct = "16.3",
                    Abs_Persistent_Est_Previous2_Pct = "16.7"
                },
                new LAAbsence(),
                new EnglandAbsence
                {
                    Abs_Tot_Secondary_Eng_Current_Pct = "4.8",
                    Abs_Tot_Secondary_Eng_Previous_Pct = "4.9",
                    Abs_Tot_Secondary_Eng_Previous2_Pct = "5.0",
                    Abs_Persistent_Secondary_Eng_Current_Pct = "15.6",
                    Abs_Persistent_Secondary_Eng_Previous_Pct = "15.8",
                    Abs_Persistent_Secondary_Eng_Previous2_Pct = "16.0"
                }));

        var result = await _sut.AttendanceData("145327", "142075");

        var json = result.Should().BeOfType<JsonResult>().Subject;
        var payload = JsonSerializer.Serialize(json.Value);
        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;

        root.GetProperty("absenceType").GetString().Should().Be("overall");
        root.GetProperty("bar").GetArrayLength().Should().Be(3);
        root.GetProperty("years").GetArrayLength().Should().Be(3);

        var table = root.GetProperty("table");
        table.GetProperty("thisSchool").GetArrayLength().Should().Be(3);
        table.GetProperty("similarSchool").GetArrayLength().Should().Be(3);
        table.GetProperty("england").GetArrayLength().Should().Be(3);
    }

    private void SetupBaseDependencies(
        string currentUrn,
        string similarUrn,
        Establishment currentSchool,
        Establishment similarSchool)
    {
        similarSchool.URN = similarUrn;
        currentSchool.URN = currentUrn;

        var group = new List<Establishment> { similarSchool }.AsReadOnly();

        _establishmentRepositoryMock
            .Setup(r => r.GetEstablishmentsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<Establishment> {
                currentSchool,
                similarSchool
            });
        _establishmentRepositoryMock
            .Setup(r => r.GetEstablishmentAsync(currentUrn))
            .ReturnsAsync(currentSchool);
        _establishmentRepositoryMock
            .Setup(r => r.GetEstablishmentAsync(similarUrn))
            .ReturnsAsync(similarSchool);
        _repoMock
            .Setup(r => r.GetGroupAsync(It.IsAny<string>()))
            .ReturnsAsync(group.Select(g => new SimilarSchoolsSecondaryGroupsEntry { URN = currentUrn, NeighbourURN = g.URN }).ToList());

        //_schoolDetailsServiceMock
        //    .Setup(s => s.GetByUrnAsync(It.IsAny<string>()))
        //    .ReturnsAsync(similarDetails);

        //_schoolDetailsServiceMock
        //    .Setup(s => s.GetByUrnAsync(It.IsAny<string>()))
        //    .ReturnsAsync(similarDetails);
    }

    private void SetupSecondaryValues(string currentUrn, string similarUrn)
    {
        var values = new List<SimilarSchoolsSecondaryValuesEntry>
        {
            new SimilarSchoolsSecondaryValuesEntry
            {
                URN = currentUrn,
                KS2MRP = "104.3",
                NumberOfPupils = "760",
                PStability = "90",
                PPPerc = "52.0",
                IdaciPupils = "0.316508",
                Polar4QuintilePupils = "3",
                PercentageStatementOrEHP = "2.105263",
                PercentSchSupport = "16.315789",
                PercentEAL = "39.525692"
            },
            new SimilarSchoolsSecondaryValuesEntry
            {
                URN = similarUrn,
                KS2MRP = "103.65",
                NumberOfPupils = "962",
                PStability = "91.7",
                PPPerc = "41.2",
                IdaciPupils = "0.351137",
                Polar4QuintilePupils = "2",
                PercentageStatementOrEHP = "3.326403",
                PercentSchSupport = "8.939709",
                PercentEAL = "61.954262"
            }
        };

        _repoMock
            .Setup(r => r.GetValuesByUrnsAsync(
                It.Is<IEnumerable<string>>(u => u.Contains(currentUrn) && u.Contains(similarUrn))))
            .ReturnsAsync(values);
    }

    private void SetupAbsence()
    {
        _absenceRepositoryMock
            .Setup(r => r.GetByUrnAsync(It.IsAny<string>()))
            .ReturnsAsync((AbsenceData?)null);
        _absenceRepositoryMock
            .Setup(r => r.GetByUrnsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<AbsenceData>());
    }

    private void SetupPerfomance()
    {
        _ks4PerformanceRepositoryMock
            .Setup(r => r.GetByUrnAsync(It.IsAny<string>()))
            .ReturnsAsync((Ks4PerformanceData?)null);
        _ks4PerformanceRepositoryMock
            .Setup(r => r.GetByUrnsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<Ks4PerformanceData>());
    }

    private static Establishment CreateSchool(string urn, string name, BNGCoordinates coordinates)
    {
        return new Establishment
        {
            URN = urn,
            EstablishmentName = name,
            Street = "123 Test Street",
            Town = "Sheffield",
            Postcode = "S1 1AA",
            Locality = "",
            Address3 = "",
            TotalCapacity = 1200,
            TotalPupils = 1000,
            NurseryProvisionName = "No",
            LAId = "373",
            LAName = "Sheffield",
            RegionId = "R",
            RegionName = "Yorkshire and the Humber",
            UrbanRuralId = "A1",
            UrbanRuralName = "Urban",
            PhaseOfEducationId = "P",
            PhaseOfEducationName = "Secondary",
            OfficialSixthFormId = "1",
            OfficialSixthFormName = "Has sixth form",
            AdmissionsPolicyId = "1",
            AdmissionsPolicyName = "Comprehensive",
            GenderId = "M",
            GenderName = "Mixed",
            ResourcedProvisionId = "0",
            ResourcedProvisionName = "No",
            TypeOfEstablishmentId = "27",
            TypeOfEstablishmentName = "Academy",
            EstablishmentTypeGroupId = "10",
            EstablishmentTypeGroupName = "Academies",
            TrustSchoolFlagId = "0",
            TrustSchoolFlagName = "No",
            Easting = coordinates.Easting,
            Northing = coordinates.Northing,
        };
    }
}
