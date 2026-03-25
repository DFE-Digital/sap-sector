using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Features.Attendance.UseCases;
using SAPSec.Core.Features.Geography;
using SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Core.Model.Generated;
using SAPSec.Data;
using SAPSec.Data.Model.Generated;
using SAPSec.Web.Constants;
using SAPSec.Web.Controllers;
using SAPSec.Web.Formatters;
using SAPSec.Web.ViewModels;
using System.Text.Json;

namespace SAPSec.Web.Tests.Controllers;

public class SimilarSchoolsComparisonControllerTests
{
    private readonly Mock<ISchoolDetailsService> _schoolDetailsServiceMock = new();
    private readonly Mock<IEstablishmentRepository> _establishmentRepositoryMock = new();
    private readonly Mock<ISimilarSchoolsSecondaryRepository> _repoMock = new();
    private readonly Mock<IAbsenceRepository> _attendanceRepositoryMock = new();
    private readonly Mock<IKs4PerformanceRepository> _ks4PerformanceRepositoryMock = new();
    private readonly Mock<IKs4DestinationsRepository> _ks4DestinationsRepositoryMock = new();
    private readonly Mock<ILogger<SimilarSchoolsComparisonController>> _loggerMock = new();
    private readonly SimilarSchoolsComparisonController _sut;

    public SimilarSchoolsComparisonControllerTests()
    {
        var getSimilarSchoolDetails = new GetSimilarSchoolDetails(
            _establishmentRepositoryMock.Object,
            _repoMock.Object,
            _schoolDetailsServiceMock.Object,
            _ks4PerformanceRepositoryMock.Object);
        var ks4UseCase = new GetKs4HeadlineMeasures(
            _ks4PerformanceRepositoryMock.Object,
            _ks4DestinationsRepositoryMock.Object,
            _schoolDetailsServiceMock.Object);
        var attendanceUseCase = new GetAttendanceMeasures(
            _attendanceRepositoryMock.Object,
            _establishmentRepositoryMock.Object);

        var getCharacteristicsComparison = new GetCharacteristicsComparison(
            _repoMock.Object);

        _repoMock
            .Setup(r => r.GetSimilarSchoolsSecondaryStandardDeviationsAsync())
            .ReturnsAsync(new SimilarSchoolsSecondaryStandardDeviationsEntry
            {
                PPPerc = 13.983589m,
                PercentEAL = 18.755181m,
                Polar4QuintilePupils = 1.022255m,
                PStability = 6.442814m,
                IdaciPupils = 0.078069m,
                PercentSchSupport = 5.530940m,
                NumberOfPupils = 388.664809m,
                PercentageStatementOrEHP = 1.678816m,
                KS2AVG = 2.527329m
            });

        var characteristicsFormatter = new CharacteristicsComparisonFormatter();

        _sut = new SimilarSchoolsComparisonController(
            getSimilarSchoolDetails,
            attendanceUseCase,
            ks4UseCase,
            getCharacteristicsComparison,
            characteristicsFormatter,
            _loggerMock.Object,
            _repoMock.Object);
    }

    [Fact]
    public async Task Index_ReturnsView_WithMappedModel_AndSetsViewData()
    {
        var urn = "145327";
        var similarUrn = "142075";

        var currentSchool = CreateSimilarSchool(urn, "Main School",
            new BNGCoordinates(Easting: 430000, Northing: 380000));

        var similarSchool = CreateSimilarSchool(similarUrn, "Similar School Group",
            new BNGCoordinates(Easting: 431000, Northing: 381000));

        var similarDetails = CreateSchoolDetails(similarUrn, "Similar School");

        SetupBaseDependencies(urn, similarUrn, currentSchool, similarSchool, similarDetails);
        SetupSecondaryValues(urn, similarUrn);

        var result = await _sut.Index(urn, similarUrn);

        var view = result.Should().BeOfType<ViewResult>().Subject;
        var model = view.Model.Should().BeOfType<SimilarSchoolsComparisonViewModel>().Subject;

        model.Urn.Should().Be(urn);
        model.SimilarSchoolUrn.Should().Be(similarUrn);
        model.Name.Should().Be(currentSchool.Name);
        model.SimilarSchoolName.Should().Be(similarDetails.Name);

        _sut.ViewData[ViewDataKeys.BreadcrumbNode].Should().NotBeNull();
        _sut.ViewData["ComparisonSchool"].Should().BeSameAs(model);
    }

    [Fact]
    public async Task SchoolDetails_ReturnsFullMappedModel()
    {
        var urn = "145327";
        var similarUrn = "142075";

        var currentSchool = CreateSimilarSchool(urn, "Main School",
            new BNGCoordinates(Easting: 430000, Northing: 380000));

        var similarSchool = CreateSimilarSchool(similarUrn, "Similar School Group",
            new BNGCoordinates(Easting: 431000, Northing: 381000));

        var similarDetails = CreateSchoolDetails(similarUrn, "Similar School");

        SetupBaseDependencies(urn, similarUrn, currentSchool, similarSchool, similarDetails);
        SetupSecondaryValues(urn, similarUrn);

        var result = await _sut.SchoolDetails(urn, similarUrn);

        var view = result.Should().BeOfType<ViewResult>().Subject;
        var model = view.Model.Should().BeOfType<SimilarSchoolsComparisonViewModel>().Subject;

        model.Distance.Should().BeGreaterThan(0);
        model.SimilarSchoolDetails.Should().NotBeNull();
        model.SimilarSchoolDetails!.Urn.Should().Be(similarUrn);
    }

    [Fact]
    public async Task Similarity_ReturnsView_WithCharacteristicsRows_AndSimilarityLabels()
    {
        var urn = "145327";
        var similarUrn = "142075";

        var currentSchool = CreateSimilarSchool(urn, "Main School",
            new BNGCoordinates(Easting: 430000, Northing: 380000));

        var similarSchool = CreateSimilarSchool(similarUrn, "Similar School Group",
            new BNGCoordinates(Easting: 431000, Northing: 381000));

        var similarDetails = CreateSchoolDetails(similarUrn, "Similar School");

        SetupBaseDependencies(urn, similarUrn, currentSchool, similarSchool, similarDetails);
        SetupSecondaryValues(urn, similarUrn);

        var result = await _sut.Similarity(urn, similarUrn);

        var view = result.Should().BeOfType<ViewResult>().Subject;
        var model = view.Model.Should().BeOfType<SimilarSchoolsComparisonViewModel>().Subject;

        model.CharacteristicsRows.Should().NotBeNull();
        model.CharacteristicsRows.Should().HaveCount(9);

        model.CharacteristicsRows[0].Characteristic.Should().Be("Average KS2 reading and maths score");
        model.CharacteristicsRows[0].CurrentSchoolValue.Should().NotBeNullOrWhiteSpace();
        model.CharacteristicsRows[0].SimilarSchoolValue.Should().NotBeNullOrWhiteSpace();

        model.CharacteristicsRows[4].Characteristic.Should().Be("Average IDACI score");
        model.CharacteristicsRows[4].Similarity.Should().Be(SchoolSimilarity.LessSimilar);

        model.CharacteristicsRows[5].Characteristic.Should().Be("Average POLAR4 quintile");
        model.CharacteristicsRows[5].Similarity.Should().Be(SchoolSimilarity.NotSimilar);
    }

    [Fact]
    public async Task Similarity_WithGroupCalculationQuery_UsesGroupStandardDeviation()
    {
        var urn = "145327";
        var similarUrn = "142075";

        var currentSchool = CreateSimilarSchool(urn, "Main School",
            new BNGCoordinates(Easting: 430000, Northing: 380000));

        var similarSchool = CreateSimilarSchool(similarUrn, "Similar School Group",
            new BNGCoordinates(Easting: 431000, Northing: 381000));

        var similarDetails = CreateSchoolDetails(similarUrn, "Similar School");

        SetupBaseDependencies(urn, similarUrn, currentSchool, similarSchool, similarDetails);
        SetupSecondaryValues(urn, similarUrn);
        SetupGroupSecondaryValues(urn, new[] { similarUrn, "300001", "300002", "300003", "300004" });

        var result = await _sut.Similarity(urn, similarUrn, "group");

        var view = result.Should().BeOfType<ViewResult>().Subject;
        var model = view.Model.Should().BeOfType<SimilarSchoolsComparisonViewModel>().Subject;

        model.CharacteristicsRows.Should().NotBeNull();
        model.CharacteristicsRows[0].Similarity.Should().Be(SchoolSimilarity.LessSimilar);
    }

    [Fact]
    public async Task AttendanceData_ReturnsDefaultPayloadShape()
    {
        _establishmentRepositoryMock
            .Setup(x => x.GetEstablishmentAsync(It.IsAny<string>()))
            .ReturnsAsync(new Establishment { URN = "145327", LAId = "373" });

        _attendanceRepositoryMock
            .Setup(x => x.GetByUrnAsync(It.IsAny<string>(), It.IsAny<string?>()))
            .ReturnsAsync(new AbsenceData(
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
                    Abs_Tot_Eng_Current_Pct = "4.8",
                    Abs_Tot_Eng_Previous_Pct = "4.9",
                    Abs_Tot_Eng_Previous2_Pct = "5.0",
                    Abs_Persistent_Eng_Current_Pct = "15.6",
                    Abs_Persistent_Eng_Previous_Pct = "15.8",
                    Abs_Persistent_Eng_Previous2_Pct = "16.0"
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
        table.GetProperty("thisSchool").GetArrayLength().Should().Be(4);
        table.GetProperty("similarSchool").GetArrayLength().Should().Be(4);
        table.GetProperty("england").GetArrayLength().Should().Be(4);
    }

    private void SetupBaseDependencies(
        string currentUrn,
        string similarUrn,
        SimilarSchool currentSchool,
        SimilarSchool similarSchool,
        SchoolDetails similarDetails)
    {
        similarSchool.URN = similarUrn;
        currentSchool.URN = currentUrn;

        var group = new List<SimilarSchool> { similarSchool }.AsReadOnly();

        _repoMock
            .Setup(r => r.GetSimilarSchoolsGroupAsync(It.IsAny<string>()))
            .ReturnsAsync(group.Select(g => new SimilarSchoolsSecondaryGroupsEntry { URN = currentUrn, NeighbourURN = g.URN }).ToList());

        _schoolDetailsServiceMock
            .Setup(s => s.GetByUrnAsync(It.IsAny<string>()))
            .ReturnsAsync(similarDetails);

        _schoolDetailsServiceMock
            .Setup(s => s.GetByUrnAsync(It.IsAny<string>()))
            .ReturnsAsync(similarDetails);
    }

    private void SetupSecondaryValues(string currentUrn, string similarUrn)
    {
        var values = new List<SimilarSchoolsSecondaryValuesEntry>
        {
            new SimilarSchoolsSecondaryValuesEntry
            {
                URN = currentUrn,
                KS2RP = "104.5",
                KS2MP = "104.1",
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
                KS2RP = "103.7",
                KS2MP = "103.6",
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
            .Setup(r => r.GetSecondaryValuesByUrnsAsync(
                It.Is<IEnumerable<string>>(u => u.Contains(currentUrn) && u.Contains(similarUrn))))
            .ReturnsAsync(values);
    }

    private void SetupGroupSecondaryValues(string currentUrn, IReadOnlyCollection<string> groupUrns)
    {
        _repoMock
            .Setup(r => r.GetSimilarSchoolsGroupAsync(currentUrn))
            .ReturnsAsync(groupUrns.Select(urn => new SimilarSchoolsSecondaryGroupsEntry { URN = currentUrn, NeighbourURN = urn }).ToList());

        var groupValues = new List<SimilarSchoolsSecondaryValuesEntry>
        {
            new()
            {
                URN = groupUrns.ElementAt(0),
                KS2RP = "101",
                KS2MP = "101"
            },
            new()
            {
                URN = groupUrns.ElementAt(1),
                KS2RP = "102",
                KS2MP = "102"
            },
            new()
            {
                URN = groupUrns.ElementAt(2),
                KS2RP = "103",
                KS2MP = "103"
            },
            new()
            {
                URN = groupUrns.ElementAt(3),
                KS2RP = "104",
                KS2MP = "104"
            },
            new()
            {
                URN = groupUrns.ElementAt(4),
                KS2RP = "105",
                KS2MP = "105"
            }
        };

        _repoMock
            .Setup(r => r.GetSecondaryValuesByUrnsAsync(It.Is<IEnumerable<string>>(u => u.SequenceEqual(groupUrns))))
            .ReturnsAsync(groupValues);
    }

    private static SimilarSchool CreateSimilarSchool(string urn, string name, BNGCoordinates coordinates)
    {
        return new SimilarSchool
        {
            URN = urn,
            Name = name,
            Address = new Address
            {
                Street = "123 Test Street",
                Town = "Sheffield",
                Postcode = "S1 1AA",
                Locality = "",
                Address3 = ""
            },
            TotalCapacity = 1200,
            TotalPupils = 1000,
            NurseryProvisionName = "No",
            LocalAuthority = new ReferenceData("373", "Sheffield"),
            Region = new ReferenceData("R", "Yorkshire and the Humber"),
            UrbanRural = new ReferenceData("A1", "Urban"),
            PhaseOfEducation = new ReferenceData("P", "Secondary"),
            OfficialSixthForm = new ReferenceData("1", "Has sixth form"),
            AdmissionsPolicy = new ReferenceData("1", "Comprehensive"),
            Gender = new ReferenceData("M", "Mixed"),
            ResourcedProvision = new ReferenceData("0", "No"),
            TypeOfEstablishment = new ReferenceData("27", "Academy"),
            EstablishmentTypeGroup = new ReferenceData("10", "Academies"),
            TrustSchoolFlag = new ReferenceData("0", "No"),
            Coordinates = coordinates,
            Attainment8Score = DataWithAvailability.Available(50m),
            BiologyGcseGrade5AndAbovePercentage = DataWithAvailability.Available(60m),
            ChemistryGcseGrade5AndAbovePercentage = DataWithAvailability.Available(61m),
            CombinedScienceGcseGrade55AndAbovePercentage = DataWithAvailability.Available(62m),
            EnglishLanguageGcseGrade5AndAbovePercentage = DataWithAvailability.Available(63m),
            EnglishLiteratureGcseGrade5AndAbovePercentage = DataWithAvailability.Available(64m),
            EnglishMathsGcseGrade5AndAbovePercentage = DataWithAvailability.Available(65m),
            MathsGcseGrade5AndAbovePercentage = DataWithAvailability.Available(66m),
            PhysicsGcseGrade5AndAbovePercentage = DataWithAvailability.Available(67m)
        };
    }

    private static SchoolDetails CreateSchoolDetails(string urn, string name)
    {
        return new SchoolDetails
        {
            Urn = urn,
            Name = name,
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
}
