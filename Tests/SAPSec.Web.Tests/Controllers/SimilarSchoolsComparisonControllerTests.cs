using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Features.Geography;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Web.Constants;
using SAPSec.Web.Controllers;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Tests.Controllers;

public class SimilarSchoolsComparisonControllerTests
{
    private readonly Mock<ISchoolDetailsService> _schoolDetailsServiceMock = new();
    private readonly Mock<ISimilarSchoolsSecondaryRepository> _repoMock = new();
    private readonly Mock<ILogger<SimilarSchoolsComparisonController>> _loggerMock = new();

    private readonly SimilarSchoolsComparisonController _sut;

    public SimilarSchoolsComparisonControllerTests()
    {
        var useCase = new GetSimilarSchoolDetails(
            _repoMock.Object,
            _schoolDetailsServiceMock.Object);

        _sut = new SimilarSchoolsComparisonController(useCase, _loggerMock.Object);
    }

    [Fact]
    public async Task Index_ReturnsView_WithMappedModel_AndSetsViewData()
    {
        var urn = "145327";
        var similarUrn = "142075";

        var currentSchool = CreateSimilarSchool(urn, "Main School",
            new BNGCoordinates(Easting: 430000, Northing: 380000));

        var similarSchool = CreateSimilarSchool(similarUrn, "Similar School Group",
            new BNGCoordinates(Easting: 431000, Northing: 381000)); // different coords => distance > 0
        var similarDetails = CreateSchoolDetails(similarUrn, "Similar School");

        SetupDependencies(urn, similarUrn, currentSchool, new[] { similarSchool }, similarDetails);

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
            new BNGCoordinates(Easting: 431000, Northing: 381000)); // different coords => distance > 0

        var similarDetails = CreateSchoolDetails(similarUrn, "Similar School");

        SetupDependencies(urn, similarUrn, currentSchool, new[] { similarSchool }, similarDetails);

        var result = await _sut.SchoolDetails(urn, similarUrn);

        var view = result.Should().BeOfType<ViewResult>().Subject;
        var model = view.Model.Should().BeOfType<SimilarSchoolsComparisonViewModel>().Subject;

        model.Distance.Should().BeGreaterThan(0);
        model.SimilarSchoolDetails.Should().NotBeNull();
        model.SimilarSchoolDetails!.Urn.Should().Be(similarUrn);
    }
    private void SetupDependencies(
        string currentUrn,
        string similarUrn,
        SimilarSchool currentSchool,
        IReadOnlyCollection<SimilarSchool> group,
        SchoolDetails similarDetails)
    {
        _repoMock
            .Setup(r => r.GetSimilarSchoolsGroupAsync(currentUrn))
            .ReturnsAsync((currentSchool, group));

        _schoolDetailsServiceMock
            .Setup(s => s.GetByUrnAsync(similarUrn))
            .ReturnsAsync(similarDetails);
    }

    // ============================
    // FULLY POPULATED TEST DATA
    // ============================

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
            Coordinates = coordinates,
            TotalCapacity = string.Empty,
            TotalPupils = string.Empty,
            NurseryProvisionName = string.Empty,
            LocalAuthority = new("373", "Sheffield"),
            UrbanRural = new("A1", "Urban"),
            Region = new(string.Empty, string.Empty),
            AdmissionsPolicy = new(string.Empty, string.Empty),
            PhaseOfEducation = new(string.Empty, string.Empty),
            Gender = new(string.Empty, string.Empty),
            TypeOfEstablishment = new(string.Empty, string.Empty),
            EstablishmentTypeGroup = new(string.Empty, string.Empty),
            TrustSchoolFlag = new(string.Empty, string.Empty),
            OfficialSixthForm = new(string.Empty, string.Empty),
            ResourcedProvision = new(string.Empty, string.Empty),
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