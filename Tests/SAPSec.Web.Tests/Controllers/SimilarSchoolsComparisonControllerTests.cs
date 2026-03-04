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
using SAPSec.Web.Formatters;
using SAPSec.Web.Helpers;
using SAPSec.Web.ViewModels;
using LocalAuthority = SAPSec.Core.Features.SimilarSchools.LocalAuthority;

namespace SAPSec.Web.Tests.Controllers;

public class SimilarSchoolsComparisonControllerTests
{
    private readonly Mock<ISchoolDetailsService> _schoolDetailsServiceMock = new();
    private readonly Mock<ISimilarSchoolsSecondaryRepository> _repoMock = new();
    private readonly Mock<ILogger<SimilarSchoolsComparisonController>> _loggerMock = new();
    private readonly Mock<GetSimilarSchoolsSecondaryNationalStandardDeviations> _getNationalStandardDeviationsMock;

    private readonly SimilarSchoolsComparisonController _sut;

    public SimilarSchoolsComparisonControllerTests()
    {
        var getSimilarSchoolDetails = new GetSimilarSchoolDetails(
            _repoMock.Object,
            _schoolDetailsServiceMock.Object);

        var getCharacteristicsComparison = new GetCharacteristicsComparison(
            _repoMock.Object);

        var characteristicsFormatter = new CharacteristicsComparisonFormatter();

        _getNationalStandardDeviationsMock = new Mock<GetSimilarSchoolsSecondaryNationalStandardDeviations>(_repoMock.Object);
        _getNationalStandardDeviationsMock
            .Setup(r => r.Execute(It.IsAny<GetSimilarSchoolsSecondaryNationalStandardDeviationsRequest>()))
            .ReturnsAsync(new GetSimilarSchoolsSecondaryNationalStandardDeviationsResponse(new SimilarSchoolsSecondaryNationalSD
            {
                PupilPremiumEligibilityPercentage = 13.983589m,
                PupilsWithEalPercentage = 18.755181m,
                Polar4Quintile = 1.022255m,
                PupilStabilityRate = 6.442814m,
                AverageIdaciScore = 0.078069m,
                PupilsWithSenSupportPercentage = 5.530940m,
                PupilCount = 388.664809m,
                PupilsWithEhcPlanPercentage = 1.678816m,
                Ks2AverageScore = 2.527329m
            }));

        _sut = new SimilarSchoolsComparisonController(
            getSimilarSchoolDetails,
            getCharacteristicsComparison,
            _getNationalStandardDeviationsMock.Object,
            characteristicsFormatter,
            _loggerMock.Object);
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
        model.SimilarSchoolName.Should().Be(similarDetails.Name.Display());

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

        var result = await _sut.SchoolDetails(urn, similarUrn);

        var view = result.Should().BeOfType<ViewResult>().Subject;
        var model = view.Model.Should().BeOfType<SimilarSchoolsComparisonViewModel>().Subject;

        model.Distance.Should().BeGreaterThan(0);
        model.SimilarSchoolDetails.Should().NotBeNull();
        model.SimilarSchoolDetails!.Urn.Value.Should().Be(similarUrn);
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
        model.CharacteristicsRows[4].Similarity.Should().Be(SimilarSchoolsComparisonViewModel.SimilarityLabel.LessSimilar);

        model.CharacteristicsRows[5].Characteristic.Should().Be("Average POLAR4 quintile");
        model.CharacteristicsRows[5].Similarity.Should().Be(SimilarSchoolsComparisonViewModel.SimilarityLabel.NotSimilar);
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
            .ReturnsAsync((currentSchool, group));

        _schoolDetailsServiceMock
            .Setup(s => s.GetByUrnAsync(It.IsAny<string>()))
            .ReturnsAsync(similarDetails);
    }

    private void SetupSecondaryValues(string currentUrn, string similarUrn)
    {
        var values = new List<SimilarSchoolsSecondaryValues>
        {
            new SimilarSchoolsSecondaryValues
            {
                Urn = currentUrn,
                Ks2ReadingScore = 104.5m,
                Ks2MathsScore = 104.1m,
                PupilCount = 760,
                PupilStabilityRate = 90m,
                PupilPremiumEligibilityPercentage = 52.0m,
                AverageIdaciScore = 0.316508m,
                Polar4Quintile = 3,
                PupilsWithEhcPlanPercentage = 2.105263m,
                PupilsWithSenSupportPercentage = 16.315789m,
                PupilsWithEalPercentage = 39.525692m
            },
            new SimilarSchoolsSecondaryValues
            {
                Urn = similarUrn,
                Ks2ReadingScore = 103.7m,
                Ks2MathsScore = 103.6m,
                PupilCount = 962,
                PupilStabilityRate = 91.7m,
                PupilPremiumEligibilityPercentage = 41.2m,
                AverageIdaciScore = 0.351137m,
                Polar4Quintile = 2,
                PupilsWithEhcPlanPercentage = 3.326403m,
                PupilsWithSenSupportPercentage = 8.939709m,
                PupilsWithEalPercentage = 61.954262m
            }
        };

        _repoMock
            .Setup(r => r.GetSecondaryValuesByUrnsAsync(
                It.Is<IEnumerable<string>>(u => u.Contains(currentUrn) && u.Contains(similarUrn))))
            .ReturnsAsync(values);
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
            LocalAuthority = new LocalAuthority("373", "Sheffield"),
            Coordinates = coordinates,
            UrbanRuralId = "A1",
            UrbanRuralName = "Urban",
            Attainment8Score = DataWithAvailability.Available(50m),
            BiologyGcseGrade5AndAbovePercentage = DataWithAvailability.Available(60m),
            ChemistryGcseGrade5AndAbovePercentage = DataWithAvailability.Available(61m),
            CombinedSciencGcseGrade55AndAbovePercentage = DataWithAvailability.Available(62m),
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
            Urn = DataWithAvailability.Available(urn),
            Name = DataWithAvailability.Available(name),
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
