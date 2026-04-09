using SAPSec.Core.Features.SimilarSchools.UseCases;
using System.Linq.Expressions;

namespace SAPSec.Core.Tests.Features.SimilarSchools.UseCases;

public class FindSimilarSchoolsTests
{
    private readonly InMemorySimilarSchoolsSecondaryRepository _similarSchoolsRepo = new();
    private readonly InMemoryEstablishmentRepository _establishmentRepo = new();
    private readonly InMemoryKs4PerformanceRepository _performanceRepo = new();
    private readonly FindSimilarSchools _sut;

    public FindSimilarSchoolsTests()
    {
        _sut = new FindSimilarSchools(
            _establishmentRepo,
            _similarSchoolsRepo,
            _performanceRepo);
    }

    [Theory]
    [InlineData("XXX")]
    [InlineData("12345")]
    [InlineData("1234567")]
    public async Task WhenCurrentSchoolUrnIsInvalid_ThrowsValidationException(string invalidUrn)
    {
        var act = async () => await _sut.Execute(Request(invalidUrn));

        var x = await act.Should().ThrowAsync<ValidationException>()
            .Where(e => e.Errors.Contains("Current School URN should be a valid URN"));
    }

    [Fact]
    public async Task WhenCurrentSchoolUrnDoesNotExist_ThrowsNotFoundException()
    {
        var act = async () => await _sut.Execute(Request("999999"));

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*999999*");
    }

    [Fact]
    public async Task WhenCurrentSchoolUrnExistsButSimilarSchoolsDoNotExist_ReturnsEmptyResponse()
    {
        _establishmentRepo.SetupEstablishments([
            new() { URN = "100001", EstablishmentName = "Test School" }
        ]);

        var response = await _sut.Execute(Request("100001"));

        response.Should().NotBeNull();
        response.SchoolName.Should().Be("Test School");
        response.AllResults.Should().BeEmpty();
        response.ResultsPage.Should().BeEmpty();
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

    [Fact]
    public async Task FilterBy_CombinesMultipleFiltersWithLogicalAND()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", Easting = 100000, Northing = 100000 },
            new() { URN = "100002", Easting = 108046, Northing = 100000, UrbanRuralId = "UN1", UrbanRuralName = "Urban: Nearer" },
            new() { URN = "100003", Easting = 108046, Northing = 100000, UrbanRuralId = "UF1", UrbanRuralName = "Urban: Further" },
            new() { URN = "100004", Easting = 108046, Northing = 100000, UrbanRuralId = "RLN1", UrbanRuralName = "Larger rural: Nearer" },
            new() { URN = "100005", Easting = 108046, Northing = 100000, UrbanRuralId = "RLF1", UrbanRuralName = "Larger rural: Further" },
            new() { URN = "100006", Easting = 108047, Northing = 100000, UrbanRuralId = "UN1", UrbanRuralName = "Urban: Nearer" },
            new() { URN = "100007", Easting = 108047, Northing = 100000, UrbanRuralId = "UF1", UrbanRuralName = "Urban: Further" },
            new() { URN = "100008", Easting = 108047, Northing = 100000, UrbanRuralId = "RLN1", UrbanRuralName = "Larger rural: Nearer" },
            new() { URN = "100009", Easting = 108047, Northing = 100000, UrbanRuralId = "RLF1", UrbanRuralName = "Larger rural: Further" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" },
            new() { URN = "100001", NeighbourURN = "100005" },
            new() { URN = "100001", NeighbourURN = "100006" },
            new() { URN = "100001", NeighbourURN = "100007" },
            new() { URN = "100001", NeighbourURN = "100008" },
            new() { URN = "100001", NeighbourURN = "100009" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["dist"] = ["5"],
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

    [Fact]
    public async Task FilterBy_IgnoresInvalidFilterKeys()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", Easting = 100000, Northing = 100000 },
            new() { URN = "100002", Easting = 108046, Northing = 100000, UrbanRuralId = "UN1", UrbanRuralName = "Urban: Nearer" },
            new() { URN = "100003", Easting = 108046, Northing = 100000, UrbanRuralId = "UF1", UrbanRuralName = "Urban: Further" },
            new() { URN = "100004", Easting = 108046, Northing = 100000, UrbanRuralId = "RLN1", UrbanRuralName = "Larger rural: Nearer" },
            new() { URN = "100005", Easting = 108046, Northing = 100000, UrbanRuralId = "RLF1", UrbanRuralName = "Larger rural: Further" },
            new() { URN = "100006", Easting = 108047, Northing = 100000, UrbanRuralId = "UN1", UrbanRuralName = "Urban: Nearer" },
            new() { URN = "100007", Easting = 108047, Northing = 100000, UrbanRuralId = "UF1", UrbanRuralName = "Urban: Further" },
            new() { URN = "100008", Easting = 108047, Northing = 100000, UrbanRuralId = "RLN1", UrbanRuralName = "Larger rural: Nearer" },
            new() { URN = "100009", Easting = 108047, Northing = 100000, UrbanRuralId = "RLF1", UrbanRuralName = "Larger rural: Further" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" },
            new() { URN = "100001", NeighbourURN = "100005" },
            new() { URN = "100001", NeighbourURN = "100006" },
            new() { URN = "100001", NeighbourURN = "100007" },
            new() { URN = "100001", NeighbourURN = "100008" },
            new() { URN = "100001", NeighbourURN = "100009" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["xxx"] = ["1", "2"],
            ["dist"] = ["5"],
            ["ur"] = ["UF1", "RLN1"],
            ["yyy"] = ["3", "4"],
        }));

        response.Should().NotBeNull();

        response.AllResults.Should().Satisfy(
            r => r.SimilarSchool.URN == "100003",
            r => r.SimilarSchool.URN == "100004");

        response.ResultsPage.Should().Satisfy(
            r => r.SimilarSchool.URN == "100003",
            r => r.SimilarSchool.URN == "100004");
    }

    [Theory]
    [InlineData("ur", new[] { "UF1", "RLN1" }, new[] { "100003", "100004" })]
    // Filter key is case insensitive
    [InlineData("UR", new[] { "UF1", "RLN1" }, new[] { "100003", "100004" })]
    // Filter values are case insensitive
    [InlineData("ur", new[] { "uf1", "rln1" }, new[] { "100003", "100004" })]
    // Invalid filter values are ignored
    [InlineData("ur", new[] { "xxx", "RLN1" }, new[] { "100004" })]
    // Duplicate filter values are ignored
    [InlineData("ur", new[] { "UF1", "UF1", "RLN1", "RLN1" }, new[] { "100003", "100004" })]
    // Empty filter values returns all results
    [InlineData("ur", new string[0], new[] { "100002", "100003", "100004", "100005" })]
    // All filter values returns all results
    [InlineData("ur", new[] { "UN1", "UF1", "RLN1", "RLF1" }, new[] { "100002", "100003", "100004", "100005" })]
    public async Task FilterBy_UrbanRural(string filterKey, string[] filterValues, string[] expectedUrns)
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
            [filterKey] = filterValues
        }));

        response.Should().NotBeNull();

        Expression<Func<SimilarSchoolResult, bool>> Expectation(string urn) =>
            r => r.SimilarSchool.URN == urn;

        response.AllResults.Should().Satisfy(expectedUrns.Select(Expectation));
        response.ResultsPage.Should().Satisfy(expectedUrns.Select(Expectation));
    }

    [Theory]
    [InlineData("dist", new[] { "5" }, new[] { "100002" })]
    [InlineData("dist", new[] { "10" }, new[] { "100002", "100003", "100004" })]
    [InlineData("dist", new[] { "25" }, new[] { "100002", "100003", "100004", "100005", "100006" })]
    [InlineData("dist", new[] { "50" }, new[] { "100002", "100003", "100004", "100005", "100006" })]
    [InlineData("dist", new[] { "100" }, new[] { "100002", "100003", "100004", "100005", "100006" })]
    // Filter key is case insensitive
    [InlineData("DIST", new[] { "5" }, new[] { "100002" })]
    // Invalid filter values are ignored
    [InlineData("dist", new[] { "XXX" }, new[] { "100002", "100003", "100004", "100005", "100006", "100007" })]
    [InlineData("dist", new[] { "4" }, new[] { "100002", "100003", "100004", "100005", "100006", "100007" })]
    [InlineData("dist", new[] { "6" }, new[] { "100002", "100003", "100004", "100005", "100006", "100007" })]
    // Duplicate filter values are ignored
    [InlineData("dist", new[] { "5", "5" }, new[] { "100002" })]
    // Empty filter values returns all results
    [InlineData("dist", new string[0], new[] { "100002", "100003", "100004", "100005", "100006", "100007" })]
    // When multiple values provided, last given value is used
    [InlineData("dist", new[] { "25", "10", "5" }, new[] { "100002" })]
    [InlineData("dist", new[] { "XXX", "5" }, new[] { "100002" })]
    public async Task FilterBy_Distance(string filterKey, string[] filterValues, string[] expectedUrns)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", Easting = 100000, Northing = 100000 },
            // 5 miles ~ 8046.72m
            new() { URN = "100002", Easting = 108046, Northing = 100000 },
            new() { URN = "100003", Easting = 100000, Northing = 108047 },
            // 10 miles ~ 16093.44m
            new() { URN = "100004", Easting = 116093, Northing = 100000 },
            new() { URN = "100005", Easting = 100000, Northing = 116094 },
            // 25 miles ~ 40233.6m
            new() { URN = "100006", Easting = 140233, Northing = 100000 },
            new() { URN = "100007", Easting = 100000, Northing = 140234 }
        );

        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" },
            new() { URN = "100001", NeighbourURN = "100005" },
            new() { URN = "100001", NeighbourURN = "100006" },
            new() { URN = "100001", NeighbourURN = "100007" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [filterKey] = filterValues
        }));

        response.Should().NotBeNull();

        Expression<Func<SimilarSchoolResult, bool>> Expectation(string urn) =>
            r => r.SimilarSchool.URN == urn;

        response.AllResults.Should().Satisfy(expectedUrns.Select(Expectation));
        response.ResultsPage.Should().Satisfy(expectedUrns.Select(Expectation));
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