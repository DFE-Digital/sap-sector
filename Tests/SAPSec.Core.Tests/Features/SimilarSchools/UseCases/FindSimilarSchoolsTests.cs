using SAPSec.Core.Features.SimilarSchools.UseCases;

namespace SAPSec.Core.Tests.Features.SimilarSchools.UseCases;

public class FindSimilarSchoolsTests
{
    private readonly InMemorySimilarSchoolsSecondaryRepository _similarSchoolsRepo;
    private readonly InMemoryEstablishmentRepository _establishmentRepo;
    private readonly InMemoryKs4PerformanceRepository _performanceRepo;
    private readonly FindSimilarSchools _sut;

    public FindSimilarSchoolsTests()
    {
        _similarSchoolsRepo = new InMemorySimilarSchoolsSecondaryRepository();
        _establishmentRepo = new InMemoryEstablishmentRepository();
        _performanceRepo = new InMemoryKs4PerformanceRepository();

        _sut = new FindSimilarSchools(
            _establishmentRepo,
            _similarSchoolsRepo,
            _performanceRepo);
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
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002" },
            new() { URN = "100003" },
            new() { URN = "100004" },
            new() { URN = "100005" },
            new() { URN = "100006" },
            new() { URN = "100007" },
            new() { URN = "100008" },
            new() { URN = "100009" },
            new() { URN = "100010" },
            new() { URN = "100011" },
            new() { URN = "100012" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" },
            new() { URN = "100001", NeighbourURN = "100005" },
            new() { URN = "100001", NeighbourURN = "100006" },
            new() { URN = "100001", NeighbourURN = "100007" },
            new() { URN = "100001", NeighbourURN = "100008" },
            new() { URN = "100001", NeighbourURN = "100009" },
            new() { URN = "100001", NeighbourURN = "100010" },
            new() { URN = "100001", NeighbourURN = "100011" },
            new() { URN = "100001", NeighbourURN = "100012" }
        );

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
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002" },
            new() { URN = "100003" },
            new() { URN = "100004" },
            new() { URN = "100005" },
            new() { URN = "100006" },
            new() { URN = "100007" },
            new() { URN = "100008" },
            new() { URN = "100009" },
            new() { URN = "100010" },
            new() { URN = "100011" },
            new() { URN = "100012" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" },
            new() { URN = "100001", NeighbourURN = "100005" },
            new() { URN = "100001", NeighbourURN = "100006" },
            new() { URN = "100001", NeighbourURN = "100007" },
            new() { URN = "100001", NeighbourURN = "100008" },
            new() { URN = "100001", NeighbourURN = "100009" },
            new() { URN = "100001", NeighbourURN = "100010" },
            new() { URN = "100001", NeighbourURN = "100011" },
            new() { URN = "100001", NeighbourURN = "100012" }
        );

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
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", UrbanRuralId = "UN1", UrbanRuralName = "Urban: Nearer" },
            new() { URN = "100003", UrbanRuralId = "UF1", UrbanRuralName = "Urban: Further" },
            new() { URN = "100004", UrbanRuralId = "RLN1", UrbanRuralName = "Larger rural: Nearer" },
            new() { URN = "100005", UrbanRuralId = "RLF1", UrbanRuralName = "Larger rural: Further" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" },
            new() { URN = "100001", NeighbourURN = "100005" }
        );

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
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", Easting = 100000, Northing = 100000 },
            // 5 miles ~ 8046.72m
            new() { URN = "100002", Easting = 108046, Northing = 100000 },
            new() { URN = "100003", Easting = 108047, Northing = 100000 },
            new() { URN = "100004", Easting = 100000, Northing = 108046 },
            new() { URN = "100005", Easting = 100000, Northing = 108047 }
        );

        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" },
            new() { URN = "100001", NeighbourURN = "100005" }
        );

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
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002" },
            new() { URN = "100003" },
            new() { URN = "100004" },
            new() { URN = "100005" }
        );
        _performanceRepo.SetupEstablishmentPerformance(
            new() { Id = "100002", Attainment8_Tot_Est_Current_Num = "10" },
            new() { Id = "100003", Attainment8_Tot_Est_Current_Num = "z" },
            new() { Id = "100004", Attainment8_Tot_Est_Current_Num = "30" },
            new() { Id = "100005", Attainment8_Tot_Est_Current_Num = "20" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" },
            new() { URN = "100001", NeighbourURN = "100005" }
        );

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
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002" },
            new() { URN = "100003" },
            new() { URN = "100004" },
            new() { URN = "100005" }
        );
        _performanceRepo.SetupEstablishmentPerformance(
            new() { Id = "100002", EngMaths59_Tot_Est_Current_Pct = "10" },
            new() { Id = "100003", EngMaths59_Tot_Est_Current_Pct = "z" },
            new() { Id = "100004", EngMaths59_Tot_Est_Current_Pct = "30" },
            new() { Id = "100005", EngMaths59_Tot_Est_Current_Pct = "20" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" },
            new() { URN = "100001", NeighbourURN = "100005" }
        );

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
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002" },
            new() { URN = "100003" },
            new() { URN = "100004" },
            new() { URN = "100005" }
        );
        _performanceRepo.SetupEstablishmentPerformance(
            new() { Id = "100002", Attainment8_Tot_Est_Current_Num = "10" },
            new() { Id = "100003", Attainment8_Tot_Est_Current_Num = "z" },
            new() { Id = "100004", Attainment8_Tot_Est_Current_Num = "30" },
            new() { Id = "100005", Attainment8_Tot_Est_Current_Num = "20" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" },
            new() { URN = "100001", NeighbourURN = "100005" }
        );

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
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002" },
            new() { URN = "100003" },
            new() { URN = "100004" },
            new() { URN = "100005" },
            new() { URN = "100006" },
            new() { URN = "100007" },
            new() { URN = "100008" },
            new() { URN = "100009" },
            new() { URN = "100010" },
            new() { URN = "100011" },
            new() { URN = "100012" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" },
            new() { URN = "100001", NeighbourURN = "100005" },
            new() { URN = "100001", NeighbourURN = "100006" },
            new() { URN = "100001", NeighbourURN = "100007" },
            new() { URN = "100001", NeighbourURN = "100008" },
            new() { URN = "100001", NeighbourURN = "100009" },
            new() { URN = "100001", NeighbourURN = "100010" },
            new() { URN = "100001", NeighbourURN = "100011" },
            new() { URN = "100001", NeighbourURN = "100012" }
        );

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

    private FindSimilarSchoolsRequest Request(string urn, Dictionary<string, IEnumerable<string>>? filterBy = null, string? sortBy = null, int? page = null) =>
        new(urn, filterBy ?? [], sortBy ?? "", page ?? 1);
}