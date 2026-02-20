using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Features.Geography;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Web.Controllers;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Tests.Controllers;

public class SimilarSchoolsControllerTests
{
    private readonly Mock<ISchoolDetailsService> _schoolDetailsServiceMock;
    private readonly Mock<ISimilarSchoolsSecondaryRepository> _similarSchoolsRepoMock;
    private readonly Mock<ILogger<SimilarSchoolsController>> _loggerMock;
    private readonly SimilarSchoolsController _sut;

    public SimilarSchoolsControllerTests()
    {
        _schoolDetailsServiceMock = new Mock<ISchoolDetailsService>();
        _similarSchoolsRepoMock = new Mock<ISimilarSchoolsSecondaryRepository>();
        _loggerMock = new Mock<ILogger<SimilarSchoolsController>>();
        _sut = new SimilarSchoolsController(_schoolDetailsServiceMock.Object, new FindSimilarSchools(_similarSchoolsRepoMock.Object), _loggerMock.Object);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Fact]
    public async Task ViewSimilarSchools_ValidUrn_ReturnsViewWithSimilarSchoolsPageViewModel()
    {
        var urn = "147788";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");
        _schoolDetailsServiceMock
            .Setup(x => x.TryGetByUrnAsync(urn))
            .ReturnsAsync(schoolDetails);
        SetupSimilarSchoolsRepo();

        var result = await _sut.ViewSimilarSchools(urn, "Att8", 1);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<SimilarSchoolsPageViewModel>().Subject;
        model.Urn.Should().Be(int.Parse(urn));
        model.EstablishmentName.Should().Be("Test Academy");
        model.Schools.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ViewSimilarSchools_SetsBreadcrumbAndSchoolDetailsInViewData()
    {
        var urn = "147788";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");
        _schoolDetailsServiceMock
            .Setup(x => x.TryGetByUrnAsync(urn))
            .ReturnsAsync(schoolDetails);
        SetupSimilarSchoolsRepo();

        await _sut.ViewSimilarSchools(urn, "Att8", 1);

        _sut.ViewData["BreadcrumbNode"].Should().NotBeNull();
        _sut.ViewData["SchoolDetails"].Should().BeSameAs(schoolDetails);
    }

    [Fact]
    public async Task ViewSimilarSchools_SchoolNotFound_RedirectsToError()
    {
        var urn = "999999";
        _schoolDetailsServiceMock
            .Setup(x => x.TryGetByUrnAsync(urn))
            .ReturnsAsync((SchoolDetails?)null);

        var result = await _sut.ViewSimilarSchools(urn, "Att8", 1);

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Error");
    }

    [Fact]
    public async Task ViewSimilarSchools_InvalidUrn_ReturnsView()
    {
        var urn = "not-a-number";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");
        _schoolDetailsServiceMock
            .Setup(x => x.TryGetByUrnAsync(urn))
            .ReturnsAsync(schoolDetails);
        SetupSimilarSchoolsRepo();

        var result = await _sut.ViewSimilarSchools(urn, "Att8", 1);

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task ViewSimilarSchools_PaginatesResults()
    {
        var urn = "147788";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");
        _schoolDetailsServiceMock
            .Setup(x => x.TryGetByUrnAsync(urn))
            .ReturnsAsync(schoolDetails);
        SetupSimilarSchoolsRepo();

        var result = await _sut.ViewSimilarSchools(urn, "Att8", 2);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<SimilarSchoolsPageViewModel>().Subject;
        model.CurrentPage.Should().Be(2);
        model.Schools.Should().HaveCount(2);
    }

    [Fact]
    public async Task ViewSimilarSchools_AppliesUrbanRuralFilterFromQuery()
    {
        var urn = "147788";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");
        _schoolDetailsServiceMock
            .Setup(x => x.TryGetByUrnAsync(urn))
            .ReturnsAsync(schoolDetails);
        SetupSimilarSchoolsRepo();
        _sut.ControllerContext.HttpContext!.Request.QueryString = new QueryString("?ur=UR1&sortBy=Att8");

        var result = await _sut.ViewSimilarSchools(urn, "Att8", 1);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<SimilarSchoolsPageViewModel>().Subject;
        model.MapSchools.Should().OnlyContain(s => s.UrbanOrRural == "Urban");
    }

    [Fact]
    public async Task ViewSimilarSchools_SetsSortByFromQuery()
    {
        var urn = "147788";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");
        _schoolDetailsServiceMock
            .Setup(x => x.TryGetByUrnAsync(urn))
            .ReturnsAsync(schoolDetails);
        SetupSimilarSchoolsRepo();

        var result = await _sut.ViewSimilarSchools(urn, "EngMat", 1);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<SimilarSchoolsPageViewModel>().Subject;
        model.SortBy.Should().Be("EngMat");
    }

    [Fact]
    public async Task ViewSimilarSchools_MapSchools_MatchesFilteredResults()
    {
        var urn = "147788";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");
        _schoolDetailsServiceMock
            .Setup(x => x.TryGetByUrnAsync(urn))
            .ReturnsAsync(schoolDetails);
        SetupSimilarSchoolsRepo();

        var result = await _sut.ViewSimilarSchools(urn, "Att8", 1);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<SimilarSchoolsPageViewModel>().Subject;

        model.MapSchools.Should().HaveCount(model.TotalResults);
        model.MapSchools.Should().OnlyContain(s => !string.IsNullOrWhiteSpace(s.EstablishmentName));
    }

    private static SchoolDetails CreateTestSchoolDetails(string urn, string name)
    {
        return new SchoolDetails
        {
            Name = DataWithAvailability.Available(name),
            Urn = DataWithAvailability.Available(urn),
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

    private void SetupSimilarSchoolsRepo()
    {
        var currentSchool = CreateSimilarSchool("147788", "Current School", "UR1", "Urban");
        var similarSchools = new List<SimilarSchool>();
        for (var i = 0; i < 12; i++)
        {
            var urn = (111111 + i).ToString();
            var urbanId = i % 2 == 0 ? "UR1" : "UR2";
            var urbanName = i % 2 == 0 ? "Urban" : "Rural";
            similarSchools.Add(CreateSimilarSchool(urn, $"Similar {i + 1}", urbanId, urbanName));
        }

        _similarSchoolsRepoMock
            .Setup(x => x.GetSimilarSchoolsGroupAsync(It.IsAny<string>()))
            .ReturnsAsync((currentSchool, similarSchools));
    }

    private static SimilarSchool CreateSimilarSchool(string urn, string name, string urbanId, string urbanName)
    {
        return new SimilarSchool
        {
            URN = urn,
            Name = name,
            Address = new Address
            {
                Street = "Street",
                Locality = "Locality",
                Address3 = "Address3",
                Town = "Town",
                Postcode = "ZZ1 1ZZ"
            },
            LocalAuthority = new SAPSec.Core.Features.SimilarSchools.LocalAuthority("001", "Authority"),
            Coordinates = new BNGCoordinates(100, 100),
            UrbanRuralId = urbanId,
            UrbanRuralName = urbanName,
            Attainment8Score = DataWithAvailability.Available(50m),
            BiologyGcseGrade5AndAbovePercentage = DataWithAvailability.Available(60m),
            ChemistryGcseGrade5AndAbovePercentage = DataWithAvailability.Available(60m),
            CombinedSciencGcseGrade55AndAbovePercentage = DataWithAvailability.Available(60m),
            EnglishLanguageGcseGrade5AndAbovePercentage = DataWithAvailability.Available(60m),
            EnglishLiteratureGcseGrade5AndAbovePercentage = DataWithAvailability.Available(60m),
            EnglishMathsGcseGrade5AndAbovePercentage = DataWithAvailability.Available(60m),
            MathsGcseGrade5AndAbovePercentage = DataWithAvailability.Available(60m),
            PhysicsGcseGrade5AndAbovePercentage = DataWithAvailability.Available(60m)
        };
    }
}
