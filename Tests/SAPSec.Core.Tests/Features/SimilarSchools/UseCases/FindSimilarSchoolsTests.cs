using Moq;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Model;

namespace SAPSec.Core.Tests.Features.SimilarSchools.UseCases;

public class FindSimilarSchoolsTests
{
    private readonly Mock<ISimilarSchoolsSecondaryRepository> _repo;
    private readonly FindSimilarSchools _sut;

    public FindSimilarSchoolsTests()
    {
        _repo = new Mock<ISimilarSchoolsSecondaryRepository>();
        _sut = new FindSimilarSchools(_repo.Object);
    }

    [Fact(Skip = "TODO")]
    public async Task WhenCurrentSchoolUrnIsInvalid_ReturnsValidationError()
    {
    }

    [Fact(Skip = "TODO")]
    public async Task WhenCurrentSchoolUrnDoesNotExist_ReturnsNotFoundError()
    {
    }

    [Fact(Skip = "TODO")]
    public async Task WhenCurrentSchoolUrnExistsButSimilarSchoolsDoNotExist_ReturnsEmptyResponse()
    {
    }

    [Fact]
    public async Task AllResults_ContainsAllResults()
    {
        SetupSimilarSchools(CurrentSchool("100001"), [
            SimilarSchool("100002"),
            SimilarSchool("100003"),
            SimilarSchool("100004"),
            SimilarSchool("100005"),
            SimilarSchool("100006"),
            SimilarSchool("100007"),
            SimilarSchool("100008"),
            SimilarSchool("100009"),
            SimilarSchool("100010"),
            SimilarSchool("100011"),
            SimilarSchool("100012"),
        ]);

        var response = await _sut.Execute(Request("100001"));

        response.Should().NotBeNull();

        response.AllResults.Should().Satisfy(
            r => r.SimilarSchool.URN == "100002",
            r => r.SimilarSchool.URN == "100003",
            r => r.SimilarSchool.URN == "100004",
            r => r.SimilarSchool.URN == "100005",
            r => r.SimilarSchool.URN == "100006",
            r => r.SimilarSchool.URN == "100007",
            r => r.SimilarSchool.URN == "100008",
            r => r.SimilarSchool.URN == "100009",
            r => r.SimilarSchool.URN == "100010",
            r => r.SimilarSchool.URN == "100011",
            r => r.SimilarSchool.URN == "100012");
    }

    [Fact]
    public async Task ResultsPage_ContainsFirst10Results()
    {
        SetupSimilarSchools(CurrentSchool("100001"), [
            SimilarSchool("100002"),
            SimilarSchool("100003"),
            SimilarSchool("100004"),
            SimilarSchool("100005"),
            SimilarSchool("100006"),
            SimilarSchool("100007"),
            SimilarSchool("100008"),
            SimilarSchool("100009"),
            SimilarSchool("100010"),
            SimilarSchool("100011"),
            SimilarSchool("100012"),
        ]);

        var response = await _sut.Execute(Request("100001"));

        response.Should().NotBeNull();

        response.ResultsPage.Should().Satisfy(
            r => r.SimilarSchool.URN == "100002",
            r => r.SimilarSchool.URN == "100003",
            r => r.SimilarSchool.URN == "100004",
            r => r.SimilarSchool.URN == "100005",
            r => r.SimilarSchool.URN == "100006",
            r => r.SimilarSchool.URN == "100007",
            r => r.SimilarSchool.URN == "100008",
            r => r.SimilarSchool.URN == "100009",
            r => r.SimilarSchool.URN == "100010",
            r => r.SimilarSchool.URN == "100011");
    }

    [Fact(Skip = "TODO")]
    public async Task FilterBy_IgnoresInvalidFilterKey()
    {
    }

    [Fact(Skip = "TODO")]
    public async Task FilterBy_UrbanRural_IgnoresInvalidFilterValues()
    {
    }

    [Fact]
    public async Task FilterBy_UrbanRural()
    {
        SetupSimilarSchools(CurrentSchool("100001"), [
            SimilarSchool("100002", b => b.WithUrbanRural("UN1", "Urban: Nearer")),
            SimilarSchool("100003", b => b.WithUrbanRural("UF1", "Urban: Further")),
            SimilarSchool("100004", b => b.WithUrbanRural("RLN1", "Larger rural: Nearer")),
            SimilarSchool("100005", b => b.WithUrbanRural("RLF1", "Larger rural: Further"))
        ]);

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["ur"] = ["UF1", "RLN1"]
        }));

        response.Should().NotBeNull();

        response.AllResults.Should().Satisfy(
            r => r.SimilarSchool.URN == "100003",
            r => r.SimilarSchool.URN == "100004");

        response.ResultsPage.Should().Satisfy(
            r => r.SimilarSchool.URN == "100003",
            r => r.SimilarSchool.URN == "100004");
    }

    [Fact(Skip = "TODO")]
    public async Task FilterBy_UrbanRural_OtherFilterOptions()
    {
    }

    [Fact(Skip = "TODO")]
    public async Task FilterBy_Distance_WhenMultipleFilterValuesDefined_UsesTheLastGivenValue()
    {
    }

    [Fact(Skip = "TODO")]
    public async Task FilterBy_Distance_IgnoresInvalidFilterValues()
    {
    }

    [Fact]
    public async Task FilterBy_Distance()
    {
        SetupSimilarSchools(CurrentSchool("100001", b => b.WithCoordinates(100000, 100000)), [
            // 5 miles ~ 8046.72m
            SimilarSchool("100002", b => b.WithCoordinates(108046, 100000)),
            SimilarSchool("100003", b => b.WithCoordinates(108047, 100000)),
            SimilarSchool("100004", b => b.WithCoordinates(100000, 108046)),
            SimilarSchool("100005", b => b.WithCoordinates(100000, 108047))
        ]);

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["dist"] = ["5"]
        }));

        response.Should().NotBeNull();

        response.AllResults.Should().Satisfy(
            r => r.SimilarSchool.URN == "100002",
            r => r.SimilarSchool.URN == "100004");

        response.ResultsPage.Should().Satisfy(
            r => r.SimilarSchool.URN == "100002",
            r => r.SimilarSchool.URN == "100004");
    }

    [Fact(Skip = "TODO")]
    public async Task FilterBy_Distance_OtherFilterOptions()
    {
    }

    [Fact(Skip = "TODO")]
    public async Task FilterBy_Distance_AvailableFilterOptionsBasedOnSimilarSchoolsGroupData()
    {
    }

    [Fact(Skip = "TODO")]
    public async Task FilterBy_Distance_AvailableFilterOptionsSelectedBasedOnFilterValues()
    {
    }

    [Fact(Skip = "TODO")]
    public async Task FilterBy_Distance_WhenCurrentSchoolCoordinatesMissing_DoesNotErrorAndReturnsAllResults()
    {
    }

    [Fact(Skip = "TODO")]
    public async Task FilterBy_Distance_WhenSimilarSchoolCoordinatesMissing_DoesNotErrorAndExcludesSchoolFromResults()
    {
    }

    [Fact(Skip = "TODO")]
    public async Task FilterBy_Distance_WhenSimilarSchoolCoordinatesMissing_DoesExcludesSchoolFromResultsIfFilterNotApplied()
    {
    }

    [Fact(Skip = "TODO")]
    public async Task FilterBy_AllOtherFilters()
    {
    }

    [Fact(Skip = "TODO")]
    public async Task SortBy_IgnoresInvalidSortKey()
    {
    }

    [Fact]
    public async Task SortBy_Attainment8()
    {
        SetupSimilarSchools(CurrentSchool("100001"), [
            SimilarSchool("100002", b => b.WithAttainment8(DataWithAvailability.Available(10M))),
            SimilarSchool("100003", b => b.WithAttainment8(DataWithAvailability.NotAvailable<decimal>())),
            SimilarSchool("100004", b => b.WithAttainment8(DataWithAvailability.Available(30M))),
            SimilarSchool("100005", b => b.WithAttainment8(DataWithAvailability.Available(20M)))
        ]);

        var response = await _sut.Execute(Request("100001", sortBy: "Att8"));

        response.Should().NotBeNull();

        response.AllResults.Should().Equal(
            ["100004", "100005", "100002", "100003"],
            (r, urn) => r.SimilarSchool.URN == urn);

        response.ResultsPage.Should().Equal(
            ["100004", "100005", "100002", "100003"],
            (r, urn) => r.SimilarSchool.URN == urn);
    }

    [Fact]
    public async Task SortBy_EnglishMaths()
    {
        SetupSimilarSchools(CurrentSchool("100001"), [
            SimilarSchool("100002", b => b.WithEnglishMaths(DataWithAvailability.Available(10M))),
            SimilarSchool("100003", b => b.WithEnglishMaths(DataWithAvailability.NotAvailable<decimal>())),
            SimilarSchool("100004", b => b.WithEnglishMaths(DataWithAvailability.Available(30M))),
            SimilarSchool("100005", b => b.WithEnglishMaths(DataWithAvailability.Available(20M)))
        ]);

        var response = await _sut.Execute(Request("100001", sortBy: "EngMat"));

        response.Should().NotBeNull();

        response.AllResults.Should().Equal(
            ["100004", "100005", "100002", "100003"],
            (r, urn) => r.SimilarSchool.URN == urn);

        response.ResultsPage.Should().Equal(
            ["100004", "100005", "100002", "100003"],
            (r, urn) => r.SimilarSchool.URN == urn);
    }

    [Fact]
    public async Task SortBy_DefaultsToAttainment8()
    {
        SetupSimilarSchools(CurrentSchool("100001"), [
            SimilarSchool("100002", b => b.WithAttainment8(DataWithAvailability.Available(10M))),
            SimilarSchool("100003", b => b.WithAttainment8(DataWithAvailability.NotAvailable<decimal>())),
            SimilarSchool("100004", b => b.WithAttainment8(DataWithAvailability.Available(30M))),
            SimilarSchool("100005", b => b.WithAttainment8(DataWithAvailability.Available(20M)))
        ]);

        var response = await _sut.Execute(Request("100001"));

        response.Should().NotBeNull();

        response.AllResults.Should().Equal(
            ["100004", "100005", "100002", "100003"],
            (r, urn) => r.SimilarSchool.URN == urn);

        response.ResultsPage.Should().Equal(
            ["100004", "100005", "100002", "100003"],
            (r, urn) => r.SimilarSchool.URN == urn);
    }

    [Fact(Skip = "TODO")]
    public async Task SortBy_AllOtherSortOptions()
    {
    }

    [Fact]
    public async Task Pagination_WhenPageIsInvalid_DefaultsToFirstPage()
    {
    }

    [Fact]
    public async Task Pagination_WhenPageIsGreaterThanTotalPageCount_DefaultsToLastPage()
    {
    }

    [Fact]
    public async Task Pagination_SelectsAppropriatePageOfResults()
    {
        SetupSimilarSchools(CurrentSchool("100001"), [
            SimilarSchool("100002"),
            SimilarSchool("100003"),
            SimilarSchool("100004"),
            SimilarSchool("100005"),
            SimilarSchool("100006"),
            SimilarSchool("100007"),
            SimilarSchool("100008"),
            SimilarSchool("100009"),
            SimilarSchool("100010"),
            SimilarSchool("100011"),
            SimilarSchool("100012"),
        ]);

        var response = await _sut.Execute(Request("100001", page: 2));

        response.Should().NotBeNull();

        response.ResultsPage.TotalCount.Should().Be(11);
        response.ResultsPage.TotalPages.Should().Be(2);
        response.ResultsPage.ItemsPerPage.Should().Be(10);
        response.ResultsPage.CurrentPage.Should().Be(2);
        response.ResultsPage.Count.Should().Be(1);

        response.ResultsPage.Should().Satisfy(
            r => r.SimilarSchool.URN == "100012");
    }

    [Fact(Skip = "TODO")]
    public async Task Pagination_WorksWithFiltering()
    {
    }

    [Fact(Skip = "TODO")]
    public async Task Pagination_WorksWithSorting()
    {
    }

    [Fact(Skip = "TODO")]
    public async Task Pagination_WorksWithFilteringAndSorting()
    {
    }

    private void SetupSimilarSchools(SimilarSchool currentSchool, List<SimilarSchool> similarSchools)
    {
        _repo.Setup(r => r.GetSimilarSchoolsGroupAsync(currentSchool.URN))
            .ReturnsAsync((currentSchool, similarSchools.AsReadOnly()));
    }

    private FindSimilarSchoolsRequest Request(string urn, Dictionary<string, IEnumerable<string>>? filterBy = null, string? sortBy = null, int? page = null) =>
        new(urn, filterBy ?? [], sortBy ?? "", page ?? 1);

    private SimilarSchool CurrentSchool(string urn, Func<SimilarSchoolBuilder, SimilarSchoolBuilder> build = null)
    {
        build ??= b => b;
        var builder = new SimilarSchoolBuilder(urn);
        return build(builder).Build();
    }

    private SimilarSchool SimilarSchool(string urn, Func<SimilarSchoolBuilder, SimilarSchoolBuilder> build = null)
    {
        build ??= b => b;
        var builder = new SimilarSchoolBuilder(urn);
        return build(builder).Build();
    }
}

