using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Features.Attendance;
using SAPSec.Core.Features.Ks4HeadlineMeasures;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Model;
using SAPSec.Core.Model.Generated;
using SAPSec.Core.Services;
using SAPSec.Web.Controllers;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Tests.Controllers;

public class SimilarSchoolsControllerTests
{
    //private readonly Mock<ISchoolDetailsService> _schoolDetailsServiceMock;
    private readonly Mock<ISimilarSchoolsSecondaryRepository> _similarSchoolsRepoMock = new();
    private readonly Mock<IEstablishmentRepository> _establishmentRepoMock = new();
    private readonly Mock<IKs4PerformanceRepository> _performanceRepoMock = new();
    private readonly Mock<IAbsenceRepository> _absenceRepoMock = new();
    private readonly Mock<ILogger<SimilarSchoolsController>> _loggerMock = new();
    private readonly SimilarSchoolsController _sut;

    public SimilarSchoolsControllerTests()
    {
        //_schoolDetailsServiceMock = new Mock<ISchoolDetailsService>();
        var logger = new Mock<ILogger<SchoolDetailsService>>();
        var schoolDetailsService = new SchoolDetailsService(_establishmentRepoMock.Object, logger.Object);
        _sut = new SimilarSchoolsController(schoolDetailsService,
            new FindSimilarSchools(
                _establishmentRepoMock.Object,
                _similarSchoolsRepoMock.Object,
                _performanceRepoMock.Object,
                _absenceRepoMock.Object),
            _loggerMock.Object);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Fact]
    public async Task ViewSimilarSchools_ValidUrn_ReturnsViewWithSimilarSchoolsPageViewModel()
    {
        var urn = "105574";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");
        SetupSimilarSchoolsRepo(urn, schoolDetails);

        var result = await _sut.ViewSimilarSchools(urn);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<SimilarSchoolsPageViewModel>().Subject;
        model.Urn.Should().Be(int.Parse(urn));
        model.EstablishmentName.Should().Be("Test Academy");
        model.Schools.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ViewSimilarSchools_SetsBreadcrumbAndSchoolDetailsInViewData()
    {
        var urn = "105574";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");
        SetupSimilarSchoolsRepo(urn, schoolDetails);

        await _sut.ViewSimilarSchools(urn);

        _sut.ViewData["BreadcrumbNode"].Should().NotBeNull();
        _sut.ViewData["SchoolDetails"].Should().BeAssignableTo<SchoolDetails>()
            .Which.Name.Should().Be(schoolDetails.EstablishmentName);
    }

    [Fact]
    public async Task ViewSimilarSchools_InvalidUrn_ReturnsView()
    {
        var urn = "not-a-number";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");
        SetupSimilarSchoolsRepo(urn, schoolDetails);

        var result = await _sut.ViewSimilarSchools(urn);

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task ViewSimilarSchools_PaginatesResults()
    {
        var urn = "105574";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");
        SetupSimilarSchoolsRepo(urn, schoolDetails);

        var result = await _sut.ViewSimilarSchools(urn, page: "2");

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<SimilarSchoolsPageViewModel>().Subject;
        model.CurrentPage.Should().Be(2);
        model.Schools.Should().HaveCount(2);
    }

    [Fact]
    public async Task ViewSimilarSchools_AppliesUrbanRuralFilterFromQuery()
    {
        var urn = "105574";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");
        SetupSimilarSchoolsRepo(urn, schoolDetails);

        _sut.ControllerContext.HttpContext!.Request.QueryString = new QueryString("?ur=UR1&sortBy=Att8");

        var result = await _sut.ViewSimilarSchools(urn);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<SimilarSchoolsPageViewModel>().Subject;
        model.MapSchools.Should().OnlyContain(s => s.UrbanOrRural == "Urban");
    }

    [Fact]
    public async Task ViewSimilarSchools_SetsSortByFromQuery()
    {
        var urn = "105574";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");
        SetupSimilarSchoolsRepo(urn, schoolDetails);

        var result = await _sut.ViewSimilarSchools(urn, "EngMat");

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<SimilarSchoolsPageViewModel>().Subject;
        model.SortBy.Should().Be("EngMat");
    }

    [Fact]
    public async Task ViewSimilarSchools_MapSchools_MatchesFilteredResults()
    {
        var urn = "105574";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");
        SetupSimilarSchoolsRepo(urn, schoolDetails);

        var result = await _sut.ViewSimilarSchools(urn);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<SimilarSchoolsPageViewModel>().Subject;

        model.MapSchools.Should().HaveCount(model.TotalResults);
        model.MapSchools.Should().OnlyContain(s => !string.IsNullOrWhiteSpace(s.EstablishmentName));
    }

    private static Establishment CreateTestSchoolDetails(string urn, string name)
    {
        return new Establishment
        {
            URN = urn,
            EstablishmentName = name,
            LAESTAB = "373/1234",
            UKPRN = "10012345",
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
            RegionName = "Yorkshire",
            UrbanRuralId = "A1",
            UrbanRuralName = "Urban",
            PhaseOfEducationId = "P",
            PhaseOfEducationName = "Secondary",
            OfficialSixthFormId = "1",
            OfficialSixthFormName = "Has sixth form",
            AdmissionsPolicyId = "1",
            AdmissionsPolicyName = "Non-selective",
            GenderId = "M",
            GenderName = "Mixed",
            ResourcedProvisionId = "0",
            ResourcedProvisionName = "No",
            TypeOfEstablishmentId = "34",
            TypeOfEstablishmentName = "Academy converter",
            EstablishmentTypeGroupId = "10",
            EstablishmentTypeGroupName = "Academies",
            TrustSchoolFlagId = "0",
            TrustSchoolFlagName = "No",
            AgeRangeLow = 11,
            AgeRangeHigh = 18,
            ReligiousCharacterId = "1",
            ReligiousCharacterName = "None",
            TrustId = "5001",
            TrustName = "Test Trust",
            HeadFirstName = "John",
            HeadLastName = "Smith",
            HeadTitle = "Mr",
            Website = "https://www.testacademy.org.uk",
            TelephoneNum = "0114 123 4567"
        };
    }

    private void SetupSimilarSchoolsRepo(string urn, Establishment currentSchool)
    {
        //var currentSchool = CreateSimilarSchool(urn, "Current School", "UR1", "Urban");
        var similarSchools = new List<Establishment>();
        for (var i = 0; i < 12; i++)
        {
            var similarSchoolUrn = (111111 + i).ToString();
            var urbanId = i % 2 == 0 ? "UR1" : "UR2";
            var urbanName = i % 2 == 0 ? "Urban" : "Rural";
            var similarSchool = CreateSimilarSchool(urn, $"Similar {i + 1}", urbanId, urbanName);
            similarSchools.Add(similarSchool);
            _establishmentRepoMock
                .Setup(x => x.GetEstablishmentAsync(similarSchoolUrn))
                .ReturnsAsync(similarSchool);
        }

        _establishmentRepoMock
            .Setup(x => x.GetEstablishmentAsync(urn))
            .ReturnsAsync(currentSchool);
        _establishmentRepoMock
            .Setup(x => x.GetEstablishmentsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([currentSchool, .. similarSchools]);
        _similarSchoolsRepoMock
            .Setup(x => x.GetSimilarSchoolsGroupAsync(urn))
            .ReturnsAsync(similarSchools.Select(s => new SimilarSchoolsSecondaryGroupsEntry { URN = urn, NeighbourURN = s.URN }).ToList());
        _absenceRepoMock
            .Setup(r => r.GetByUrnAsync(It.IsAny<string>()))
            .ReturnsAsync((AbsenceData?)null);
        _absenceRepoMock
            .Setup(r => r.GetByUrnsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<AbsenceData>());
        _performanceRepoMock
            .Setup(r => r.GetByUrnAsync(It.IsAny<string>()))
            .ReturnsAsync((Ks4PerformanceData?)null);
        _performanceRepoMock
            .Setup(r => r.GetByUrnsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<Ks4PerformanceData>());
    }

    private static Establishment CreateSimilarSchool(string urn, string name, string urbanId, string urbanName)
    {
        return new Establishment
        {
            URN = urn,
            EstablishmentName = name,
            Street = "Street",
            Locality = "Locality",
            Address3 = "Address3",
            Town = "Town",
            Postcode = "ZZ1 1ZZ",
            Easting = 100,
            Northing = 100,
            TotalCapacity = null,
            TotalPupils = null,
            NurseryProvisionName = string.Empty,
            LAId = "001",
            LAName = "Authority",
            UrbanRuralId = urbanId,
            UrbanRuralName = urbanName,
            //Attainment8Score = DataWithAvailability.Available(50m),
            //BiologyGcseGrade5AndAbovePercentage = DataWithAvailability.Available(60m),
            //ChemistryGcseGrade5AndAbovePercentage = DataWithAvailability.Available(60m),
            //CombinedScienceGcseGrade55AndAbovePercentage = DataWithAvailability.Available(60m),
            //EnglishLanguageGcseGrade5AndAbovePercentage = DataWithAvailability.Available(60m),
            //EnglishLiteratureGcseGrade5AndAbovePercentage = DataWithAvailability.Available(60m),
            //EnglishMathsGcseGrade5AndAbovePercentage = DataWithAvailability.Available(60m),
            //MathsGcseGrade5AndAbovePercentage = DataWithAvailability.Available(60m),
            //PhysicsGcseGrade5AndAbovePercentage = DataWithAvailability.Available(60m),
            //OverallAbsenceRate = DataWithAvailability.Available(0m),
            //PersistentAbsenceRate = DataWithAvailability.Available(0m)
        };
    }
}
