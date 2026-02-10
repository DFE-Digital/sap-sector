using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Web.Controllers;
using SAPSec.Web.MockData;
using SAPSec.Web.ViewModels;

namespace SAPSec.Web.Tests.Controllers;

public class SchoolControllerViewSimilarSchoolsTests
{
    private readonly Mock<ISchoolDetailsService> _schoolDetailsServiceMock;
    private readonly Mock<ILogger<SchoolController>> _loggerMock;
    private readonly SchoolController _sut;

    public SchoolControllerViewSimilarSchoolsTests()
    {
        _schoolDetailsServiceMock = new Mock<ISchoolDetailsService>();
        _loggerMock = new Mock<ILogger<SchoolController>>();
        _sut = new SchoolController(_schoolDetailsServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void ViewSimilarSchools_ValidUrn_ReturnsViewWithSimilarSchoolsPageViewModel()
    {
        var urn = "147788";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");
        _schoolDetailsServiceMock
            .Setup(x => x.TryGetByUrn(urn))
            .Returns(schoolDetails);

        var result = _sut.ViewSimilarSchools(urn, new SimilarSchoolsFilterViewModel(), "Attainment 8", 1);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<SimilarSchoolsPageViewModel>().Subject;
        model.Urn.Should().Be(int.Parse(urn));
        model.EstablishmentName.Should().Be("Test Academy");
        model.Schools.Should().NotBeEmpty();
    }

    [Fact]
    public void ViewSimilarSchools_SetsBreadcrumbAndSchoolDetailsInViewData()
    {
        var urn = "147788";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");
        _schoolDetailsServiceMock
            .Setup(x => x.TryGetByUrn(urn))
            .Returns(schoolDetails);

        _sut.ViewSimilarSchools(urn, new SimilarSchoolsFilterViewModel(), "Attainment 8", 1);

        _sut.ViewData["BreadcrumbNode"].Should().NotBeNull();
        _sut.ViewData["SchoolDetails"].Should().BeSameAs(schoolDetails);
    }

    [Fact]
    public void ViewSimilarSchools_SchoolNotFound_RedirectsToError()
    {
        var urn = "999999";
        _schoolDetailsServiceMock
            .Setup(x => x.TryGetByUrn(urn))
            .Returns((SchoolDetails?)null);

        var result = _sut.ViewSimilarSchools(urn, new SimilarSchoolsFilterViewModel(), "Attainment 8", 1);

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Error");
    }

    [Fact]
    public void ViewSimilarSchools_InvalidUrn_RedirectsToError()
    {
        var urn = "not-a-number";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");
        _schoolDetailsServiceMock
            .Setup(x => x.TryGetByUrn(urn))
            .Returns(schoolDetails);

        var result = _sut.ViewSimilarSchools(urn, new SimilarSchoolsFilterViewModel(), "Attainment 8", 1);

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Error");
    }

    [Fact]
    public void ViewSimilarSchools_PaginatesResults()
    {
        var urn = "147788";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");
        _schoolDetailsServiceMock
            .Setup(x => x.TryGetByUrn(urn))
            .Returns(schoolDetails);

        var result = _sut.ViewSimilarSchools(urn, new SimilarSchoolsFilterViewModel(), "Attainment 8", 2);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<SimilarSchoolsPageViewModel>().Subject;
        model.CurrentPage.Should().Be(2);
        model.Schools.Should().HaveCount(10);
    }

    [Fact]
    public void ViewSimilarSchools_AppliesRegionFilter()
    {
        var urn = "147788";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");
        _schoolDetailsServiceMock
            .Setup(x => x.TryGetByUrn(urn))
            .Returns(schoolDetails);

        var filters = new SimilarSchoolsFilterViewModel
        {
            SelectedRegions = new List<string> { "North East" }
        };

        var result = _sut.ViewSimilarSchools(urn, filters, "Attainment 8", 1);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<SimilarSchoolsPageViewModel>().Subject;
        var allSchools = MockSimilarSchoolsData.GetSimilarSchools(int.Parse(urn));
        var expected = allSchools.Where(s => s.Region == "North East").ToList();

        model.MapSchools.Should().HaveCount(expected.Count);
        model.MapSchools.Should().OnlyContain(s => s.Region == "North East");
    }

    [Fact]
    public void ViewSimilarSchools_SortsBySchoolName()
    {
        var urn = "147788";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");
        _schoolDetailsServiceMock
            .Setup(x => x.TryGetByUrn(urn))
            .Returns(schoolDetails);

        var result = _sut.ViewSimilarSchools(urn, new SimilarSchoolsFilterViewModel(), "School name", 1);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<SimilarSchoolsPageViewModel>().Subject;
        model.Schools.Select(s => s.EstablishmentName).Should().BeInAscendingOrder();
    }

    [Fact]
    public void ViewSimilarSchools_MapSchools_MatchesFilteredResults()
    {
        var urn = "147788";
        var schoolDetails = CreateTestSchoolDetails(urn, "Test Academy");
        _schoolDetailsServiceMock
            .Setup(x => x.TryGetByUrn(urn))
            .Returns(schoolDetails);

        var filters = new SimilarSchoolsFilterViewModel
        {
            SelectedUrbanOrRural = new List<string> { "Urban" }
        };

        var result = _sut.ViewSimilarSchools(urn, filters, "Attainment 8", 1);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<SimilarSchoolsPageViewModel>().Subject;

        model.MapSchools.Should().HaveCount(model.TotalResults);
        model.MapSchools.Should().OnlyContain(s => s.UrbanOrRural == "Urban");
    }

    private static SchoolDetails CreateTestSchoolDetails(string urn, string name)
    {
        return new SchoolDetails
        {
            Name = DataAvailability.Available(name),
            Urn = DataAvailability.Available(urn),
            DfENumber = DataAvailability.Available("373/1234"),
            Ukprn = DataAvailability.Available("10012345"),
            Address = DataAvailability.Available("123 Test Street, Sheffield, S1 1AA"),
            LocalAuthorityName = DataAvailability.Available("Sheffield"),
            LocalAuthorityCode = DataAvailability.Available("373"),
            Region = DataAvailability.Available("Yorkshire"),
            UrbanRuralDescription = DataAvailability.Available("Urban"),
            AgeRangeLow = DataAvailability.Available(11),
            AgeRangeHigh = DataAvailability.Available(18),
            GenderOfEntry = DataAvailability.Available("Mixed"),
            PhaseOfEducation = DataAvailability.Available("Secondary"),
            SchoolType = DataAvailability.Available("Academy converter"),
            AdmissionsPolicy = DataAvailability.Available("Non-selective"),
            ReligiousCharacter = DataAvailability.Available("None"),
            GovernanceStructure = DataAvailability.Available(GovernanceType.MultiAcademyTrust),
            AcademyTrustName = DataAvailability.Available("Test Trust"),
            AcademyTrustId = DataAvailability.Available("5001"),
            HasNurseryProvision = DataAvailability.Available(false),
            HasSixthForm = DataAvailability.Available(true),
            HasSenUnit = DataAvailability.Available(false),
            HasResourcedProvision = DataAvailability.Available(false),
            HeadteacherName = DataAvailability.Available("Mr John Smith"),
            Website = DataAvailability.Available("https://www.testacademy.org.uk"),
            Telephone = DataAvailability.Available("0114 123 4567"),
            Email = DataAvailability.NotAvailable<string>()
        };
    }
}
