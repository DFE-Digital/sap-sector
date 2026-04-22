using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Model;

namespace SAPSec.Core.Tests.Features.SimilarSchools.UseCases;

public class FindSimilarSchoolsTests
{
    private readonly InMemorySimilarSchoolsSecondaryRepository _similarSchoolsRepo = new();
    private readonly InMemoryEstablishmentRepository _establishmentRepo = new();
    private readonly InMemoryKs4PerformanceRepository _performanceRepo = new();
    private readonly InMemoryAbsenceRepository _absenceRepo = new();
    private readonly FindSimilarSchools _sut;

    public FindSimilarSchoolsTests()
    {
        _sut = new FindSimilarSchools(
            _establishmentRepo,
            _similarSchoolsRepo,
            _performanceRepo,
            _absenceRepo);
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

        response.SchoolName.Should().Be("Test School");
        response.AllResults.Should().BeEmpty();
        response.ResultsPage.Should().BeEmpty();
    }

    [Fact]
    public async Task ResultsContainOnlySchoolsInSimilarSchoolsGroup()
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
            new() { URN = "100001", NeighbourURN = "100006" }
        );

        var response = await _sut.Execute(Request("100001"));

        response.AllResults.Select(r => r.SimilarSchool.URN)
            .Should().BeEquivalentTo(
                "100002",
                "100003",
                "100004",
                "100005",
                "100006");

        response.ResultsPage.Select(r => r.SimilarSchool.URN)
            .Should().BeEquivalentTo(
                "100002",
                "100003",
                "100004",
                "100005",
                "100006");
    }

    [Fact]
    public async Task SortOptions()
    {
        _establishmentRepo.SetupEstablishments([
            new() { URN = "100001", EstablishmentName = "Test School" }
        ]);

        var response = await _sut.Execute(Request("100001"));

        response.SortOptions.Should().SatisfyRespectively(
            o => o.Should().BeEquivalentTo(new { Key = "Att8", Name = "Attainment 8", Selected = true }),
            o => o.Should().BeEquivalentTo(new { Key = "EngMat", Name = "English and maths GCSEs (Grade 5 and above)", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "EngLang", Name = "English language GCSE (Grade 5 and above)", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "EngLit", Name = "English literature GCSE (Grade 5 and above)", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Maths", Name = "Mathematics GCSE (Grade 5 and above)", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "CombSci", Name = "Combined science (double award) GCSE (Grade 5-5 and above)", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Bio", Name = "Biology GCSE (Grade 5 and above)", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Chem", Name = "Chemistry GCSE (Grade 5 and above)", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Phys", Name = "Physics GCSE (Grade 5 and above)", Selected = false })
        );
    }

    [Fact]
    public async Task FilterOptions_WhenZeroSimilarSchools_ReturnsEmptyFilterOptions()
    {
        _establishmentRepo.SetupEstablishments([
            new() { URN = "100001", Easting = 100000, Northing = 100000 }
        ]);
        _similarSchoolsRepo.SetupGroups([]);

        var response = await _sut.Execute(Request("100001"));

        // Numeric range filters do not have options so they are always included
        response.FilterOptions.Select(f => f.Key)
            .Should().Equal("sciu", "oar", "par");
    }

    [Fact]
    public async Task FilterOptions_WhenOnlyOneSimilarSchool_ReturnsEmptyFilterOptions()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", Easting = 100000, Northing = 100000 },
            new() { URN = "100002", Easting = 300000, Northing = 100000 }
        );
        _similarSchoolsRepo.SetupGroups([
            new() { URN = "100001", NeighbourURN = "100002" }
        ]);

        var response = await _sut.Execute(Request("100001"));

        // Numeric range filters do not have options so they are always included
        response.FilterOptions.Select(f => f.Key)
            .Should().Equal("sciu", "oar", "par");
    }

    [Fact]
    public async Task FilterBy_CombinesMultipleFiltersWithLogicalAND()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", Easting = 100000, Northing = 100000 },
            new() { URN = "100002", Easting = 108046, Northing = 100000, UrbanRuralId = "UN1" },
            new() { URN = "100003", Easting = 108046, Northing = 100000, UrbanRuralId = "UF1" },
            new() { URN = "100004", Easting = 108046, Northing = 100000, UrbanRuralId = "RLN1" },
            new() { URN = "100005", Easting = 108046, Northing = 100000, UrbanRuralId = "RLF1" },
            new() { URN = "100006", Easting = 108047, Northing = 100000, UrbanRuralId = "UN1" },
            new() { URN = "100007", Easting = 108047, Northing = 100000, UrbanRuralId = "UF1" },
            new() { URN = "100008", Easting = 108047, Northing = 100000, UrbanRuralId = "RLN1" },
            new() { URN = "100009", Easting = 108047, Northing = 100000, UrbanRuralId = "RLF1" }
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

        response.AllResults.Select(r => r.SimilarSchool.URN)
            .Should().BeEquivalentTo("100003", "100004");

        response.ResultsPage.Select(r => r.SimilarSchool.URN)
            .Should().BeEquivalentTo("100003", "100004");
    }

    [Fact]
    public async Task FilterBy_IgnoresInvalidFilterKeys()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", Easting = 100000, Northing = 100000 },
            new() { URN = "100002", Easting = 108046, Northing = 100000, UrbanRuralId = "UN1" },
            new() { URN = "100003", Easting = 108046, Northing = 100000, UrbanRuralId = "UF1" },
            new() { URN = "100004", Easting = 108046, Northing = 100000, UrbanRuralId = "RLN1" },
            new() { URN = "100005", Easting = 108046, Northing = 100000, UrbanRuralId = "RLF1" },
            new() { URN = "100006", Easting = 108047, Northing = 100000, UrbanRuralId = "UN1" },
            new() { URN = "100007", Easting = 108047, Northing = 100000, UrbanRuralId = "UF1" },
            new() { URN = "100008", Easting = 108047, Northing = 100000, UrbanRuralId = "RLN1" },
            new() { URN = "100009", Easting = 108047, Northing = 100000, UrbanRuralId = "RLF1" }
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

        response.AllResults.Select(r => r.SimilarSchool.URN)
            .Should().BeEquivalentTo("100003", "100004");
    }

    [Theory]
    [InlineData("dist", new[] { "5" }, new[] { "100002" })]
    [InlineData("dist", new[] { "10" }, new[] { "100002", "100003", "100004" })]
    [InlineData("dist", new[] { "25" }, new[] { "100002", "100003", "100004", "100005", "100006" })]
    [InlineData("dist", new[] { "50" }, new[] { "100002", "100003", "100004", "100005", "100006", "100007", "100008" })]
    [InlineData("dist", new[] { "100" }, new[] { "100002", "100003", "100004", "100005", "100006", "100007", "100008", "100009", "100010" })]
    [InlineData("dist", new[] { "all" }, new[] { "100002", "100003", "100004", "100005", "100006", "100007", "100008", "100009", "100010", "100011" })]
    // Filter key is case insensitive
    [InlineData("DIST", new[] { "5" }, new[] { "100002" })]
    // Invalid filter values are ignored
    [InlineData("dist", new[] { "XXX" }, new[] { "100002", "100003", "100004", "100005", "100006", "100007", "100008", "100009", "100010", "100011" })]
    [InlineData("dist", new[] { "4" }, new[] { "100002", "100003", "100004", "100005", "100006", "100007", "100008", "100009", "100010", "100011" })]
    [InlineData("dist", new[] { "6" }, new[] { "100002", "100003", "100004", "100005", "100006", "100007", "100008", "100009", "100010", "100011" })]
    // Duplicate filter values are ignored
    [InlineData("dist", new[] { "5", "5" }, new[] { "100002" })]
    // Empty filter values returns all results
    [InlineData("dist", new string[0], new[] { "100002", "100003", "100004", "100005", "100006", "100007", "100008", "100009", "100010", "100011" })]
    // When multiple values provided, last given value is used
    [InlineData("dist", new[] { "25", "10", "5" }, new[] { "100002" })]
    [InlineData("dist", new[] { "XXX", "5" }, new[] { "100002" })]
    public async Task FilterBy_Distance(string filterKey, string[] filterValues, string[] expectedUrns)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", Easting = 100000, Northing = 100000 },
            new() { URN = "100002", Easting = 108046, Northing = 100000 },
            // 5 miles ~ 8046.72m
            new() { URN = "100003", Easting = 100000, Northing = 108047 },
            new() { URN = "100004", Easting = 116093, Northing = 100000 },
            // 10 miles ~ 16093.44m
            new() { URN = "100005", Easting = 100000, Northing = 116094 },
            new() { URN = "100006", Easting = 140233, Northing = 100000 },
            // 25 miles ~ 40233.6m
            new() { URN = "100007", Easting = 100000, Northing = 140234 },
            new() { URN = "100008", Easting = 180467, Northing = 100000 },
            // 50 miles ~ 80467.2m
            new() { URN = "100009", Easting = 100000, Northing = 180468 },
            new() { URN = "100010", Easting = 260934, Northing = 100000 },
            // 100 miles ~ 160934.4m
            new() { URN = "100011", Easting = 100000, Northing = 260935 }
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
            new() { URN = "100001", NeighbourURN = "100011" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [filterKey] = filterValues
        }));

        response.AllResults.Select(r => r.SimilarSchool.URN)
            .Should().BeEquivalentTo(expectedUrns);
    }

    [Fact]
    public async Task FilterBy_Distance_WhenCurrentSchoolCoordinatesMissing_DoesNotErrorAndReturnsAllResults()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", Easting = 100000, Northing = 180000 },
            new() { URN = "100003", Easting = 180000, Northing = 100000 }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["dist"] = ["100"]
        }));

        response.AllResults.Select(r => r.SimilarSchool.URN)
            .Should().BeEquivalentTo("100002", "100003");
    }

    [Fact]
    public async Task FilterBy_Distance_WhenSimilarSchoolCoordinatesMissing_DoesNotErrorAndExcludesSchoolFromResults()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", Easting = 100000, Northing = 100000 },
            new() { URN = "100002", Easting = 100000, Northing = 180000 },
            new() { URN = "100003", Easting = 180000 },
            new() { URN = "100004", Northing = 180000 },
            new() { URN = "100005" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" },
            new() { URN = "100001", NeighbourURN = "100005" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["dist"] = ["100"]
        }));

        response.AllResults.Select(r => r.SimilarSchool.URN)
            .Should().BeEquivalentTo("100002");
    }

    [Fact]
    public async Task FilterBy_Distance_FilterOptions()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", Easting = 100000, Northing = 100000 },
            new() { URN = "100002", Easting = 108046, Northing = 100000 },
            // 5 miles ~ 8046.72m
            new() { URN = "100003", Easting = 100000, Northing = 108047 },
            new() { URN = "100004", Easting = 116093, Northing = 100000 },
            // 10 miles ~ 16093.44m
            new() { URN = "100005", Easting = 100000, Northing = 116094 },
            new() { URN = "100006", Easting = 140233, Northing = 100000 },
            // 25 miles ~ 40233.6m
            new() { URN = "100007", Easting = 100000, Northing = 140234 },
            new() { URN = "100008", Easting = 180467, Northing = 100000 },
            // 50 miles ~ 80467.2m
            new() { URN = "100009", Easting = 100000, Northing = 180468 },
            new() { URN = "100010", Easting = 260934, Northing = 100000 },
            // 100 miles ~ 160934.4m
            new() { URN = "100011", Easting = 100000, Northing = 260935 }
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
            new() { URN = "100001", NeighbourURN = "100011" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["dist"] = ["10"]
        }));

        response.FilterOptions.Should().SatisfyRespectively(
            f => f.Should().BeEquivalentTo(new
            {
                Key = "dist",
                Name = "Distance",
                Options = new[]
                {
                    new { Key = "5", Name = "Up to 5 miles", Selected = false, Count = 1 },
                    new { Key = "10", Name = "Up to 10 miles", Selected = true, Count = 3 },
                    new { Key = "25", Name = "Up to 25 miles", Selected = false, Count = 5 },
                    new { Key = "50", Name = "Up to 50 miles", Selected = false, Count = 7 },
                    new { Key = "100", Name = "Up to 100 miles", Selected = false, Count = 9 },
                    new { Key = "All", Name = "All schools", Selected = false, Count = 10 },
                },

            }),
            // Numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Fact]
    public async Task FilterBy_Distance_FilterOptions_LimitedToSimilarSchoolsGroup()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", Easting = 100000, Northing = 100000 },
            new() { URN = "100002", Easting = 108046, Northing = 100000 },
            // 5 miles ~ 8046.72m
            new() { URN = "100003", Easting = 100000, Northing = 108047 },
            new() { URN = "100004", Easting = 116093, Northing = 100000 },
            // 10 miles ~ 16093.44m
            new() { URN = "100005", Easting = 100000, Northing = 116094 },
            new() { URN = "100006", Easting = 140233, Northing = 100000 },
            // 25 miles ~ 40233.6m
            new() { URN = "100007", Easting = 100000, Northing = 140234 },
            new() { URN = "100008", Easting = 180467, Northing = 100000 },
            // 50 miles ~ 80467.2m
            new() { URN = "100009", Easting = 100000, Northing = 180468 },
            new() { URN = "100010", Easting = 260934, Northing = 100000 },
            // 100 miles ~ 160934.4m
            new() { URN = "100011", Easting = 100000, Northing = 260935 }
        );

        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100005" },
            new() { URN = "100001", NeighbourURN = "100011" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["dist"] = ["10"]
        }));

        response.FilterOptions.Should().SatisfyRespectively(
            f => f.Should().BeEquivalentTo(new
            {
                Key = "dist",
                Name = "Distance",
                Options = new[]
                {
                    new { Key = "5", Name = "Up to 5 miles", Selected = false, Count = 1 },
                    new { Key = "10", Name = "Up to 10 miles", Selected = true, Count = 1 },
                    new { Key = "25", Name = "Up to 25 miles", Selected = false, Count = 2 },
                    new { Key = "50", Name = "Up to 50 miles", Selected = false, Count = 2 },
                    new { Key = "100", Name = "Up to 100 miles", Selected = false, Count = 2 },
                    new { Key = "All", Name = "All schools", Selected = false, Count = 3 },
                },
                CurrentSchoolValue = (DataWithAvailability<string>?)null
            }),
            // Numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Fact]
    public async Task FilterBy_Distance_FilterOptions_WhenOnlyOneFilterOptionAvailable_FilterIsExcluded()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", Easting = 100000, Northing = 100000 },
            new() { URN = "100002", Easting = 100000, Northing = 300000 },
            new() { URN = "100003", Easting = 300000, Northing = 100000 }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" }
        );

        var response = await _sut.Execute(Request("100001"));

        response.FilterOptions.Select(f => f.Key)
            // Numeric range filters are always included
            .Should().Equal("sciu", "oar", "par");
    }

    [Fact]
    public async Task FilterBy_Distance_FilterOptions_WhenMoreThanOneFilterOptionAvailable_FilterIsIncluded()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", Easting = 100000, Northing = 100000 },
            new() { URN = "100002", Easting = 100000, Northing = 260000 },
            new() { URN = "100003", Easting = 260000, Northing = 100000 }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" }
        );

        var response = await _sut.Execute(Request("100001"));

        response.FilterOptions.Should().SatisfyRespectively(
            f => f.Should().BeEquivalentTo(new
            {
                Key = "dist",
                Name = "Distance",
                Options = new[]
                {
                    new { Key = "100", Name = "Up to 100 miles", Selected = false, Count = 2 },
                    new { Key = "All", Name = "All schools", Selected = false, Count = 2 }
                }
            }),
            // Numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Fact]
    public async Task FilterBy_Distance_FilterOptions_WhenCurrentSchoolCoordinatesMissing_DoesNotErrorAndFilterIsExcluded()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", Easting = 100000, Northing = 180000 },
            new() { URN = "100003", Easting = 180000, Northing = 100000 }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" }
        );

        var response = await _sut.Execute(Request("100001"));

        response.FilterOptions.Should().SatisfyRespectively(
            // Numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Fact]
    public async Task FilterBy_Distance_FilterOptions_WhenSimilarSchoolCoordinatesMissing_DoesNotErrorAndExcludesOptions()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", Easting = 100000, Northing = 100000 },
            new() { URN = "100002", Easting = 100000, Northing = 180000 },
            new() { URN = "100003", Easting = 180000 },
            new() { URN = "100004", Northing = 180000 },
            new() { URN = "100005" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" },
            new() { URN = "100001", NeighbourURN = "100005" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["dist"] = ["100"]
        }));

        response.FilterOptions.Should().SatisfyRespectively(
            f => f.Should().BeEquivalentTo(new
            {
                Key = "dist",
                Name = "Distance",
                Options = new[]
                {
                    new { Key = "50", Name = "Up to 50 miles", Selected = false, Count = 1 },
                    new { Key = "100", Name = "Up to 100 miles", Selected = true, Count = 1 },
                    new { Key = "All", Name = "All schools", Selected = false, Count = 4 }
                }
            }),
            // Numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Theory]
    [InlineData("reg", new[] { "2", "3" }, new[] { "100003", "100004" })]
    // Filter key is case insensitive
    [InlineData("REG", new[] { "2", "3" }, new[] { "100003", "100004" })]
    // Invalid filter values are ignored
    [InlineData("reg", new[] { "xxx", "3" }, new[] { "100004" })]
    // Duplicate filter values are ignored
    [InlineData("reg", new[] { "2", "2", "3", "3" }, new[] { "100003", "100004" })]
    // Empty filter values returns all results
    [InlineData("reg", new string[0], new[] { "100002", "100003", "100004", "100005" })]
    // All filter values returns all results
    [InlineData("reg", new[] { "1", "2", "3", "4" }, new[] { "100002", "100003", "100004", "100005" })]
    public async Task FilterBy_Region(string filterKey, string[] filterValues, string[] expectedUrns)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", RegionId = "1" },
            new() { URN = "100003", RegionId = "2" },
            new() { URN = "100004", RegionId = "3" },
            new() { URN = "100005", RegionId = "4" }
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

        response.AllResults.Select(r => r.SimilarSchool.URN)
            .Should().BeEquivalentTo(expectedUrns);
    }

    [Fact]
    public async Task FilterBy_Region_FiltersOutEstablishmentsWithMissingRegionId()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", RegionId = "1", RegionName = "West Midlands" },
            new() { URN = "100003", RegionId = "", RegionName = "West Midlands" },
            new() { URN = "100004", RegionName = "West Midlands" },
            new() { URN = "100005" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" },
            new() { URN = "100001", NeighbourURN = "100005" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["reg"] = ["1"]
        }));

        response.AllResults.Select(r => r.SimilarSchool.URN)
            .Should().BeEquivalentTo(["100002"]);
    }

    [Fact]
    public async Task FilterBy_Region_FilterOptions()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", RegionId = "4", RegionName = "East Midlands" },
            new() { URN = "100002", RegionId = "1", RegionName = "West Midlands" },
            new() { URN = "100003", RegionId = "1", RegionName = "West Midlands" },
            new() { URN = "100004", RegionId = "2", RegionName = "Yorkshire" },
            new() { URN = "100005", RegionId = "3", RegionName = "London" },
            new() { URN = "100006", RegionId = "3", RegionName = "London" },
            new() { URN = "100007", RegionId = "3", RegionName = "London" },
            new() { URN = "100008", RegionId = "4", RegionName = "East Midlands" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" },
            new() { URN = "100001", NeighbourURN = "100005" },
            new() { URN = "100001", NeighbourURN = "100006" },
            new() { URN = "100001", NeighbourURN = "100007" },
            new() { URN = "100001", NeighbourURN = "100008" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["reg"] = ["1", "2"]
        }));

        response.FilterOptions.Should().SatisfyRespectively(
            f => f.Should().BeEquivalentTo(new
            {
                Key = "reg",
                Name = "Region",
                Options = new[]
                {
                    new { Key = "4", Name = "East Midlands", Selected = false, Count = 1 },
                    new { Key = "3", Name = "London", Selected = false, Count = 3 },
                    new { Key = "1", Name = "West Midlands", Selected = true, Count = 2 },
                    new { Key = "2", Name = "Yorkshire", Selected = true, Count = 1 }
                },
                CurrentSchoolValue = DataWithAvailability.Available("East Midlands")
            }),
            // Numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Fact]
    public async Task FilterBy_Region_FilterOptions_LimitedToSimilarSchoolsGroup()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", RegionId = "1", RegionName = "West Midlands" },
            new() { URN = "100003", RegionId = "1", RegionName = "West Midlands" },
            new() { URN = "100004", RegionId = "2", RegionName = "Yorkshire" },
            new() { URN = "100005", RegionId = "3", RegionName = "London" },
            new() { URN = "100006", RegionId = "3", RegionName = "London" },
            new() { URN = "100007", RegionId = "3", RegionName = "London" },
            new() { URN = "100008", RegionId = "4", RegionName = "East Midlands" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100005" },
            new() { URN = "100001", NeighbourURN = "100008" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["reg"] = ["1", "2"]
        }));

        response.FilterOptions.Should().SatisfyRespectively(
            f => f.Should().BeEquivalentTo(new
            {
                Key = "reg",
                Name = "Region",
                Options = new[]
                {
                    new { Key = "1", Name = "West Midlands", Selected = true, Count = 1 },
                    new { Key = "3", Name = "London", Selected = false, Count = 1 },
                    new { Key = "4", Name = "East Midlands", Selected = false, Count = 1 }
                }
            }),
            // Numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Fact]
    public async Task FilterBy_Region_FilterOptions_WhenOnlyOneFilterOptionAvailable_FilterIsExcluded()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", RegionId = "1", RegionName = "West Midlands" },
            new() { URN = "100003", RegionId = "1", RegionName = "West Midlands" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" }
        );

        var response = await _sut.Execute(Request("100001"));

        response.FilterOptions.Select(f => f.Key)
            // Numeric range filters are always included
            .Should().Equal("sciu", "oar", "par");
    }

    [Fact]
    public async Task FilterBy_Region_FilterOptions_WhenMoreThanOneFilterOptionAvailable_FilterIsIncluded()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", RegionId = "1", RegionName = "West Midlands" },
            new() { URN = "100003", RegionId = "2", RegionName = "Yorkshire" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" }
        );

        var response = await _sut.Execute(Request("100001"));

        response.FilterOptions.Should().SatisfyRespectively(
            f => f.Should().BeEquivalentTo(new
            {
                Key = "reg",
                Name = "Region",
                Options = new[]
                {
                    new { Key = "1", Name = "West Midlands", Selected = false, Count = 1 },
                    new { Key = "2", Name = "Yorkshire", Selected = false, Count = 1 }
                }
            }),
            // Numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Theory]
    [InlineData("", "West Midlands")]
    [InlineData("1", "")]
    [InlineData("", "")]
    public async Task FilterBy_Region_FilterOptions_WhenCurrentSchoolRegionDataMissing_CurrentSchoolValueIsNotAvailable(string regionId, string regionName)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", RegionId = regionId, RegionName = regionName },
            new() { URN = "100002", RegionId = "1", RegionName = "West Midlands" },
            new() { URN = "100003", RegionId = "2", RegionName = "Yorkshire" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" }
        );

        var response = await _sut.Execute(Request("100001"));

        response.FilterOptions.Should().SatisfyRespectively(
            f => f.Should().BeEquivalentTo(new
            {
                Key = "reg",
                Name = "Region",
                Options = new[]
                {
                    new { Key = "1", Name = "West Midlands", Selected = false, Count = 1 },
                    new { Key = "2", Name = "Yorkshire", Selected = false, Count = 1 }
                },
                CurrentSchoolValue = DataWithAvailability.NotAvailable<string>()
            }),
            // Numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
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
            new() { URN = "100002", UrbanRuralId = "UN1" },
            new() { URN = "100003", UrbanRuralId = "UF1" },
            new() { URN = "100004", UrbanRuralId = "RLN1" },
            new() { URN = "100005", UrbanRuralId = "RLF1" }
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

        response.AllResults.Select(r => r.SimilarSchool.URN)
            .Should().BeEquivalentTo(expectedUrns);
    }

    [Fact]
    public async Task FilterBy_UrbanRural_FiltersOutEstablishmentsWithMissingUrbanRuralId()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", UrbanRuralId = "UN1", UrbanRuralName = "Urban: Nearer" },
            new() { URN = "100003", UrbanRuralId = "", UrbanRuralName = "Urban: Nearer" },
            new() { URN = "100004", UrbanRuralName = "Urban: Nearer" },
            new() { URN = "100005" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" },
            new() { URN = "100001", NeighbourURN = "100005" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["ur"] = ["UN1"]
        }));

        response.AllResults.Select(r => r.SimilarSchool.URN)
            .Should().BeEquivalentTo(["100002"]);
    }

    [Fact]
    public async Task FilterBy_UrbanRural_FilterOptions()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", UrbanRuralId = "RLF1", UrbanRuralName = "Larger rural: Further" },
            new() { URN = "100002", UrbanRuralId = "UN1", UrbanRuralName = "Urban: Nearer" },
            new() { URN = "100003", UrbanRuralId = "UN1", UrbanRuralName = "Urban: Nearer" },
            new() { URN = "100004", UrbanRuralId = "UF1", UrbanRuralName = "Urban: Further" },
            new() { URN = "100005", UrbanRuralId = "RLN1", UrbanRuralName = "Larger rural: Nearer" },
            new() { URN = "100006", UrbanRuralId = "RLN1", UrbanRuralName = "Larger rural: Nearer" },
            new() { URN = "100007", UrbanRuralId = "RLN1", UrbanRuralName = "Larger rural: Nearer" },
            new() { URN = "100008", UrbanRuralId = "RLF1", UrbanRuralName = "Larger rural: Further" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" },
            new() { URN = "100001", NeighbourURN = "100005" },
            new() { URN = "100001", NeighbourURN = "100006" },
            new() { URN = "100001", NeighbourURN = "100007" },
            new() { URN = "100001", NeighbourURN = "100008" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["ur"] = ["UN1", "UF1"]
        }));

        response.FilterOptions.Should().SatisfyRespectively(
            f => f.Should().BeEquivalentTo(new
            {
                Key = "ur",
                Name = "Urban or rural",
                Options = new[]
                {
                    new { Key = "UN1", Name = "Urban: Nearer", Selected = true, Count = 2 },
                    new { Key = "UF1", Name = "Urban: Further", Selected = true, Count = 1 },
                    new { Key = "RLN1", Name = "Larger rural: Nearer", Selected = false, Count = 3 },
                    new { Key = "RLF1", Name = "Larger rural: Further", Selected = false, Count = 1 }
                },
                CurrentSchoolValue = DataWithAvailability.Available("Larger rural: Further")
            }),
            // Numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Fact]
    public async Task FilterBy_UrbanRural_FilterOptions_LimitedToSimilarSchoolsGroup()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", UrbanRuralId = "UN1", UrbanRuralName = "Urban: Nearer" },
            new() { URN = "100003", UrbanRuralId = "UN1", UrbanRuralName = "Urban: Nearer" },
            new() { URN = "100004", UrbanRuralId = "UF1", UrbanRuralName = "Urban: Further" },
            new() { URN = "100005", UrbanRuralId = "RLN1", UrbanRuralName = "Larger rural: Nearer" },
            new() { URN = "100006", UrbanRuralId = "RLN1", UrbanRuralName = "Larger rural: Nearer" },
            new() { URN = "100007", UrbanRuralId = "RLN1", UrbanRuralName = "Larger rural: Nearer" },
            new() { URN = "100008", UrbanRuralId = "RLF1", UrbanRuralName = "Larger rural: Further" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100005" },
            new() { URN = "100001", NeighbourURN = "100008" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["ur"] = ["UN1", "UF1"]
        }));

        response.FilterOptions.Should().SatisfyRespectively(
            f => f.Should().BeEquivalentTo(new
            {
                Key = "ur",
                Name = "Urban or rural",
                Options = new[]
                {
                    new { Key = "UN1", Name = "Urban: Nearer", Selected = true, Count = 1 },
                    new { Key = "RLN1", Name = "Larger rural: Nearer", Selected = false, Count = 1 },
                    new { Key = "RLF1", Name = "Larger rural: Further", Selected = false, Count = 1 }
                }
            }),
            // Numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Fact]
    public async Task FilterBy_UrbanRural_FilterOptions_WhenOnlyOneFilterOptionAvailable_FilterIsExcluded()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", UrbanRuralId = "UN1", UrbanRuralName = "Urban: Nearer" },
            new() { URN = "100003", UrbanRuralId = "UN1", UrbanRuralName = "Urban: Nearer" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" }
        );

        var response = await _sut.Execute(Request("100001"));

        response.FilterOptions.Select(f => f.Key)
            // Numeric range filters are always included
            .Should().Equal("sciu", "oar", "par");
    }

    [Fact]
    public async Task FilterBy_UrbanRural_FilterOptions_WhenMoreThanOneFilterOptionAvailable_FilterIsIncluded()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", UrbanRuralId = "UN1", UrbanRuralName = "Urban: Nearer" },
            new() { URN = "100003", UrbanRuralId = "UF1", UrbanRuralName = "Urban: Further" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" }
        );

        var response = await _sut.Execute(Request("100001"));

        response.FilterOptions.Should().SatisfyRespectively(
            f => f.Should().BeEquivalentTo(new
            {
                Key = "ur",
                Name = "Urban or rural",
                Options = new[]
                {
                    new { Key = "UN1", Name = "Urban: Nearer", Selected = false, Count = 1 },
                    new { Key = "UF1", Name = "Urban: Further", Selected = false, Count = 1 }
                }
            }),
            // Numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Theory]
    [InlineData("", "Urban: Nearer")]
    [InlineData("UN1", "")]
    [InlineData("", "")]
    public async Task FilterBy_UrbanRural_FilterOptions_WhenCurrentSchoolUrbanRuralDataMissing_CurrentSchoolValueIsNotAvailable(string urbanRuralId, string urbanRuralName)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", UrbanRuralId = urbanRuralId, UrbanRuralName = urbanRuralName },
            new() { URN = "100002", UrbanRuralId = "UN1", UrbanRuralName = "Urban: Nearer" },
            new() { URN = "100003", UrbanRuralId = "UF1", UrbanRuralName = "Urban: Further" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" }
        );

        var response = await _sut.Execute(Request("100001"));

        response.FilterOptions.Should().SatisfyRespectively(
            f => f.Should().BeEquivalentTo(new
            {
                Key = "ur",
                Name = "Urban or rural",
                Options = new[]
                {
                    new { Key = "UN1", Name = "Urban: Nearer", Selected = false, Count = 1 },
                    new { Key = "UF1", Name = "Urban: Further", Selected = false, Count = 1 }
                },
                CurrentSchoolValue = DataWithAvailability.NotAvailable<string>()
            }),
            // Numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Theory]
    [InlineData("poe", new[] { "2", "3" }, new[] { "100003", "100004" })]
    // Filter key is case insensitive
    [InlineData("POE", new[] { "2", "3" }, new[] { "100003", "100004" })]
    // Invalid filter values are ignored
    [InlineData("poe", new[] { "xxx", "3" }, new[] { "100004" })]
    // Duplicate filter values are ignored
    [InlineData("poe", new[] { "2", "2", "3", "3" }, new[] { "100003", "100004" })]
    // Empty filter values returns all results
    [InlineData("poe", new string[0], new[] { "100002", "100003", "100004", "100005" })]
    // All filter values returns all results
    [InlineData("poe", new[] { "1", "2", "3", "4" }, new[] { "100002", "100003", "100004", "100005" })]
    public async Task FilterBy_PhaseOfEducation(string filterKey, string[] filterValues, string[] expectedUrns)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", PhaseOfEducationId = "1" },
            new() { URN = "100003", PhaseOfEducationId = "2" },
            new() { URN = "100004", PhaseOfEducationId = "3" },
            new() { URN = "100005", PhaseOfEducationId = "4" }
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

        response.AllResults.Select(r => r.SimilarSchool.URN)
            .Should().BeEquivalentTo(expectedUrns);

        response.ResultsPage.Select(r => r.SimilarSchool.URN)
            .Should().BeEquivalentTo(expectedUrns);
    }

    [Fact]
    public async Task FilterBy_PhaseOfEducation_FiltersOutEstablishmentsWithMissingPhaseOfEducationId()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", PhaseOfEducationId = "1", PhaseOfEducationName = "Secondary" },
            new() { URN = "100003", PhaseOfEducationId = "", PhaseOfEducationName = "Secondary" },
            new() { URN = "100004", PhaseOfEducationName = "Secondary" },
            new() { URN = "100005" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" },
            new() { URN = "100001", NeighbourURN = "100005" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["poe"] = ["1"]
        }));

        response.AllResults.Select(r => r.SimilarSchool.URN)
            .Should().BeEquivalentTo(["100002"]);
    }

    [Fact]
    public async Task FilterBy_PhaseOfEducation_FilterOptions()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", PhaseOfEducationId = "4", PhaseOfEducationName = "Primary" },
            new() { URN = "100002", PhaseOfEducationId = "1", PhaseOfEducationName = "Secondary" },
            new() { URN = "100003", PhaseOfEducationId = "1", PhaseOfEducationName = "Secondary" },
            new() { URN = "100004", PhaseOfEducationId = "2", PhaseOfEducationName = "All-through" },
            new() { URN = "100005", PhaseOfEducationId = "3", PhaseOfEducationName = "Middle" },
            new() { URN = "100006", PhaseOfEducationId = "3", PhaseOfEducationName = "Middle" },
            new() { URN = "100007", PhaseOfEducationId = "3", PhaseOfEducationName = "Middle" },
            new() { URN = "100008", PhaseOfEducationId = "4", PhaseOfEducationName = "Primary" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" },
            new() { URN = "100001", NeighbourURN = "100005" },
            new() { URN = "100001", NeighbourURN = "100006" },
            new() { URN = "100001", NeighbourURN = "100007" },
            new() { URN = "100001", NeighbourURN = "100008" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["poe"] = ["1", "2"]
        }));

        response.FilterOptions.Should().SatisfyRespectively(
            f => f.Should().BeEquivalentTo(new
            {
                Key = "poe",
                Name = "Phase of education",
                Options = new[]
                {
                    new { Key = "4", Name = "Primary", Selected = false, Count = 1 },
                    new { Key = "3", Name = "Middle", Selected = false, Count = 3 },
                    new { Key = "1", Name = "Secondary", Selected = true, Count = 2 },
                    new { Key = "2", Name = "All-through", Selected = true, Count = 1 }
                },
                CurrentSchoolValue = DataWithAvailability.Available("Primary")
            }),
            // Numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Fact]
    public async Task FilterBy_PhaseOfEducation_FilterOptions_LimitedToSimilarSchoolsGroup()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", PhaseOfEducationId = "1", PhaseOfEducationName = "Secondary" },
            new() { URN = "100003", PhaseOfEducationId = "1", PhaseOfEducationName = "Secondary" },
            new() { URN = "100004", PhaseOfEducationId = "2", PhaseOfEducationName = "All-through" },
            new() { URN = "100005", PhaseOfEducationId = "3", PhaseOfEducationName = "Middle" },
            new() { URN = "100006", PhaseOfEducationId = "3", PhaseOfEducationName = "Middle" },
            new() { URN = "100007", PhaseOfEducationId = "3", PhaseOfEducationName = "Middle" },
            new() { URN = "100008", PhaseOfEducationId = "4", PhaseOfEducationName = "Primary" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100005" },
            new() { URN = "100001", NeighbourURN = "100008" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["poe"] = ["1", "2"]
        }));

        response.FilterOptions.Should().SatisfyRespectively(
            f => f.Should().BeEquivalentTo(new
            {
                Key = "poe",
                Name = "Phase of education",
                Options = new[]
                {
                    new { Key = "1", Name = "Secondary", Selected = true, Count = 1 },
                    new { Key = "3", Name = "Middle", Selected = false, Count = 1 },
                    new { Key = "4", Name = "Primary", Selected = false, Count = 1 }
                }
            }),
            // Numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Fact]
    public async Task FilterBy_PhaseOfEducation_FilterOptions_WhenOnlyOneFilterOptionAvailable_FilterIsExcluded()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", PhaseOfEducationId = "1", PhaseOfEducationName = "Secondary" },
            new() { URN = "100003", PhaseOfEducationId = "1", PhaseOfEducationName = "Secondary" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" }
        );

        var response = await _sut.Execute(Request("100001"));

        response.FilterOptions.Select(f => f.Key)
            // Numeric range filters are always included
            .Should().Equal("sciu", "oar", "par");
    }

    [Fact]
    public async Task FilterBy_PhaseOfEducation_FilterOptions_WhenMoreThanOneFilterOptionAvailable_FilterIsIncluded()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", PhaseOfEducationId = "1", PhaseOfEducationName = "Secondary" },
            new() { URN = "100003", PhaseOfEducationId = "2", PhaseOfEducationName = "All-through" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" }
        );

        var response = await _sut.Execute(Request("100001"));

        response.FilterOptions.Should().SatisfyRespectively(
            f => f.Should().BeEquivalentTo(new
            {
                Key = "poe",
                Name = "Phase of education",
                Options = new[]
                {
                    new { Key = "1", Name = "Secondary", Selected = false, Count = 1 },
                    new { Key = "2", Name = "All-through", Selected = false, Count = 1 }
                }
            }),
            // Numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Theory]
    [InlineData("", "Secondary")]
    [InlineData("1", "")]
    [InlineData("", "")]
    public async Task FilterBy_PhaseOfEducation_FilterOptions_WhenCurrentSchoolRegionDataMissing_CurrentSchoolValueIsNotAvailable(string regionId, string regionName)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", PhaseOfEducationId = regionId, PhaseOfEducationName = regionName },
            new() { URN = "100002", PhaseOfEducationId = "1", PhaseOfEducationName = "Secondary" },
            new() { URN = "100003", PhaseOfEducationId = "2", PhaseOfEducationName = "All-through" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" }
        );

        var response = await _sut.Execute(Request("100001"));

        response.FilterOptions.Should().SatisfyRespectively(
            f => f.Should().BeEquivalentTo(new
            {
                Key = "poe",
                Name = "Phase of education",
                Options = new[]
                {
                    new { Key = "1", Name = "Secondary", Selected = false, Count = 1 },
                    new { Key = "2", Name = "All-through", Selected = false, Count = 1 }
                },
                CurrentSchoolValue = DataWithAvailability.NotAvailable<string>()
            }),
            // Numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Theory]
    [InlineData("sciu_f", new[] { "50" }, "sciu_t", new[] { "51" }, new string[0])]
    [InlineData("sciu_f", new[] { "50" }, "", new string[0], new string[0])]
    [InlineData("", new string[0], "sciu_t", new[] { "51" }, new string[0])]
    [InlineData("sciu_f", new[] { "XXX" }, "", new string[0], new[] { "sciu_f:Enter a numeric value" })]
    [InlineData("sciu_f", new[] { "10..0" }, "", new string[0], new[] { "sciu_f:Enter a numeric value" })]
    [InlineData("sciu_f", new[] { "..10..0" }, "", new string[0], new[] { "sciu_f:Enter a numeric value" })]
    [InlineData("", new string[0], "sciu_t", new[] { "XXX" }, new[] { "sciu_t:Enter a numeric value" })]
    [InlineData("", new string[0], "sciu_t", new[] { "10..0" }, new[] { "sciu_t:Enter a numeric value" })]
    [InlineData("", new string[0], "sciu_t", new[] { "..10..0" }, new[] { "sciu_t:Enter a numeric value" })]
    [InlineData("sciu_f", new[] { "XXX" }, "sciu_t", new[] { "XXX" }, new[] { "sciu_f:Enter a numeric value", "sciu_t:Enter a numeric value" })]
    [InlineData("sciu_f", new[] { "20" }, "sciu_t", new[] { "10" }, new[] { "sciu:The From value must be lower than the To value" })]
    [InlineData("sciu_f", new[] { "-1" }, "sciu_t", new[] { "99999999999999999999999999999999999999999999999999999999999" }, new[] { "sciu_f:Enter a value between 0 and 999", "sciu_t:Enter a value between 0 and 999" })]
    public async Task FilterBy_SchoolCapacityInUse_ValidationErrors(string fromFilterKey, string[] fromFilterValues, string toFilterKey, string[] toFilterValues, string[] expectedValidationErrors)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            // Less than 50%
            new() { URN = "100002", TotalCapacity = 50, TotalPupils = 24 },
            new() { URN = "100003", TotalCapacity = 100, TotalPupils = 49 },
            new() { URN = "100004", TotalCapacity = 200, TotalPupils = 99 },
            // 50%
            new() { URN = "100005", TotalCapacity = 50, TotalPupils = 25 },
            new() { URN = "100006", TotalCapacity = 100, TotalPupils = 50 },
            new() { URN = "100007", TotalCapacity = 200, TotalPupils = 100 },
            // 51%
            new() { URN = "100008", TotalCapacity = 100, TotalPupils = 51 },
            new() { URN = "100009", TotalCapacity = 200, TotalPupils = 102 },
            // More than 51%
            new() { URN = "100010", TotalCapacity = 50, TotalPupils = 26 },
            new() { URN = "100011", TotalCapacity = 100, TotalPupils = 52 }
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

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [fromFilterKey] = fromFilterValues,
            [toFilterKey] = toFilterValues
        }));

        response.ValidationErrors.Select(e => $"{e.Key}:{e.Message}")
            .Should().BeEquivalentTo(expectedValidationErrors);

        response.FilterOptions.OfType<SimilarSchoolsNumericRangeAvailableFilter>()
            .SelectMany(f => f.ValidationErrors.Select(e => $"{e.Key}:{e.Message}"))
            .Should().BeEquivalentTo(expectedValidationErrors);
    }

    [Theory]
    [InlineData("sciu_f", new[] { "50" }, "sciu_t", new[] { "51" }, new[] { "100005", "100006", "100007", "100008", "100009" })]
    [InlineData("sciu_f", new[] { "50" }, "", new string[0], new[] { "100005", "100006", "100007", "100008", "100009", "100010", "100011", "100012" })]
    [InlineData("", new string[0], "sciu_t", new[] { "51" }, new[] { "100002", "100003", "100004", "100005", "100006", "100007", "100008", "100009" })]
    // Filter key is case insensitive
    [InlineData("SCIU_F", new[] { "50" }, "sciu_T", new[] { "51" }, new[] { "100005", "100006", "100007", "100008", "100009" })]
    // Duplicate filter values are ignored (uses last given value)
    [InlineData("sciu_f", new[] { "0", "50" }, "sciu_t", new[] { "100", "51" }, new[] { "100005", "100006", "100007", "100008", "100009" })]
    // Empty filter values returns all results
    [InlineData("", new string[0], "", new string[0], new[] { "100002", "100003", "100004", "100005", "100006", "100007", "100008", "100009", "100010", "100011", "100012" })]
    public async Task FilterBy_SchoolCapacityInUse(string fromFilterKey, string[] fromFilterValues, string toFilterKey, string[] toFilterValues, string[] expectedUrns)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            // Less than 50%
            new() { URN = "100002", TotalCapacity = 50, TotalPupils = 24 },
            new() { URN = "100003", TotalCapacity = 100, TotalPupils = 49 },
            new() { URN = "100004", TotalCapacity = 200, TotalPupils = 99 },
            // 50%
            new() { URN = "100005", TotalCapacity = 50, TotalPupils = 25 },
            new() { URN = "100006", TotalCapacity = 100, TotalPupils = 50 },
            new() { URN = "100007", TotalCapacity = 200, TotalPupils = 100 },
            // 51%
            new() { URN = "100008", TotalCapacity = 100, TotalPupils = 51 },
            new() { URN = "100009", TotalCapacity = 200, TotalPupils = 102 },
            // More than 51%
            new() { URN = "100010", TotalCapacity = 50, TotalPupils = 26 },
            new() { URN = "100011", TotalCapacity = 100, TotalPupils = 52 },
            new() { URN = "100012", TotalCapacity = 200, TotalPupils = 103 }
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

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [fromFilterKey] = fromFilterValues,
            [toFilterKey] = toFilterValues
        }));

        response.AllResults.Select(r => r.SimilarSchool.URN)
            .Should().BeEquivalentTo(expectedUrns);
    }

    [Fact]
    public async Task FilterBy_SchoolCapacityInUse_FiltersOutEstablishmentsWithMissingTotalCapacityOrTotalPupils()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", TotalCapacity = 100, TotalPupils = 50 },
            new() { URN = "100003", TotalCapacity = 100, TotalPupils = null },
            new() { URN = "100004", TotalCapacity = null, TotalPupils = 50 },
            new() { URN = "100005", TotalCapacity = null, TotalPupils = null },
            new() { URN = "100006" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" },
            new() { URN = "100001", NeighbourURN = "100005" },
            new() { URN = "100001", NeighbourURN = "100006" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["sciu_f"] = ["0"],
            ["sciu_t"] = [""]
        }));

        response.AllResults.Select(r => r.SimilarSchool.URN)
            .Should().BeEquivalentTo(["100002"]);
    }

    [Fact]
    public async Task FilterBy_SchoolCapacityInUse_DoesNotApplyFilterIfBothFromAndToFieldsAreEmpty()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", TotalCapacity = 100, TotalPupils = 50 },
            new() { URN = "100003", TotalCapacity = 100, TotalPupils = null },
            new() { URN = "100004", TotalCapacity = null, TotalPupils = 50 },
            new() { URN = "100005", TotalCapacity = null, TotalPupils = null },
            new() { URN = "100006" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" },
            new() { URN = "100001", NeighbourURN = "100005" },
            new() { URN = "100001", NeighbourURN = "100006" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["sciu_f"] = [""],
            ["sciu_t"] = [""]
        }));

        response.AllResults.Select(r => r.SimilarSchool.URN)
            .Should().BeEquivalentTo("100002", "100003", "100004", "100005", "100006");
    }

    [Fact]
    public async Task FilterBy_SchoolCapacityInUse_FilterOptions()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", TotalCapacity = 50, TotalPupils = 24 },
            new() { URN = "100002", TotalCapacity = 50, TotalPupils = 25 }
        );
        _similarSchoolsRepo.SetupGroups([
            new() { URN = "100001", NeighbourURN = "100002" }
        ]);

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["sciu_f"] = ["50"],
            ["sciu_t"] = ["51"]
        }));

        response.FilterOptions.Should().SatisfyRespectively(
            f => f.Should().BeEquivalentTo(new
            {
                Key = "sciu",
                Name = "School capacity in use",
                CurrentSchoolValue = DataWithAvailability.Available("48.0%"),
                From = new { Key = "sciu_f", Value = "50" },
                To = new { Key = "sciu_t", Value = "51" },
            }),
            // Other numeric range filters are always included
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Theory]
    [InlineData("sciu_f", new[] { "50" }, "sciu_t", new[] { "51.0" }, "50", "51.0")]
    [InlineData("sciu_f", new[] { "50.0099999" }, "sciu_t", new[] { "51.000000001" }, "50.01", "51.00")]
    [InlineData("sciu_f", new string[0], "sciu_t", new[] { "" }, "", "")]
    public async Task FilterBy_SchoolCapacityInUse_FilterOptions_RoundsLongTrailingDecimals(string fromFilterKey, string[] fromFilterValues, string toFilterKey, string[] toFilterValues, string expectedFromValue, string expectedToValue)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", TotalCapacity = 50, TotalPupils = 24 },
            new() { URN = "100002", TotalCapacity = 50, TotalPupils = 25 }
        );
        _similarSchoolsRepo.SetupGroups([
            new() { URN = "100001", NeighbourURN = "100002" }
        ]);

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [fromFilterKey] = fromFilterValues,
            [toFilterKey] = toFilterValues
        }));

        response.FilterOptions.Should().SatisfyRespectively(
            f => f.Should().BeEquivalentTo(new
            {
                Key = "sciu",
                Name = "School capacity in use",
                CurrentSchoolValue = DataWithAvailability.Available("48.0%"),
                From = new { Key = "sciu_f", Value = expectedFromValue },
                To = new { Key = "sciu_t", Value = expectedToValue },
            }),
            // Other numeric range filters are always included
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(null, 50)]
    [InlineData(100, null)]
    public async Task FilterBy_SchoolCapacityInUse_FilterOptions_WhenCurrentSchoolTotalCapacityOrTotalPupilsMissing_CurrentSchoolValueIsNotAvailable(int? totalCapacity, int? totalPupils)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", TotalCapacity = totalCapacity, TotalPupils = totalPupils },
            new() { URN = "100002", TotalCapacity = 50, TotalPupils = 25 }
        );
        _similarSchoolsRepo.SetupGroups([
            new() { URN = "100001", NeighbourURN = "100002" }
        ]);

        var response = await _sut.Execute(Request("100001"));

        response.FilterOptions.Should().SatisfyRespectively(
            f => f.Should().BeEquivalentTo(new
            {
                Key = "sciu",
                Name = "School capacity in use",
                CurrentSchoolValue = DataWithAvailability.NotAvailable<string>()
            }),
            // Other numeric range filters are always included
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Theory]
    [InlineData("np", new[] { "Has Nursery Classes" }, new[] { "100002" })]
    [InlineData("np", new[] { "No Nursery Classes" }, new[] { "100003" })]
    [InlineData("np", new[] { "Not applicable" }, new[] { "100004" })]
    [InlineData("np", new[] { "Has Nursery Classes", "No Nursery Classes" }, new[] { "100002", "100003" })]
    // Filter key is case insensitive
    [InlineData("NP", new[] { "Has Nursery Classes", "No Nursery Classes" }, new[] { "100002", "100003" })]
    // Filter values are case insensitive
    [InlineData("np", new[] { "HAS NURSERY CLASSES", "no nursery classes" }, new[] { "100002", "100003" })]
    // Invalid filter values are ignored
    [InlineData("np", new[] { "xxx", "Has Nursery Classes" }, new[] { "100002" })]
    // Duplicate filter values are ignored
    [InlineData("np", new[] { "Has Nursery Classes", "Has Nursery Classes" }, new[] { "100002" })]
    // Empty filter values returns all results
    [InlineData("np", new string[0], new[] { "100002", "100003", "100004" })]
    // All filter values returns all results
    [InlineData("np", new[] { "Has Nursery Classes", "No Nursery Classes", "Not applicable" }, new[] { "100002", "100003", "100004" })]
    public async Task FilterBy_NurseryProvision(string filterKey, string[] filterValues, string[] expectedUrns)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", NurseryProvisionName = "Has Nursery Classes" },
            new() { URN = "100003", NurseryProvisionName = "No Nursery Classes" },
            new() { URN = "100004", NurseryProvisionName = "Not applicable" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [filterKey] = filterValues
        }));

        response.AllResults.Select(r => r.SimilarSchool.URN)
            .Should().BeEquivalentTo(expectedUrns);
    }

    [Fact]
    public async Task FilterBy_NurseryProvision_FilterOptions()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", NurseryProvisionName = "Has Nursery Classes" },
            new() { URN = "100002", NurseryProvisionName = "Has Nursery Classes" },
            new() { URN = "100003", NurseryProvisionName = "Has Nursery Classes" },
            new() { URN = "100004", NurseryProvisionName = "No Nursery Classes" },
            new() { URN = "100005", NurseryProvisionName = "No Nursery Classes" },
            new() { URN = "100006", NurseryProvisionName = "Not applicable" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" },
            new() { URN = "100001", NeighbourURN = "100005" },
            new() { URN = "100001", NeighbourURN = "100006" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["np"] = ["Has Nursery Classes", "No Nursery Classes"]
        }));

        response.FilterOptions.Should().SatisfyRespectively(
            // Numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Should().BeEquivalentTo(new
            {
                Key = "np",
                Name = "Nursery provision",
                Options = new[]
                {
                    new { Key = "Has Nursery Classes", Name = "Has Nursery Classes", Selected = true, Count = 2 },
                    new { Key = "No Nursery Classes", Name = "No Nursery Classes", Selected = true, Count = 2 },
                    new { Key = "Not applicable", Name = "Not applicable", Selected = false, Count = 1 }
                },
                CurrentSchoolValue = DataWithAvailability.Available("Has Nursery Classes")
            }),
            // Numeric range filters are always included
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Fact]
    public async Task FilterBy_NurseryProvision_FilterOptions_LimitedToSimilarSchoolsGroup()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", NurseryProvisionName = "Has Nursery Classes" },
            new() { URN = "100002", NurseryProvisionName = "Has Nursery Classes" },
            new() { URN = "100003", NurseryProvisionName = "Has Nursery Classes" },
            new() { URN = "100004", NurseryProvisionName = "No Nursery Classes" },
            new() { URN = "100005", NurseryProvisionName = "No Nursery Classes" },
            new() { URN = "100006", NurseryProvisionName = "Not applicable" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100004" },
            new() { URN = "100001", NeighbourURN = "100006" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["np"] = ["Has Nursery Classes", "No Nursery Classes"]
        }));

        response.FilterOptions.Should().SatisfyRespectively(
            // Numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Should().BeEquivalentTo(new
            {
                Key = "np",
                Name = "Nursery provision",
                Options = new[]
                {
                    new { Key = "Has Nursery Classes", Name = "Has Nursery Classes", Selected = true, Count = 1 },
                    new { Key = "No Nursery Classes", Name = "No Nursery Classes", Selected = true, Count = 1 },
                    new { Key = "Not applicable", Name = "Not applicable", Selected = false, Count = 1 }
                },
                CurrentSchoolValue = DataWithAvailability.Available("Has Nursery Classes")
            }),
            // Numeric range filters are always included
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Fact]
    public async Task FilterBy_NurseryProvision_FilterOptions_WhenOnlyOneFilterOptionAvailable_FilterIsExcluded()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", NurseryProvisionName = "Has Nursery Classes" },
            new() { URN = "100003", NurseryProvisionName = "Has Nursery Classes" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" }
        );

        var response = await _sut.Execute(Request("100001"));

        response.FilterOptions.Select(f => f.Key)
            // Numeric range filters are always included
            .Should().Equal("sciu", "oar", "par");
    }

    [Fact]
    public async Task FilterBy_NurseryProvision_FilterOptions_WhenMoreThanOneFilterOptionAvailable_FilterIsIncluded()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", NurseryProvisionName = "Has Nursery Classes" },
            new() { URN = "100003", NurseryProvisionName = "No Nursery Classes" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" }
        );

        var response = await _sut.Execute(Request("100001"));

        response.FilterOptions.Should().SatisfyRespectively(
            // Numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Should().BeEquivalentTo(new
            {
                Key = "np",
                Name = "Nursery provision",
                Options = new[]
                {
                    new { Key = "Has Nursery Classes", Name = "Has Nursery Classes", Selected = false, Count = 1 },
                    new { Key = "No Nursery Classes", Name = "No Nursery Classes", Selected = false, Count = 1 }
                }
            }),
            // Numeric range filters are always included
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Theory]
    [InlineData("Has Nursery Classes", "Has Nursery Classes")]
    [InlineData("No Nursery Classes", "No Nursery Classes")]
    [InlineData("Not applicable", "Not applicable")]
    [InlineData("", null)]
    [InlineData("Some other value", "Some other value")]
    public async Task FilterBy_NurseryProvision_FilterOptions_CurrentSchoolValue(string nurseryProvisionName, string? expectedCurrentSchoolValue)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", NurseryProvisionName = nurseryProvisionName },
            new() { URN = "100002", NurseryProvisionName = "Has Nursery Classes" },
            new() { URN = "100003", NurseryProvisionName = "No Nursery Classes" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" }
        );

        var response = await _sut.Execute(Request("100001"));

        response.FilterOptions.Should().SatisfyRespectively(
            // Numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Should().BeEquivalentTo(new
            {
                Key = "np",
                Name = "Nursery provision",
                CurrentSchoolValue = expectedCurrentSchoolValue is null
                    ? DataWithAvailability.NotAvailable<string>()
                    : DataWithAvailability.Available(expectedCurrentSchoolValue)
            }),
            // Numeric range filters are always included
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Theory]
    [InlineData("sf", new[] { "1" }, new[] { "100002" })]
    [InlineData("sf", new[] { "2" }, new[] { "100003" })]
    // Filter key is case insensitive
    [InlineData("SF", new[] { "2" }, new[] { "100003" })]
    // Duplicate filter values are ignored
    [InlineData("sf", new[] { "2", "2" }, new[] { "100003" })]
    // Empty filter values returns all results
    [InlineData("sf", new string[0], new[] { "100002", "100003" })]
    // All filter values returns all results
    [InlineData("sf", new[] { "1", "2" }, new[] { "100002", "100003" })]
    public async Task FilterBy_SixthFormProvision(string filterKey, string[] filterValues, string[] expectedUrns)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", OfficialSixthFormId = "1" },
            new() { URN = "100003", OfficialSixthFormId = "2" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [filterKey] = filterValues
        }));

        response.AllResults.Select(r => r.SimilarSchool.URN)
            .Should().BeEquivalentTo(expectedUrns);
    }

    [Fact]
    public async Task FilterBy_SixthFormProvision_FiltersOutEstablishmentsWithMissingOfficialSixthFormId()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", OfficialSixthFormId = "1", OfficialSixthFormName = "Has a sixth form" },
            new() { URN = "100003", OfficialSixthFormId = "", OfficialSixthFormName = "Has a sixth form" },
            new() { URN = "100004", OfficialSixthFormName = "Has a sixth form" },
            new() { URN = "100005" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" },
            new() { URN = "100001", NeighbourURN = "100005" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["sf"] = ["1"]
        }));

        response.AllResults.Select(r => r.SimilarSchool.URN)
            .Should().BeEquivalentTo(["100002"]);
    }

    [Fact]
    public async Task FilterBy_SixthFormProvision_FilterOptions()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", OfficialSixthFormId = "1", OfficialSixthFormName = "Has a sixth form" },
            new() { URN = "100002", OfficialSixthFormId = "1", OfficialSixthFormName = "Has a sixth form" },
            new() { URN = "100003", OfficialSixthFormId = "1", OfficialSixthFormName = "Has a sixth form" },
            new() { URN = "100004", OfficialSixthFormId = "2", OfficialSixthFormName = "Does not have a sixth form" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["sf"] = ["1", "2"]
        }));

        response.FilterOptions.Should().SatisfyRespectively(
            // Numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Should().BeEquivalentTo(new
            {
                Key = "sf",
                Name = "Sixth form",
                Options = new[]
                {
                    new { Key = "1", Name = "Has a sixth form", Selected = true, Count = 2 },
                    new { Key = "2", Name = "Does not have a sixth form", Selected = true, Count = 1 }
                },
                CurrentSchoolValue = DataWithAvailability.Available("Has a sixth form")
            }),
            // Numeric range filters are always included
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Fact]
    public async Task FilterBy_SixthFormProvision_FilterOptions_LimitedToSimilarSchoolsGroup()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", OfficialSixthFormId = "1", OfficialSixthFormName = "Has a sixth form" },
            new() { URN = "100003", OfficialSixthFormId = "1", OfficialSixthFormName = "Has a sixth form" },
            new() { URN = "100004", OfficialSixthFormId = "2", OfficialSixthFormName = "Does not have a sixth form" },
            new() { URN = "100005", OfficialSixthFormId = "2", OfficialSixthFormName = "Does not have a sixth form" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100005" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["sf"] = ["1", "2"]
        }));

        response.FilterOptions.Should().SatisfyRespectively(
            // Numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Should().BeEquivalentTo(new
            {
                Key = "sf",
                Name = "Sixth form",
                Options = new[]
                {
                    new { Key = "1", Name = "Has a sixth form", Selected = true, Count = 1 },
                    new { Key = "2", Name = "Does not have a sixth form", Selected = true, Count = 1 }
                }
            }),
            // Numeric range filters are always included
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Fact]
    public async Task FilterBy_SixthFormProvision_FilterOptions_WhenOnlyOneFilterOptionAvailable_FilterIsExcluded()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", OfficialSixthFormId = "1", OfficialSixthFormName = "Has a sixth form" },
            new() { URN = "100003", OfficialSixthFormId = "1", OfficialSixthFormName = "Has a sixth form" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" }
        );

        var response = await _sut.Execute(Request("100001"));

        response.FilterOptions.Select(f => f.Key)
            // Numeric range filters are always included
            .Should().Equal("sciu", "oar", "par");
    }

    [Fact]
    public async Task FilterBy_SixthFormProvision_FilterOptions_WhenMoreThanOneFilterOptionAvailable_FilterIsIncluded()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", OfficialSixthFormId = "1", OfficialSixthFormName = "Has a sixth form" },
            new() { URN = "100003", OfficialSixthFormId = "2", OfficialSixthFormName = "Does not have a sixth form" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" }
        );

        var response = await _sut.Execute(Request("100001"));

        response.FilterOptions.Should().SatisfyRespectively(
            // Numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Should().BeEquivalentTo(new
            {
                Key = "sf",
                Name = "Sixth form",
                Options = new[]
                {
                    new { Key = "1", Name = "Has a sixth form", Selected = false, Count = 1 },
                    new { Key = "2", Name = "Does not have a sixth form", Selected = false, Count = 1 }
                }
            }),
            // Numeric range filters are always included
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Theory]
    [InlineData("", "Has a sixth form")]
    [InlineData("1", "")]
    [InlineData("", "")]
    public async Task FilterBy_SixthFormProvision_FilterOptions_WhenCurrentSchoolDataMissing_CurrentSchoolValueIsNotAvailable(string regionId, string regionName)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", OfficialSixthFormId = regionId, OfficialSixthFormName = regionName },
            new() { URN = "100002", OfficialSixthFormId = "1", OfficialSixthFormName = "Has a sixth form" },
            new() { URN = "100003", OfficialSixthFormId = "2", OfficialSixthFormName = "Does not have a sixth form" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" }
        );

        var response = await _sut.Execute(Request("100001"));

        response.FilterOptions.Should().SatisfyRespectively(
            // Numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Should().BeEquivalentTo(new
            {
                Key = "sf",
                Name = "Sixth form",
                Options = new[]
                {
                    new { Key = "1", Name = "Has a sixth form", Selected = false, Count = 1 },
                    new { Key = "2", Name = "Does not have a sixth form", Selected = false, Count = 1 }
                },
                CurrentSchoolValue = DataWithAvailability.NotAvailable<string>()
            }),
            // Numeric range filters are always included
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Theory]
    [InlineData("ap", new[] { "1" }, new[] { "100002" })]
    [InlineData("ap", new[] { "2" }, new[] { "100003" })]
    // Filter key is case insensitive
    [InlineData("AP", new[] { "2" }, new[] { "100003" })]
    // Duplicate filter values are ignored
    [InlineData("ap", new[] { "2", "2" }, new[] { "100003" })]
    // Empty filter values returns all results
    [InlineData("ap", new string[0], new[] { "100002", "100003" })]
    // All filter values returns all results
    [InlineData("ap", new[] { "1", "2" }, new[] { "100002", "100003" })]
    public async Task FilterBy_AdmissionsPolicy(string filterKey, string[] filterValues, string[] expectedUrns)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", AdmissionsPolicyId = "1" },
            new() { URN = "100003", AdmissionsPolicyId = "2" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [filterKey] = filterValues
        }));

        response.AllResults.Select(r => r.SimilarSchool.URN)
            .Should().BeEquivalentTo(expectedUrns);
    }

    [Fact]
    public async Task FilterBy_AdmissionsPolicy_FiltersOutEstablishmentsWithMissingAdmissionsPolicyId()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", AdmissionsPolicyId = "1", AdmissionsPolicyName = "Selective" },
            new() { URN = "100003", AdmissionsPolicyId = "", AdmissionsPolicyName = "Selective" },
            new() { URN = "100004", AdmissionsPolicyName = "Selective" },
            new() { URN = "100005" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" },
            new() { URN = "100001", NeighbourURN = "100005" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["ap"] = ["1"]
        }));

        response.AllResults.Select(r => r.SimilarSchool.URN)
            .Should().BeEquivalentTo(["100002"]);
    }

    [Fact]
    public async Task FilterBy_AdmissionsPolicy_FilterOptions()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", AdmissionsPolicyId = "1", AdmissionsPolicyName = "Selective" },
            new() { URN = "100002", AdmissionsPolicyId = "1", AdmissionsPolicyName = "Selective" },
            new() { URN = "100003", AdmissionsPolicyId = "1", AdmissionsPolicyName = "Selective" },
            new() { URN = "100004", AdmissionsPolicyId = "2", AdmissionsPolicyName = "Non-selective" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["ap"] = ["1", "2"]
        }));

        response.FilterOptions.Should().SatisfyRespectively(
            // Numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Should().BeEquivalentTo(new
            {
                Key = "ap",
                Name = "Admissions policy",
                Options = new[]
                {
                    new { Key = "1", Name = "Selective", Selected = true, Count = 2 },
                    new { Key = "2", Name = "Non-selective", Selected = true, Count = 1 }
                },
                CurrentSchoolValue = DataWithAvailability.Available("Selective")
            }),
            // Numeric range filters are always included
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Fact]
    public async Task FilterBy_AdmissionsPolicy_FilterOptions_LimitedToSimilarSchoolsGroup()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", AdmissionsPolicyId = "1", AdmissionsPolicyName = "Selective" },
            new() { URN = "100003", AdmissionsPolicyId = "1", AdmissionsPolicyName = "Selective" },
            new() { URN = "100004", AdmissionsPolicyId = "2", AdmissionsPolicyName = "Non-selective" },
            new() { URN = "100005", AdmissionsPolicyId = "2", AdmissionsPolicyName = "Non-selective" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100005" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["ap"] = ["1", "2"]
        }));

        response.FilterOptions.Should().SatisfyRespectively(
            // Numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Should().BeEquivalentTo(new
            {
                Key = "ap",
                Name = "Admissions policy",
                Options = new[]
                {
                    new { Key = "1", Name = "Selective", Selected = true, Count = 1 },
                    new { Key = "2", Name = "Non-selective", Selected = true, Count = 1 }
                }
            }),
            // Numeric range filters are always included
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Fact]
    public async Task FilterBy_AdmissionsPolicy_FilterOptions_WhenOnlyOneFilterOptionAvailable_FilterIsExcluded()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", AdmissionsPolicyId = "1", AdmissionsPolicyName = "Selective" },
            new() { URN = "100003", AdmissionsPolicyId = "1", AdmissionsPolicyName = "Selective" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" }
        );

        var response = await _sut.Execute(Request("100001"));

        response.FilterOptions.Select(f => f.Key)
            // Numeric range filters are always included
            .Should().Equal("sciu", "oar", "par");
    }

    [Fact]
    public async Task FilterBy_AdmissionsPolicy_FilterOptions_WhenMoreThanOneFilterOptionAvailable_FilterIsIncluded()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", AdmissionsPolicyId = "1", AdmissionsPolicyName = "Selective" },
            new() { URN = "100003", AdmissionsPolicyId = "2", AdmissionsPolicyName = "Non-selective" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" }
        );

        var response = await _sut.Execute(Request("100001"));

        response.FilterOptions.Should().SatisfyRespectively(
            // Numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Should().BeEquivalentTo(new
            {
                Key = "ap",
                Name = "Admissions policy",
                Options = new[]
                {
                    new { Key = "1", Name = "Selective", Selected = false, Count = 1 },
                    new { Key = "2", Name = "Non-selective", Selected = false, Count = 1 }
                }
            }),
            // Numeric range filters are always included
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Theory]
    [InlineData("", "Selective")]
    [InlineData("1", "")]
    [InlineData("", "")]
    public async Task FilterBy_AdmissionsPolicy_FilterOptions_WhenCurrentSchoolDataMissing_CurrentSchoolValueIsNotAvailable(string regionId, string regionName)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", AdmissionsPolicyId = regionId, AdmissionsPolicyName = regionName },
            new() { URN = "100002", AdmissionsPolicyId = "1", AdmissionsPolicyName = "Selective" },
            new() { URN = "100003", AdmissionsPolicyId = "2", AdmissionsPolicyName = "Non-selective" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" }
        );

        var response = await _sut.Execute(Request("100001"));

        response.FilterOptions.Should().SatisfyRespectively(
            // Numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Should().BeEquivalentTo(new
            {
                Key = "ap",
                Name = "Admissions policy",
                Options = new[]
                {
                    new { Key = "1", Name = "Selective", Selected = false, Count = 1 },
                    new { Key = "2", Name = "Non-selective", Selected = false, Count = 1 }
                },
                CurrentSchoolValue = DataWithAvailability.NotAvailable<string>()
            }),
            // Numeric range filters are always included
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Theory]
    [InlineData("sp", new[] { "N" }, new[] { "100002", "100003", "100004", "100005", "100006", "999999" })]
    [InlineData("sp", new[] { "R" }, new[] { "100007", "100008", "100009" })]
    [InlineData("sp", new[] { "RS" }, new[] { "100010", "100011", "100012" })]
    [InlineData("sp", new[] { "S" }, new[] { "100013", "100014", "100015" })]
    [InlineData("sp", new[] { "N", "R" }, new[] { "100002", "100003", "100004", "100005", "100006", "100007", "100008", "100009", "999999" })]
    // Filter key is case insensitive
    [InlineData("SP", new[] { "N", "R" }, new[] { "100002", "100003", "100004", "100005", "100006", "100007", "100008", "100009", "999999" })]
    // Filter values are case insensitive
    [InlineData("sp", new[] { "n", "r" }, new[] { "100002", "100003", "100004", "100005", "100006", "100007", "100008", "100009", "999999" })]
    // Invalid filter values are ignored
    [InlineData("sp", new[] { "xxx", "R" }, new[] { "100007", "100008", "100009" })]
    // Duplicate filter values are ignored
    [InlineData("sp", new[] { "R", "R", "RS", "RS" }, new[] { "100007", "100008", "100009", "100010", "100011", "100012" })]
    // Empty filter values returns all results
    [InlineData("sp", new string[0], new[] { "100002", "100003", "100004", "100005", "100006", "100007", "100008", "100009", "100010", "100011", "100012", "100013", "100014", "100015", "999999" })]
    // All filter values returns all results
    [InlineData("sp", new[] { "N", "R", "RS", "S" }, new[] { "100002", "100003", "100004", "100005", "100006", "100007", "100008", "100009", "100010", "100011", "100012", "100013", "100014", "100015", "999999" })]
    public async Task FilterBy_TypeOfSpecialistProvision(string filterKey, string[] filterValues, string[] expectedUrns)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", ResourcedProvisionId = "1", ResourcedProvisionName = "Not applicable" },
            new() { URN = "100003", ResourcedProvisionId = "2", ResourcedProvisionName = "" },
            new() { URN = "100004", ResourcedProvisionId = "3" },
            new() { URN = "100005", ResourcedProvisionId = "" },
            new() { URN = "100006" },
            new() { URN = "100007", ResourcedProvisionId = "4", ResourcedProvisionName = "Resourced provision" },
            new() { URN = "100008", ResourcedProvisionId = "5", ResourcedProvisionName = "Resourced provision" },
            new() { URN = "100009", ResourcedProvisionId = "", ResourcedProvisionName = "Resourced provision" },
            new() { URN = "100010", ResourcedProvisionId = "6", ResourcedProvisionName = "Resourced provision and SEN unit" },
            new() { URN = "100011", ResourcedProvisionId = "7", ResourcedProvisionName = "Resourced provision and SEN unit" },
            new() { URN = "100012", ResourcedProvisionId = "", ResourcedProvisionName = "Resourced provision and SEN unit" },
            new() { URN = "100013", ResourcedProvisionId = "8", ResourcedProvisionName = "SEN unit" },
            new() { URN = "100014", ResourcedProvisionId = "9", ResourcedProvisionName = "SEN unit" },
            new() { URN = "100015", ResourcedProvisionId = "", ResourcedProvisionName = "SEN unit" },
            new() { URN = "999999", ResourcedProvisionId = "999", ResourcedProvisionName = "Some other value" }
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
            new() { URN = "100001", NeighbourURN = "100012" },
            new() { URN = "100001", NeighbourURN = "100013" },
            new() { URN = "100001", NeighbourURN = "100014" },
            new() { URN = "100001", NeighbourURN = "100015" },
            new() { URN = "100001", NeighbourURN = "999999" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [filterKey] = filterValues
        }));

        response.AllResults.Select(r => r.SimilarSchool.URN)
            .Should().BeEquivalentTo(expectedUrns);
    }

    [Fact]
    public async Task FilterBy_TypeOfSpecialistProvision_FilterOptions()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", ResourcedProvisionName = "Resourced provision" },
            new() { URN = "100002", ResourcedProvisionId = "1", ResourcedProvisionName = "Not applicable" },
            new() { URN = "100003", ResourcedProvisionId = "2", ResourcedProvisionName = "" },
            new() { URN = "100004", ResourcedProvisionId = "3" },
            new() { URN = "100005", ResourcedProvisionId = "" },
            new() { URN = "100006" },
            new() { URN = "100007", ResourcedProvisionId = "4", ResourcedProvisionName = "Resourced provision" },
            new() { URN = "100008", ResourcedProvisionId = "5", ResourcedProvisionName = "Resourced provision" },
            new() { URN = "100009", ResourcedProvisionId = "", ResourcedProvisionName = "Resourced provision" },
            new() { URN = "100010", ResourcedProvisionId = "6", ResourcedProvisionName = "Resourced provision and SEN unit" },
            new() { URN = "100011", ResourcedProvisionId = "7", ResourcedProvisionName = "Resourced provision and SEN unit" },
            new() { URN = "100012", ResourcedProvisionId = "", ResourcedProvisionName = "Resourced provision and SEN unit" },
            new() { URN = "100013", ResourcedProvisionId = "8", ResourcedProvisionName = "SEN unit" },
            new() { URN = "100014", ResourcedProvisionId = "9", ResourcedProvisionName = "SEN unit" },
            new() { URN = "100015", ResourcedProvisionId = "", ResourcedProvisionName = "SEN unit" },
            new() { URN = "999999", ResourcedProvisionId = "999", ResourcedProvisionName = "Some other value" }
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
            new() { URN = "100001", NeighbourURN = "100012" },
            new() { URN = "100001", NeighbourURN = "100013" },
            new() { URN = "100001", NeighbourURN = "100014" },
            new() { URN = "100001", NeighbourURN = "100015" },
            new() { URN = "100001", NeighbourURN = "999999" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["sp"] = ["N", "R"]
        }));

        response.FilterOptions.Should().SatisfyRespectively(
            // Numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Should().BeEquivalentTo(new
            {
                Key = "sp",
                Name = "Type of specialist provision",
                Options = new[]
                {
                    new { Key = "R", Name = "Resourced provision", Selected = true, Count = 3 },
                    new { Key = "S", Name = "SEN unit", Selected = false, Count = 3 },
                    new { Key = "RS", Name = "Resourced provision and SEN unit", Selected = false, Count = 3 },
                    new { Key = "N", Name = "No known specialist provision", Selected = true, Count = 6 }
                },
                CurrentSchoolValue = DataWithAvailability.Available("Resourced provision")
            }),
            // Numeric range filters are always included
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Fact]
    public async Task FilterBy_TypeOfSpecialistProvision_FilterOptions_LimitedToSimilarSchoolsGroup()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", ResourcedProvisionName = "Resourced provision" },
            new() { URN = "100002", ResourcedProvisionId = "1", ResourcedProvisionName = "Not applicable" },
            new() { URN = "100003", ResourcedProvisionId = "2", ResourcedProvisionName = "" },
            new() { URN = "100004", ResourcedProvisionId = "3" },
            new() { URN = "100005", ResourcedProvisionId = "" },
            new() { URN = "100006" },
            new() { URN = "100007", ResourcedProvisionId = "4", ResourcedProvisionName = "Resourced provision" },
            new() { URN = "100008", ResourcedProvisionId = "5", ResourcedProvisionName = "Resourced provision" },
            new() { URN = "100009", ResourcedProvisionId = "", ResourcedProvisionName = "Resourced provision" },
            new() { URN = "100010", ResourcedProvisionId = "6", ResourcedProvisionName = "Resourced provision and SEN unit" },
            new() { URN = "100011", ResourcedProvisionId = "7", ResourcedProvisionName = "Resourced provision and SEN unit" },
            new() { URN = "100012", ResourcedProvisionId = "", ResourcedProvisionName = "Resourced provision and SEN unit" },
            new() { URN = "100013", ResourcedProvisionId = "8", ResourcedProvisionName = "SEN unit" },
            new() { URN = "100014", ResourcedProvisionId = "9", ResourcedProvisionName = "SEN unit" },
            new() { URN = "100015", ResourcedProvisionId = "", ResourcedProvisionName = "SEN unit" },
            new() { URN = "999999", ResourcedProvisionId = "999", ResourcedProvisionName = "Some other value" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100005" },
            new() { URN = "100001", NeighbourURN = "100006" },
            new() { URN = "100001", NeighbourURN = "100010" },
            new() { URN = "100001", NeighbourURN = "100011" },
            new() { URN = "100001", NeighbourURN = "100013" },
            new() { URN = "100001", NeighbourURN = "100015" },
            new() { URN = "100001", NeighbourURN = "999999" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["sp"] = ["N", "R"]
        }));

        response.FilterOptions.Should().SatisfyRespectively(
            // Numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Should().BeEquivalentTo(new
            {
                Key = "sp",
                Name = "Type of specialist provision",
                Options = new[]
                {
                    new { Key = "S", Name = "SEN unit", Selected = false, Count = 2 },
                    new { Key = "RS", Name = "Resourced provision and SEN unit", Selected = false, Count = 2 },
                    new { Key = "N", Name = "No known specialist provision", Selected = true, Count = 4 }
                }
            }),
            // Numeric range filters are always included
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Fact]
    public async Task FilterBy_TypeOfSpecialistProvision_FilterOptions_WhenOnlyOneFilterOptionAvailable_FilterIsExcluded()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", ResourcedProvisionId = "6", ResourcedProvisionName = "Resourced provision and SEN unit" },
            new() { URN = "100003", ResourcedProvisionId = "7", ResourcedProvisionName = "Resourced provision and SEN unit" },
            new() { URN = "100004", ResourcedProvisionId = "", ResourcedProvisionName = "Resourced provision and SEN unit" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" }
        );

        var response = await _sut.Execute(Request("100001"));

        response.FilterOptions.Select(f => f.Key)
            // Numeric range filters are always included
            .Should().Equal("sciu", "oar", "par");
    }

    [Fact]
    public async Task FilterBy_TypeOfSpecialistProvision_FilterOptions_WhenMoreThanOneFilterOptionAvailable_FilterIsIncluded()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", ResourcedProvisionId = "1", ResourcedProvisionName = "Not applicable" },
            new() { URN = "100003", ResourcedProvisionId = "4", ResourcedProvisionName = "Resourced provision" },
            new() { URN = "999999", ResourcedProvisionId = "999", ResourcedProvisionName = "Some other value" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "999999" }
        );

        var response = await _sut.Execute(Request("100001"));

        response.FilterOptions.Should().SatisfyRespectively(
            // Numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Should().BeEquivalentTo(new
            {
                Key = "sp",
                Name = "Type of specialist provision",
                Options = new[]
                {
                    new { Key = "R", Name = "Resourced provision", Selected = false, Count = 1 },
                    new { Key = "N", Name = "No known specialist provision", Selected = false, Count = 2 }
                }
            }),
            // Numeric range filters are always included
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Theory]
    [InlineData("", "Not applicable", "No known specialist provision")]
    [InlineData("", "Resourced provision", "Resourced provision")]
    [InlineData("", "Resourced provision and SEN unit", "Resourced provision and SEN unit")]
    [InlineData("", "SEN unit", "SEN unit")]
    [InlineData("1", "", "No known specialist provision")]
    [InlineData("", "", "No known specialist provision")]
    [InlineData("", "Some other value", "No known specialist provision")]
    public async Task FilterBy_TypeOfSpecialistProvision_FilterOptions_CurrentSchoolValue(string resourcedProvisionId, string resourcedProvisionName, string? expectedCurrentSchoolValue)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", ResourcedProvisionId = resourcedProvisionId, ResourcedProvisionName = resourcedProvisionName },
            new() { URN = "100002", ResourcedProvisionId = "1", ResourcedProvisionName = "Not applicable" },
            new() { URN = "100003", ResourcedProvisionId = "4", ResourcedProvisionName = "Resourced provision" },
            new() { URN = "999999", ResourcedProvisionId = "999", ResourcedProvisionName = "Some other value" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "999999" }
        );

        var response = await _sut.Execute(Request("100001"));

        response.FilterOptions.Should().SatisfyRespectively(
            // Numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Should().BeEquivalentTo(new
            {
                Key = "sp",
                Name = "Type of specialist provision",
                CurrentSchoolValue = DataWithAvailability.Available(expectedCurrentSchoolValue)
            }),
            // Numeric range filters are always included
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Theory]
    [InlineData("goe", new[] { "2", "3" }, new[] { "100003", "100004" })]
    // Filter key is case insensitive
    [InlineData("GOE", new[] { "2", "3" }, new[] { "100003", "100004" })]
    // Invalid filter values are ignored
    [InlineData("goe", new[] { "xxx", "3" }, new[] { "100004" })]
    // Duplicate filter values are ignored
    [InlineData("goe", new[] { "2", "2", "3", "3" }, new[] { "100003", "100004" })]
    // Empty filter values returns all results
    [InlineData("goe", new string[0], new[] { "100002", "100003", "100004" })]
    // All filter values returns all results
    [InlineData("goe", new[] { "1", "2", "3" }, new[] { "100002", "100003", "100004" })]
    public async Task FilterBy_GenderOfEntry(string filterKey, string[] filterValues, string[] expectedUrns)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", GenderId = "1" },
            new() { URN = "100003", GenderId = "2" },
            new() { URN = "100004", GenderId = "3" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [filterKey] = filterValues
        }));

        response.AllResults.Select(r => r.SimilarSchool.URN)
            .Should().BeEquivalentTo(expectedUrns);
    }

    [Fact]
    public async Task FilterBy_GenderOfEntry_FiltersOutEstablishmentsWithMissingGenderId()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", GenderId = "1", GenderName = "Boys" },
            new() { URN = "100003", GenderId = "", GenderName = "Boys" },
            new() { URN = "100004", GenderName = "Boys" },
            new() { URN = "100005" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" },
            new() { URN = "100001", NeighbourURN = "100005" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["goe"] = ["1"]
        }));

        response.AllResults.Select(r => r.SimilarSchool.URN)
            .Should().BeEquivalentTo(["100002"]);
    }

    [Fact]
    public async Task FilterBy_GenderOfEntry_FilterOptions()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", GenderId = "2", GenderName = "Girls" },
            new() { URN = "100002", GenderId = "1", GenderName = "Boys" },
            new() { URN = "100003", GenderId = "1", GenderName = "Boys" },
            new() { URN = "100004", GenderId = "2", GenderName = "Girls" },
            new() { URN = "100005", GenderId = "3", GenderName = "Mixed" },
            new() { URN = "100006", GenderId = "3", GenderName = "Mixed" },
            new() { URN = "100007", GenderId = "3", GenderName = "Mixed" }
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
            ["goe"] = ["1", "2"]
        }));

        response.FilterOptions.Should().SatisfyRespectively(
            // Numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Should().BeEquivalentTo(new
            {
                Key = "goe",
                Name = "Gender of entry",
                Options = new[]
                {
                    new { Key = "1", Name = "Boys", Selected = true, Count = 2 },
                    new { Key = "2", Name = "Girls", Selected = true, Count = 1 },
                    new { Key = "3", Name = "Mixed", Selected = false, Count = 3 }
                },
                CurrentSchoolValue = DataWithAvailability.Available("Girls")
            }),
            // Numeric range filters are always included
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Fact]
    public async Task FilterBy_GenderOfEntry_FilterOptions_LimitedToSimilarSchoolsGroup()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", GenderId = "1", GenderName = "Boys" },
            new() { URN = "100003", GenderId = "1", GenderName = "Boys" },
            new() { URN = "100004", GenderId = "2", GenderName = "Girls" },
            new() { URN = "100005", GenderId = "3", GenderName = "Mixed" },
            new() { URN = "100006", GenderId = "3", GenderName = "Mixed" },
            new() { URN = "100007", GenderId = "3", GenderName = "Mixed" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100005" },
            new() { URN = "100001", NeighbourURN = "100008" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["goe"] = ["1", "2"]
        }));

        response.FilterOptions.Should().SatisfyRespectively(
            // Numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Should().BeEquivalentTo(new
            {
                Key = "goe",
                Name = "Gender of entry",
                Options = new[]
                {
                    new { Key = "1", Name = "Boys", Selected = true, Count = 1 },
                    new { Key = "3", Name = "Mixed", Selected = false, Count = 1 }
                }
            }),
            // Numeric range filters are always included
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Fact]
    public async Task FilterBy_GenderOfEntry_FilterOptions_WhenOnlyOneFilterOptionAvailable_FilterIsExcluded()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", GenderId = "1", GenderName = "Boys" },
            new() { URN = "100003", GenderId = "1", GenderName = "Boys" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" }
        );

        var response = await _sut.Execute(Request("100001"));

        response.FilterOptions.Select(f => f.Key)
            // Numeric range filters are always included
            .Should().Equal("sciu", "oar", "par");
    }

    [Fact]
    public async Task FilterBy_GenderOfEntry_FilterOptions_WhenMoreThanOneFilterOptionAvailable_FilterIsIncluded()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", GenderId = "1", GenderName = "Boys" },
            new() { URN = "100003", GenderId = "2", GenderName = "Girls" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" }
        );

        var response = await _sut.Execute(Request("100001"));

        response.FilterOptions.Should().SatisfyRespectively(
            // Numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Should().BeEquivalentTo(new
            {
                Key = "goe",
                Name = "Gender of entry",
                Options = new[]
                {
                    new { Key = "1", Name = "Boys", Selected = false, Count = 1 },
                    new { Key = "2", Name = "Girls", Selected = false, Count = 1 }
                }
            }),
            // Numeric range filters are always included
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Theory]
    [InlineData("", "Boys")]
    [InlineData("1", "")]
    [InlineData("", "")]
    public async Task FilterBy_GenderOfEntry_FilterOptions_WhenCurrentSchoolRegionDataMissing_CurrentSchoolValueIsNotAvailable(string regionId, string regionName)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001", GenderId = regionId, GenderName = regionName },
            new() { URN = "100002", GenderId = "1", GenderName = "Boys" },
            new() { URN = "100003", GenderId = "2", GenderName = "Girls" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" }
        );

        var response = await _sut.Execute(Request("100001"));

        response.FilterOptions.Should().SatisfyRespectively(
            // Numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Should().BeEquivalentTo(new
            {
                Key = "goe",
                Name = "Gender of entry",
                Options = new[]
                {
                    new { Key = "1", Name = "Boys", Selected = false, Count = 1 },
                    new { Key = "2", Name = "Girls", Selected = false, Count = 1 }
                },
                CurrentSchoolValue = DataWithAvailability.NotAvailable<string>()
            }),
            // Numeric range filters are always included
            f => f.Key.Should().Be("oar"),
            f => f.Key.Should().Be("par")
        );
    }

    [Theory]
    [InlineData("oar_f", new[] { "50" }, "oar_t", new[] { "51" }, new string[0])]
    [InlineData("oar_f", new[] { "50" }, "", new string[0], new string[0])]
    [InlineData("", new string[0], "oar_t", new[] { "51" }, new string[0])]
    [InlineData("oar_f", new[] { "XXX" }, "", new string[0], new[] { "oar_f:Enter a numeric value" })]
    [InlineData("oar_f", new[] { "10..0" }, "", new string[0], new[] { "oar_f:Enter a numeric value" })]
    [InlineData("oar_f", new[] { "..10..0" }, "", new string[0], new[] { "oar_f:Enter a numeric value" })]
    [InlineData("", new string[0], "oar_t", new[] { "XXX" }, new[] { "oar_t:Enter a numeric value" })]
    [InlineData("", new string[0], "oar_t", new[] { "10..0" }, new[] { "oar_t:Enter a numeric value" })]
    [InlineData("", new string[0], "oar_t", new[] { "..10..0" }, new[] { "oar_t:Enter a numeric value" })]
    [InlineData("oar_f", new[] { "XXX" }, "oar_t", new[] { "XXX" }, new[] { "oar_f:Enter a numeric value", "oar_t:Enter a numeric value" })]
    [InlineData("oar_f", new[] { "20" }, "oar_t", new[] { "10" }, new[] { "oar:The From value must be lower than the To value" })]
    [InlineData("oar_f", new[] { "-1" }, "oar_t", new[] { "99999999999999999999999999999999999999999999999999999999999" }, new[] { "oar_f:Enter a value between 0 and 999", "oar_t:Enter a value between 0 and 999" })]
    public async Task FilterBy_OverallAbsenceRate_ValidationErrors(string fromFilterKey, string[] fromFilterValues, string toFilterKey, string[] toFilterValues, string[] expectedValidationErrors)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002" },
            new() { URN = "100003" },
            new() { URN = "100004" },
            new() { URN = "100005" },
            new() { URN = "100006" },
            new() { URN = "100007" }
        );
        _absenceRepo.SetupEstablishmentAbsence(
            new() { Id = "100002", Abs_Tot_Est_Current_Pct = "49.99" },
            new() { Id = "100003", Abs_Tot_Est_Current_Pct = "50.00" },
            new() { Id = "100004", Abs_Tot_Est_Current_Pct = "50.01" },
            new() { Id = "100005", Abs_Tot_Est_Current_Pct = "51.00" },
            new() { Id = "100006", Abs_Tot_Est_Current_Pct = "51.01" },
            new() { Id = "100007", Abs_Tot_Est_Current_Pct = "52" }
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
            [fromFilterKey] = fromFilterValues,
            [toFilterKey] = toFilterValues
        }));

        response.ValidationErrors.Select(e => $"{e.Key}:{e.Message}")
            .Should().BeEquivalentTo(expectedValidationErrors);

        response.FilterOptions.OfType<SimilarSchoolsNumericRangeAvailableFilter>()
            .SelectMany(f => f.ValidationErrors.Select(e => $"{e.Key}:{e.Message}"))
            .Should().BeEquivalentTo(expectedValidationErrors);
    }

    [Theory]
    [InlineData("oar_f", new[] { "50" }, "oar_t", new[] { "51" }, new[] { "100003", "100004", "100005" })]
    [InlineData("oar_f", new[] { "50" }, "", new string[0], new[] { "100003", "100004", "100005", "100006", "100007" })]
    [InlineData("", new string[0], "oar_t", new[] { "51" }, new[] { "100002", "100003", "100004", "100005" })]
    // Filter key is case insensitive
    [InlineData("OAR_F", new[] { "50" }, "oar_T", new[] { "51" }, new[] { "100003", "100004", "100005" })]
    // Duplicate filter values are ignored (uses last given value)
    [InlineData("oar_f", new[] { "0", "50" }, "oar_t", new[] { "100", "51" }, new[] { "100003", "100004", "100005" })]
    // Empty filter values returns all results
    [InlineData("", new string[0], "", new string[0], new[] { "100002", "100003", "100004", "100005", "100006", "100007" })]
    public async Task FilterBy_OverallAbsenceRate(string fromFilterKey, string[] fromFilterValues, string toFilterKey, string[] toFilterValues, string[] expectedUrns)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002" },
            new() { URN = "100003" },
            new() { URN = "100004" },
            new() { URN = "100005" },
            new() { URN = "100006" },
            new() { URN = "100007" }
        );
        _absenceRepo.SetupEstablishmentAbsence(
            new() { Id = "100002", Abs_Tot_Est_Current_Pct = "49.99" },
            new() { Id = "100003", Abs_Tot_Est_Current_Pct = "50.00" },
            new() { Id = "100004", Abs_Tot_Est_Current_Pct = "50.01" },
            new() { Id = "100005", Abs_Tot_Est_Current_Pct = "51.00" },
            new() { Id = "100006", Abs_Tot_Est_Current_Pct = "51.01" },
            new() { Id = "100007", Abs_Tot_Est_Current_Pct = "52" }
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
            [fromFilterKey] = fromFilterValues,
            [toFilterKey] = toFilterValues
        }));

        response.AllResults.Select(r => r.SimilarSchool.URN)
            .Should().BeEquivalentTo(expectedUrns);
    }

    [Fact]
    public async Task FilterBy_OverallAbsenceRate_FiltersOutEstablishmentsWithMissingOrInvalidCurrentTotalAbsenceData()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002" },
            new() { URN = "100003" },
            new() { URN = "100004" },
            new() { URN = "100005" }
        );
        _absenceRepo.SetupEstablishmentAbsence(
            new() { Id = "100002", Abs_Tot_Est_Current_Pct = "49.99" },
            new() { Id = "100003", Abs_Tot_Est_Current_Pct = "" },
            new() { Id = "100004", Abs_Tot_Est_Current_Pct = "XXX" },
            new() { Id = "100005" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" },
            new() { URN = "100001", NeighbourURN = "100005" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["oar_f"] = ["0"],
            ["oar_t"] = [""]
        }));

        response.AllResults.Select(r => r.SimilarSchool.URN)
            .Should().BeEquivalentTo(["100002"]);
    }

    [Fact]
    public async Task FilterBy_OverallAbsenceRate_DoesNotApplyFilterIfBothFromAndToFieldsAreEmpty()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002" },
            new() { URN = "100003" },
            new() { URN = "100004" },
            new() { URN = "100005" }
        );
        _absenceRepo.SetupEstablishmentAbsence(
            new() { Id = "100002", Abs_Tot_Est_Current_Pct = "49.99" },
            new() { Id = "100003", Abs_Tot_Est_Current_Pct = "" },
            new() { Id = "100004", Abs_Tot_Est_Current_Pct = "XXX" },
            new() { Id = "100005" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" },
            new() { URN = "100001", NeighbourURN = "100005" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["sciu_f"] = [""],
            ["sciu_t"] = [""]
        }));

        response.AllResults.Select(r => r.SimilarSchool.URN)
            .Should().BeEquivalentTo("100002", "100003", "100004", "100005");
    }

    [Fact]
    public async Task FilterBy_OverallAbsenceRate_FilterOptions()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002" },
            new() { URN = "100003" }
        );
        _absenceRepo.SetupEstablishmentAbsence(
            new() { Id = "100001", Abs_Tot_Est_Current_Pct = "48.0" },
            new() { Id = "100002", Abs_Tot_Est_Current_Pct = "49.99" },
            new() { Id = "100003", Abs_Tot_Est_Current_Pct = "50" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["oar_f"] = ["50"],
            ["oar_t"] = ["51"]
        }));

        response.FilterOptions.Should().SatisfyRespectively(
            f => f.Key.Should().Be("sciu"),
            f => f.Should().BeEquivalentTo(new
            {
                Key = "oar",
                Name = "Overall absence rate",
                CurrentSchoolValue = DataWithAvailability.Available("48.0%"),
                From = new { Key = "oar_f", Value = "50" },
                To = new { Key = "oar_t", Value = "51" },
            }),
            // Other numeric range filters are always included
            f => f.Key.Should().Be("par")
        );
    }

    [Theory]
    [InlineData("oar_f", new[] { "50" }, "oar_t", new[] { "51.0" }, "50", "51.0")]
    [InlineData("oar_f", new[] { "50.0099999" }, "oar_t", new[] { "51.000000001" }, "50.01", "51.00")]
    [InlineData("oar_f", new string[0], "oar_t", new[] { "" }, "", "")]
    public async Task FilterBy_OverallAbsenceRate_FilterOptions_RoundsLongTrailingDecimals(string fromFilterKey, string[] fromFilterValues, string toFilterKey, string[] toFilterValues, string expectedFromValue, string expectedToValue)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002" },
            new() { URN = "100003" }
        );
        _absenceRepo.SetupEstablishmentAbsence(
            new() { Id = "100001", Abs_Tot_Est_Current_Pct = "48.0" },
            new() { Id = "100002", Abs_Tot_Est_Current_Pct = "49.99" },
            new() { Id = "100003", Abs_Tot_Est_Current_Pct = "50" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [fromFilterKey] = fromFilterValues,
            [toFilterKey] = toFilterValues
        }));

        response.FilterOptions.Should().SatisfyRespectively(
            f => f.Key.Should().Be("sciu"),
            f => f.Should().BeEquivalentTo(new
            {
                Key = "oar",
                Name = "Overall absence rate",
                CurrentSchoolValue = DataWithAvailability.Available("48.0%"),
                From = new { Key = "oar_f", Value = expectedFromValue },
                To = new { Key = "oar_t", Value = expectedToValue },
            }),
            // Other numeric range filters are always included
            f => f.Key.Should().Be("par")
        );
    }

    [Theory]
    [InlineData("48", "48.0%")]
    [InlineData("", null)]
    [InlineData("XXX", null)]
    public async Task FilterBy_OverallAbsenceRate_FilterOptions_CurrentSchoolValue(string currentSchoolValue, string? expectedCurrentSchoolValue)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002" },
            new() { URN = "100003" }
        );
        _absenceRepo.SetupEstablishmentAbsence(
            new() { Id = "100001", Abs_Tot_Est_Current_Pct = currentSchoolValue },
            new() { Id = "100002", Abs_Tot_Est_Current_Pct = "49.99" },
            new() { Id = "100003", Abs_Tot_Est_Current_Pct = "50" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" }
        );

        var response = await _sut.Execute(Request("100001"));

        response.FilterOptions.Should().SatisfyRespectively(
            f => f.Key.Should().Be("sciu"),
            f => f.Should().BeEquivalentTo(new
            {
                Key = "oar",
                Name = "Overall absence rate",
                CurrentSchoolValue = expectedCurrentSchoolValue is null
                    ? DataWithAvailability.NotAvailable<string>()
                    : DataWithAvailability.Available(expectedCurrentSchoolValue)
            }),
            // Other numeric range filters are always included
            f => f.Key.Should().Be("par")
        );
    }

    [Theory]
    [InlineData("par_f", new[] { "50" }, "par_t", new[] { "51" }, new string[0])]
    [InlineData("par_f", new[] { "50" }, "", new string[0], new string[0])]
    [InlineData("", new string[0], "par_t", new[] { "51" }, new string[0])]
    [InlineData("par_f", new[] { "XXX" }, "", new string[0], new[] { "par_f:Enter a numeric value" })]
    [InlineData("par_f", new[] { "10..0" }, "", new string[0], new[] { "par_f:Enter a numeric value" })]
    [InlineData("par_f", new[] { "..10..0" }, "", new string[0], new[] { "par_f:Enter a numeric value" })]
    [InlineData("", new string[0], "par_t", new[] { "XXX" }, new[] { "par_t:Enter a numeric value" })]
    [InlineData("", new string[0], "par_t", new[] { "10..0" }, new[] { "par_t:Enter a numeric value" })]
    [InlineData("", new string[0], "par_t", new[] { "..10..0" }, new[] { "par_t:Enter a numeric value" })]
    [InlineData("par_f", new[] { "XXX" }, "par_t", new[] { "XXX" }, new[] { "par_f:Enter a numeric value", "par_t:Enter a numeric value" })]
    [InlineData("par_f", new[] { "20" }, "par_t", new[] { "10" }, new[] { "par:The From value must be lower than the To value" })]
    [InlineData("par_f", new[] { "-1" }, "par_t", new[] { "99999999999999999999999999999999999999999999999999999999999" }, new[] { "par_f:Enter a value between 0 and 999", "par_t:Enter a value between 0 and 999" })]
    public async Task FilterBy_PersistentAbsenceRate_ValidationErrors(string fromFilterKey, string[] fromFilterValues, string toFilterKey, string[] toFilterValues, string[] expectedValidationErrors)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002" },
            new() { URN = "100003" },
            new() { URN = "100004" },
            new() { URN = "100005" },
            new() { URN = "100006" },
            new() { URN = "100007" }
        );
        _absenceRepo.SetupEstablishmentAbsence(
            new() { Id = "100002", Abs_Persistent_Est_Current_Pct = "49.99" },
            new() { Id = "100003", Abs_Persistent_Est_Current_Pct = "50.00" },
            new() { Id = "100004", Abs_Persistent_Est_Current_Pct = "50.01" },
            new() { Id = "100005", Abs_Persistent_Est_Current_Pct = "51.00" },
            new() { Id = "100006", Abs_Persistent_Est_Current_Pct = "51.01" },
            new() { Id = "100007", Abs_Persistent_Est_Current_Pct = "52" }
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
            [fromFilterKey] = fromFilterValues,
            [toFilterKey] = toFilterValues
        }));

        response.ValidationErrors.Select(e => $"{e.Key}:{e.Message}")
            .Should().BeEquivalentTo(expectedValidationErrors);

        response.FilterOptions.OfType<SimilarSchoolsNumericRangeAvailableFilter>()
            .SelectMany(f => f.ValidationErrors.Select(e => $"{e.Key}:{e.Message}"))
            .Should().BeEquivalentTo(expectedValidationErrors);
    }

    [Theory]
    [InlineData("par_f", new[] { "50" }, "par_t", new[] { "51" }, new[] { "100003", "100004", "100005" })]
    [InlineData("par_f", new[] { "50" }, "", new string[0], new[] { "100003", "100004", "100005", "100006", "100007" })]
    [InlineData("", new string[0], "par_t", new[] { "51" }, new[] { "100002", "100003", "100004", "100005" })]
    // Filter key is case insensitive
    [InlineData("PAR_F", new[] { "50" }, "par_T", new[] { "51" }, new[] { "100003", "100004", "100005" })]
    // Duplicate filter values are ignored (uses last given value)
    [InlineData("par_f", new[] { "0", "50" }, "par_t", new[] { "100", "51" }, new[] { "100003", "100004", "100005" })]
    // Empty filter values returns all results
    [InlineData("", new string[0], "", new string[0], new[] { "100002", "100003", "100004", "100005", "100006", "100007" })]
    public async Task FilterBy_PersistentAbsenceRate(string fromFilterKey, string[] fromFilterValues, string toFilterKey, string[] toFilterValues, string[] expectedUrns)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002" },
            new() { URN = "100003" },
            new() { URN = "100004" },
            new() { URN = "100005" },
            new() { URN = "100006" },
            new() { URN = "100007" }
        );
        _absenceRepo.SetupEstablishmentAbsence(
            new() { Id = "100002", Abs_Persistent_Est_Current_Pct = "49.99" },
            new() { Id = "100003", Abs_Persistent_Est_Current_Pct = "50.00" },
            new() { Id = "100004", Abs_Persistent_Est_Current_Pct = "50.01" },
            new() { Id = "100005", Abs_Persistent_Est_Current_Pct = "51.00" },
            new() { Id = "100006", Abs_Persistent_Est_Current_Pct = "51.01" },
            new() { Id = "100007", Abs_Persistent_Est_Current_Pct = "52" }
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
            [fromFilterKey] = fromFilterValues,
            [toFilterKey] = toFilterValues
        }));

        response.AllResults.Select(r => r.SimilarSchool.URN)
            .Should().BeEquivalentTo(expectedUrns);
    }

    [Fact]
    public async Task FilterBy_PersistentAbsenceRate_FiltersOutEstablishmentsWithMissingOrInvalidCurrentTotalAbsenceData()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002" },
            new() { URN = "100003" },
            new() { URN = "100004" },
            new() { URN = "100005" }
        );
        _absenceRepo.SetupEstablishmentAbsence(
            new() { Id = "100002", Abs_Persistent_Est_Current_Pct = "49.99" },
            new() { Id = "100003", Abs_Persistent_Est_Current_Pct = "" },
            new() { Id = "100004", Abs_Persistent_Est_Current_Pct = "XXX" },
            new() { Id = "100005" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" },
            new() { URN = "100001", NeighbourURN = "100005" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["par_f"] = ["0"],
            ["par_t"] = [""]
        }));

        response.AllResults.Select(r => r.SimilarSchool.URN)
            .Should().BeEquivalentTo(["100002"]);
    }

    [Fact]
    public async Task FilterBy_PersistentAbsenceRate_DoesNotApplyFilterIfBothFromAndToFieldsAreEmpty()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002" },
            new() { URN = "100003" },
            new() { URN = "100004" },
            new() { URN = "100005" }
        );
        _absenceRepo.SetupEstablishmentAbsence(
            new() { Id = "100002", Abs_Persistent_Est_Current_Pct = "49.99" },
            new() { Id = "100003", Abs_Persistent_Est_Current_Pct = "" },
            new() { Id = "100004", Abs_Persistent_Est_Current_Pct = "XXX" },
            new() { Id = "100005" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" },
            new() { URN = "100001", NeighbourURN = "100005" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["sciu_f"] = [""],
            ["sciu_t"] = [""]
        }));

        response.AllResults.Select(r => r.SimilarSchool.URN)
            .Should().BeEquivalentTo("100002", "100003", "100004", "100005");
    }

    [Fact]
    public async Task FilterBy_PersistentAbsenceRate_FilterOptions()
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002" },
            new() { URN = "100003" }
        );
        _absenceRepo.SetupEstablishmentAbsence(
            new() { Id = "100001", Abs_Persistent_Est_Current_Pct = "48.0" },
            new() { Id = "100002", Abs_Persistent_Est_Current_Pct = "49.99" },
            new() { Id = "100003", Abs_Persistent_Est_Current_Pct = "50" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            ["par_f"] = ["50"],
            ["par_t"] = ["51"]
        }));

        response.FilterOptions.Should().SatisfyRespectively(
            // Other numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Key.Should().Be("oar"),
            f => f.Should().BeEquivalentTo(new
            {
                Key = "par",
                Name = "Persistent absence rate",
                CurrentSchoolValue = DataWithAvailability.Available("48.0%"),
                From = new { Key = "par_f", Value = "50" },
                To = new { Key = "par_t", Value = "51" },
            })
        );
    }

    [Theory]
    [InlineData("par_f", new[] { "50" }, "par_t", new[] { "51.0" }, "50", "51.0")]
    [InlineData("par_f", new[] { "50.0099999" }, "par_t", new[] { "51.000000001" }, "50.01", "51.00")]
    [InlineData("par_f", new string[0], "par_t", new[] { "" }, "", "")]
    public async Task FilterBy_PersistentAbsenceRate_FilterOptions_RoundsLongTrailingDecimals(string fromFilterKey, string[] fromFilterValues, string toFilterKey, string[] toFilterValues, string expectedFromValue, string expectedToValue)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002" },
            new() { URN = "100003" }
        );
        _absenceRepo.SetupEstablishmentAbsence(
            new() { Id = "100001", Abs_Persistent_Est_Current_Pct = "48.0" },
            new() { Id = "100002", Abs_Persistent_Est_Current_Pct = "49.99" },
            new() { Id = "100003", Abs_Persistent_Est_Current_Pct = "50" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" }
        );

        var response = await _sut.Execute(Request("100001", filterBy: new()
        {
            [fromFilterKey] = fromFilterValues,
            [toFilterKey] = toFilterValues
        }));

        response.FilterOptions.Should().SatisfyRespectively(
            // Other numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Key.Should().Be("oar"),
            f => f.Should().BeEquivalentTo(new
            {
                Key = "par",
                Name = "Persistent absence rate",
                CurrentSchoolValue = DataWithAvailability.Available("48.0%"),
                From = new { Key = "par_f", Value = expectedFromValue },
                To = new { Key = "par_t", Value = expectedToValue },
            })
        );
    }

    [Theory]
    [InlineData("48", "48.0%")]
    [InlineData("", null)]
    [InlineData("XXX", null)]
    public async Task FilterBy_PersistentAbsenceRate_FilterOptions_CurrentSchoolValue(string currentSchoolValue, string? expectedCurrentSchoolValue)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002" },
            new() { URN = "100003" }
        );
        _absenceRepo.SetupEstablishmentAbsence(
            new() { Id = "100001", Abs_Persistent_Est_Current_Pct = currentSchoolValue },
            new() { Id = "100002", Abs_Persistent_Est_Current_Pct = "49.99" },
            new() { Id = "100003", Abs_Persistent_Est_Current_Pct = "50" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" }
        );

        var response = await _sut.Execute(Request("100001"));

        response.FilterOptions.Should().SatisfyRespectively(
            // Other numeric range filters are always included
            f => f.Key.Should().Be("sciu"),
            f => f.Key.Should().Be("oar"),
            f => f.Should().BeEquivalentTo(new
            {
                Key = "par",
                Name = "Persistent absence rate",
                CurrentSchoolValue = expectedCurrentSchoolValue is null
                    ? DataWithAvailability.NotAvailable<string>()
                    : DataWithAvailability.Available(expectedCurrentSchoolValue)
            })
        );
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

        response.AllResults.Select(r => (r.SimilarSchool.URN, r.SortValue.Value))
            .Should().Equal(
                ("100004", DataWithAvailability.Available("30.0")),
                ("100005", DataWithAvailability.Available("20.0")),
                ("100002", DataWithAvailability.Available("10.0")),
                ("100003", DataWithAvailability.NotAvailable<string>()));

        response.SortOptions.Should().SatisfyRespectively(
            o => o.Should().BeEquivalentTo(new { Key = "Att8", Selected = true }),
            o => o.Should().BeEquivalentTo(new { Key = "EngMat", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "EngLang", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "EngLit", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Maths", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "CombSci", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Bio", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Chem", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Phys", Selected = false })
        );
    }

    [Fact]
    public async Task SortBy_IgnoresInvalidSortKey()
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

        var response = await _sut.Execute(Request("100001", sortBy: "XXX"));

        response.AllResults.Select(r => (r.SimilarSchool.URN, r.SortValue.Value))
            .Should().Equal(
                ("100004", DataWithAvailability.Available("30.0")),
                ("100005", DataWithAvailability.Available("20.0")),
                ("100002", DataWithAvailability.Available("10.0")),
                ("100003", DataWithAvailability.NotAvailable<string>()));

        response.SortOptions.Should().SatisfyRespectively(
            o => o.Should().BeEquivalentTo(new { Key = "Att8", Selected = true }),
            o => o.Should().BeEquivalentTo(new { Key = "EngMat", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "EngLang", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "EngLit", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Maths", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "CombSci", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Bio", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Chem", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Phys", Selected = false })
        );
    }

    [Theory]
    [InlineData("Att8")]
    [InlineData("att8")]
    public async Task SortBy_Attainment8(string sortBy)
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

        var response = await _sut.Execute(Request("100001", sortBy: sortBy));

        response.AllResults.Select(r => (r.SimilarSchool.URN, r.SortValue.Value))
            .Should().Equal(
                ("100004", DataWithAvailability.Available("30.0")),
                ("100005", DataWithAvailability.Available("20.0")),
                ("100002", DataWithAvailability.Available("10.0")),
                ("100003", DataWithAvailability.NotAvailable<string>()));

        response.SortOptions.Should().SatisfyRespectively(
            o => o.Should().BeEquivalentTo(new { Key = "Att8", Selected = true }),
            o => o.Should().BeEquivalentTo(new { Key = "EngMat", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "EngLang", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "EngLit", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Maths", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "CombSci", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Bio", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Chem", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Phys", Selected = false })
        );
    }

    [Theory]
    [InlineData("EngMat")]
    [InlineData("engmat")]
    public async Task SortBy_EnglishMaths(string sortBy)
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

        var response = await _sut.Execute(Request("100001", sortBy: sortBy));

        response.AllResults.Select(r => (r.SimilarSchool.URN, r.SortValue.Value))
            .Should().Equal(
                ("100004", DataWithAvailability.Available("30.0%")),
                ("100005", DataWithAvailability.Available("20.0%")),
                ("100002", DataWithAvailability.Available("10.0%")),
                ("100003", DataWithAvailability.NotAvailable<string>()));

        response.SortOptions.Should().SatisfyRespectively(
            o => o.Should().BeEquivalentTo(new { Key = "Att8", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "EngMat", Selected = true }),
            o => o.Should().BeEquivalentTo(new { Key = "EngLang", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "EngLit", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Maths", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "CombSci", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Bio", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Chem", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Phys", Selected = false })
        );
    }

    [Theory]
    [InlineData("EngLang")]
    [InlineData("englang")]
    public async Task SortBy_EnglishLanguage(string sortBy)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002" },
            new() { URN = "100003" },
            new() { URN = "100004" },
            new() { URN = "100005" }
        );
        _performanceRepo.SetupEstablishmentPerformance(
            new() { Id = "100002", EngLang59_Sum_Est_Current_Pct = "10" },
            new() { Id = "100003", EngLang59_Sum_Est_Current_Pct = "z" },
            new() { Id = "100004", EngLang59_Sum_Est_Current_Pct = "30" },
            new() { Id = "100005", EngLang59_Sum_Est_Current_Pct = "20" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" },
            new() { URN = "100001", NeighbourURN = "100005" }
        );

        var response = await _sut.Execute(Request("100001", sortBy: sortBy));

        response.AllResults.Select(r => (r.SimilarSchool.URN, r.SortValue.Value))
            .Should().Equal(
                ("100004", DataWithAvailability.Available("30.0%")),
                ("100005", DataWithAvailability.Available("20.0%")),
                ("100002", DataWithAvailability.Available("10.0%")),
                ("100003", DataWithAvailability.NotAvailable<string>()));

        response.SortOptions.Should().SatisfyRespectively(
            o => o.Should().BeEquivalentTo(new { Key = "Att8", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "EngMat", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "EngLang", Selected = true }),
            o => o.Should().BeEquivalentTo(new { Key = "EngLit", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Maths", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "CombSci", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Bio", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Chem", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Phys", Selected = false })
        );
    }

    [Theory]
    [InlineData("EngLit")]
    [InlineData("englit")]
    public async Task SortBy_EnglishLiterature(string sortBy)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002" },
            new() { URN = "100003" },
            new() { URN = "100004" },
            new() { URN = "100005" }
        );
        _performanceRepo.SetupEstablishmentPerformance(
            new() { Id = "100002", EngLit59_Sum_Est_Current_Pct = "10" },
            new() { Id = "100003", EngLit59_Sum_Est_Current_Pct = "z" },
            new() { Id = "100004", EngLit59_Sum_Est_Current_Pct = "30" },
            new() { Id = "100005", EngLit59_Sum_Est_Current_Pct = "20" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" },
            new() { URN = "100001", NeighbourURN = "100005" }
        );

        var response = await _sut.Execute(Request("100001", sortBy: sortBy));

        response.AllResults.Select(r => (r.SimilarSchool.URN, r.SortValue.Value))
            .Should().Equal(
                ("100004", DataWithAvailability.Available("30.0%")),
                ("100005", DataWithAvailability.Available("20.0%")),
                ("100002", DataWithAvailability.Available("10.0%")),
                ("100003", DataWithAvailability.NotAvailable<string>()));

        response.SortOptions.Should().SatisfyRespectively(
            o => o.Should().BeEquivalentTo(new { Key = "Att8", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "EngMat", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "EngLang", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "EngLit", Selected = true }),
            o => o.Should().BeEquivalentTo(new { Key = "Maths", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "CombSci", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Bio", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Chem", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Phys", Selected = false })
        );
    }

    [Theory]
    [InlineData("Maths")]
    [InlineData("maths")]
    public async Task SortBy_Maths(string sortBy)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002" },
            new() { URN = "100003" },
            new() { URN = "100004" },
            new() { URN = "100005" }
        );
        _performanceRepo.SetupEstablishmentPerformance(
            new() { Id = "100002", Maths59_Sum_Est_Current_Pct = "10" },
            new() { Id = "100003", Maths59_Sum_Est_Current_Pct = "z" },
            new() { Id = "100004", Maths59_Sum_Est_Current_Pct = "30" },
            new() { Id = "100005", Maths59_Sum_Est_Current_Pct = "20" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" },
            new() { URN = "100001", NeighbourURN = "100005" }
        );

        var response = await _sut.Execute(Request("100001", sortBy: sortBy));

        response.AllResults.Select(r => (r.SimilarSchool.URN, r.SortValue.Value))
            .Should().Equal(
                ("100004", DataWithAvailability.Available("30.0%")),
                ("100005", DataWithAvailability.Available("20.0%")),
                ("100002", DataWithAvailability.Available("10.0%")),
                ("100003", DataWithAvailability.NotAvailable<string>()));

        response.SortOptions.Should().SatisfyRespectively(
            o => o.Should().BeEquivalentTo(new { Key = "Att8", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "EngMat", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "EngLang", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "EngLit", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Maths", Selected = true }),
            o => o.Should().BeEquivalentTo(new { Key = "CombSci", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Bio", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Chem", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Phys", Selected = false })
        );
    }

    [Theory]
    [InlineData("CombSci")]
    [InlineData("combsci")]
    public async Task SortBy_CombinedScience(string sortBy)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002" },
            new() { URN = "100003" },
            new() { URN = "100004" },
            new() { URN = "100005" }
        );
        _performanceRepo.SetupEstablishmentPerformance(
            new() { Id = "100002", CombSci59_Sum_Est_Current_Pct = "10" },
            new() { Id = "100003", CombSci59_Sum_Est_Current_Pct = "z" },
            new() { Id = "100004", CombSci59_Sum_Est_Current_Pct = "30" },
            new() { Id = "100005", CombSci59_Sum_Est_Current_Pct = "20" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" },
            new() { URN = "100001", NeighbourURN = "100005" }
        );

        var response = await _sut.Execute(Request("100001", sortBy: sortBy));

        response.AllResults.Select(r => (r.SimilarSchool.URN, r.SortValue.Value))
            .Should().Equal(
                ("100004", DataWithAvailability.Available("30.0%")),
                ("100005", DataWithAvailability.Available("20.0%")),
                ("100002", DataWithAvailability.Available("10.0%")),
                ("100003", DataWithAvailability.NotAvailable<string>()));

        response.SortOptions.Should().SatisfyRespectively(
            o => o.Should().BeEquivalentTo(new { Key = "Att8", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "EngMat", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "EngLang", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "EngLit", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Maths", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "CombSci", Selected = true }),
            o => o.Should().BeEquivalentTo(new { Key = "Bio", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Chem", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Phys", Selected = false })
        );
    }

    [Theory]
    [InlineData("Bio")]
    [InlineData("bio")]
    public async Task SortBy_Biology(string sortBy)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002" },
            new() { URN = "100003" },
            new() { URN = "100004" },
            new() { URN = "100005" }
        );
        _performanceRepo.SetupEstablishmentPerformance(
            new() { Id = "100002", Bio59_Sum_Est_Current_Pct = "10" },
            new() { Id = "100003", Bio59_Sum_Est_Current_Pct = "z" },
            new() { Id = "100004", Bio59_Sum_Est_Current_Pct = "30" },
            new() { Id = "100005", Bio59_Sum_Est_Current_Pct = "20" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" },
            new() { URN = "100001", NeighbourURN = "100005" }
        );

        var response = await _sut.Execute(Request("100001", sortBy: sortBy));

        response.AllResults.Select(r => (r.SimilarSchool.URN, r.SortValue.Value))
            .Should().Equal(
                ("100004", DataWithAvailability.Available("30.0%")),
                ("100005", DataWithAvailability.Available("20.0%")),
                ("100002", DataWithAvailability.Available("10.0%")),
                ("100003", DataWithAvailability.NotAvailable<string>()));

        response.SortOptions.Should().SatisfyRespectively(
            o => o.Should().BeEquivalentTo(new { Key = "Att8", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "EngMat", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "EngLang", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "EngLit", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Maths", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "CombSci", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Bio", Selected = true }),
            o => o.Should().BeEquivalentTo(new { Key = "Chem", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Phys", Selected = false })
        );
    }

    [Theory]
    [InlineData("Chem")]
    [InlineData("chem")]
    public async Task SortBy_Chemistry(string sortBy)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002" },
            new() { URN = "100003" },
            new() { URN = "100004" },
            new() { URN = "100005" }
        );
        _performanceRepo.SetupEstablishmentPerformance(
            new() { Id = "100002", Chem59_Sum_Est_Current_Pct = "10" },
            new() { Id = "100003", Chem59_Sum_Est_Current_Pct = "z" },
            new() { Id = "100004", Chem59_Sum_Est_Current_Pct = "30" },
            new() { Id = "100005", Chem59_Sum_Est_Current_Pct = "20" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" },
            new() { URN = "100001", NeighbourURN = "100005" }
        );

        var response = await _sut.Execute(Request("100001", sortBy: sortBy));

        response.AllResults.Select(r => (r.SimilarSchool.URN, r.SortValue.Value))
            .Should().Equal(
                ("100004", DataWithAvailability.Available("30.0%")),
                ("100005", DataWithAvailability.Available("20.0%")),
                ("100002", DataWithAvailability.Available("10.0%")),
                ("100003", DataWithAvailability.NotAvailable<string>()));

        response.SortOptions.Should().SatisfyRespectively(
            o => o.Should().BeEquivalentTo(new { Key = "Att8", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "EngMat", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "EngLang", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "EngLit", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Maths", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "CombSci", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Bio", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Chem", Selected = true }),
            o => o.Should().BeEquivalentTo(new { Key = "Phys", Selected = false })
        );
    }

    [Theory]
    [InlineData("Phys")]
    [InlineData("phys")]
    public async Task SortBy_Physics(string sortBy)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002" },
            new() { URN = "100003" },
            new() { URN = "100004" },
            new() { URN = "100005" }
        );
        _performanceRepo.SetupEstablishmentPerformance(
            new() { Id = "100002", Physics59_Sum_Est_Current_Pct = "10" },
            new() { Id = "100003", Physics59_Sum_Est_Current_Pct = "z" },
            new() { Id = "100004", Physics59_Sum_Est_Current_Pct = "30" },
            new() { Id = "100005", Physics59_Sum_Est_Current_Pct = "20" }
        );
        _similarSchoolsRepo.SetupGroups(
            new() { URN = "100001", NeighbourURN = "100002" },
            new() { URN = "100001", NeighbourURN = "100003" },
            new() { URN = "100001", NeighbourURN = "100004" },
            new() { URN = "100001", NeighbourURN = "100005" }
        );

        var response = await _sut.Execute(Request("100001", sortBy: sortBy));

        response.AllResults.Select(r => (r.SimilarSchool.URN, r.SortValue.Value))
            .Should().Equal(
                ("100004", DataWithAvailability.Available("30.0%")),
                ("100005", DataWithAvailability.Available("20.0%")),
                ("100002", DataWithAvailability.Available("10.0%")),
                ("100003", DataWithAvailability.NotAvailable<string>()));

        response.SortOptions.Should().SatisfyRespectively(
            o => o.Should().BeEquivalentTo(new { Key = "Att8", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "EngMat", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "EngLang", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "EngLit", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Maths", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "CombSci", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Bio", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Chem", Selected = false }),
            o => o.Should().BeEquivalentTo(new { Key = "Phys", Selected = true })
        );
    }

    [Fact]
    public async Task Pagination_DefaultsTo10ResultsPerPage()
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

        response.AllResults.Select(r => r.SimilarSchool.URN)
            .Should().BeEquivalentTo(
                "100002",
                "100003",
                "100004",
                "100005",
                "100006",
                "100007",
                "100008",
                "100009",
                "100010",
                "100011",
                "100012");

        response.ResultsPage.Select(r => r.SimilarSchool.URN)
            .Should().BeEquivalentTo(
                "100002",
                "100003",
                "100004",
                "100005",
                "100006",
                "100007",
                "100008",
                "100009",
                "100010",
                "100011");
    }

    [Theory]
    [InlineData("0", 1, new[] { "100002", "100003", "100004", "100005", "100006" })]
    [InlineData("1", 1, new[] { "100002", "100003", "100004", "100005", "100006" })]
    [InlineData("2", 2, new[] { "100007", "100008", "100009", "100010", "100011" })]
    [InlineData("3", 3, new[] { "100012" })]
    [InlineData("4", 3, new[] { "100012" })]
    [InlineData("X", 1, new[] { "100002", "100003", "100004", "100005", "100006" })]
    public async Task Pagination_SelectsAppropriatePageOfResults(string page, int expectedCurrentPage, string[] expectedUrnsOnPage)
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

        var response = await _sut.Execute(Request("100001", page: page, resultsPerPage: 5));

        response.ResultsPage.TotalCount.Should().Be(11);
        response.ResultsPage.TotalPages.Should().Be(3);
        response.ResultsPage.ItemsPerPage.Should().Be(5);
        response.ResultsPage.CurrentPage.Should().Be(expectedCurrentPage);
        response.ResultsPage.Count.Should().Be(expectedUrnsOnPage.Length);

        response.ResultsPage.Select(r => r.SimilarSchool.URN)
            .Should().Equal(expectedUrnsOnPage);
    }

    [Theory]
    [InlineData(new[] { "UN1" }, "1", 1, 2, 6, new[] { "100002", "100004", "100006", "100008", "100010" })]
    [InlineData(new[] { "UN1" }, "2", 2, 2, 6, new[] { "100012" })]
    [InlineData(new[] { "UF1" }, "1", 1, 1, 5, new[] { "100003", "100005", "100007", "100009", "100011" })]
    [InlineData(new[] { "UN1", "UF1" }, "1", 1, 3, 11, new[] { "100002", "100003", "100004", "100005", "100006" })]
    [InlineData(new[] { "UN1", "UF1" }, "2", 2, 3, 11, new[] { "100007", "100008", "100009", "100010", "100011" })]
    [InlineData(new[] { "UN1", "UF1" }, "3", 3, 3, 11, new[] { "100012" })]
    public async Task Pagination_WorksWithFiltering(string[] filterValues, string page, int expectedCurrentPage, int expectedTotalPages, int expectedTotalCount, string[] expectedUrnsOnPage)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", UrbanRuralId = "UN1" },
            new() { URN = "100003", UrbanRuralId = "UF1" },
            new() { URN = "100004", UrbanRuralId = "UN1" },
            new() { URN = "100005", UrbanRuralId = "UF1" },
            new() { URN = "100006", UrbanRuralId = "UN1" },
            new() { URN = "100007", UrbanRuralId = "UF1" },
            new() { URN = "100008", UrbanRuralId = "UN1" },
            new() { URN = "100009", UrbanRuralId = "UF1" },
            new() { URN = "100010", UrbanRuralId = "UN1" },
            new() { URN = "100011", UrbanRuralId = "UF1" },
            new() { URN = "100012", UrbanRuralId = "UN1" }
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

        var response = await _sut.Execute(Request("100001", page: page, resultsPerPage: 5, filterBy: new()
        {
            ["ur"] = filterValues
        }));

        response.ResultsPage.TotalCount.Should().Be(expectedTotalCount);
        response.ResultsPage.TotalPages.Should().Be(expectedTotalPages);
        response.ResultsPage.ItemsPerPage.Should().Be(5);
        response.ResultsPage.CurrentPage.Should().Be(expectedCurrentPage);
        response.ResultsPage.Count.Should().Be(expectedUrnsOnPage.Length);

        response.ResultsPage.Select(r => r.SimilarSchool.URN)
            .Should().Equal(expectedUrnsOnPage);
    }

    [Theory]
    [InlineData("EngMat", "1", 1, new[] { "100004", "100005", "100002", "100003", "100006" })]
    [InlineData("EngMat", "2", 2, new[] { "100007", "100008", "100009" })]
    [InlineData("Att8", "1", 1, new[] { "100008", "100009", "100006", "100002", "100003" })]
    [InlineData("Att8", "2", 2, new[] { "100004", "100005", "100007" })]
    public async Task Pagination_WorksWithSorting(string sortBy, string page, int expectedCurrentPage, string[] expectedUrnsOnPage)
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
            new() { URN = "100009" }
       );
        _performanceRepo.SetupEstablishmentPerformance(
           new() { Id = "100002", EngMaths59_Tot_Est_Current_Pct = "10", Attainment8_Tot_Est_Current_Num = "z" },
           new() { Id = "100003", EngMaths59_Tot_Est_Current_Pct = "z", Attainment8_Tot_Est_Current_Num = "z" },
           new() { Id = "100004", EngMaths59_Tot_Est_Current_Pct = "30", Attainment8_Tot_Est_Current_Num = "z" },
           new() { Id = "100005", EngMaths59_Tot_Est_Current_Pct = "20", Attainment8_Tot_Est_Current_Num = "z" },
           new() { Id = "100006", EngMaths59_Tot_Est_Current_Pct = "z", Attainment8_Tot_Est_Current_Num = "10" },
           new() { Id = "100007", EngMaths59_Tot_Est_Current_Pct = "z", Attainment8_Tot_Est_Current_Num = "z" },
           new() { Id = "100008", EngMaths59_Tot_Est_Current_Pct = "z", Attainment8_Tot_Est_Current_Num = "30" },
           new() { Id = "100009", EngMaths59_Tot_Est_Current_Pct = "z", Attainment8_Tot_Est_Current_Num = "20" }
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

        var response = await _sut.Execute(Request("100001", page: page, resultsPerPage: 5, sortBy: sortBy));

        response.ResultsPage.TotalCount.Should().Be(8);
        response.ResultsPage.TotalPages.Should().Be(2);
        response.ResultsPage.ItemsPerPage.Should().Be(5);
        response.ResultsPage.CurrentPage.Should().Be(expectedCurrentPage);
        response.ResultsPage.Count.Should().Be(expectedUrnsOnPage.Length);

        response.ResultsPage.Select(r => r.SimilarSchool.URN)
            .Should().Equal(expectedUrnsOnPage);
    }

    [Theory]
    [InlineData(new[] { "UN1" }, "EngMat", "1", 1, 1, 4, new[] { "100004", "100002", "100006", "100008" })]
    [InlineData(new[] { "UF1" }, "EngMat", "1", 1, 1, 4, new[] { "100005", "100003", "100007", "100009" })]
    [InlineData(new[] { "UN1" }, "Att8", "1", 1, 1, 4, new[] { "100008", "100006", "100002", "100004" })]
    [InlineData(new[] { "UF1" }, "Att8", "1", 1, 1, 4, new[] { "100009", "100003", "100005", "100007" })]
    [InlineData(new[] { "UN1", "UF1" }, "EngMat", "1", 1, 2, 8, new[] { "100004", "100005", "100002", "100003", "100006" })]
    [InlineData(new[] { "UN1", "UF1" }, "EngMat", "2", 2, 2, 8, new[] { "100007", "100008", "100009" })]
    [InlineData(new[] { "UN1", "UF1" }, "Att8", "1", 1, 2, 8, new[] { "100008", "100009", "100006", "100002", "100003" })]
    [InlineData(new[] { "UN1", "UF1" }, "Att8", "2", 2, 2, 8, new[] { "100004", "100005", "100007" })]
    public async Task Pagination_WorksWithFilteringAndSorting(string[] filterValues, string sortBy, string page, int expectedCurrentPage, int expectedTotalPages, int expectedTotalCount, string[] expectedUrnsOnPage)
    {
        _establishmentRepo.SetupEstablishments(
            new() { URN = "100001" },
            new() { URN = "100002", UrbanRuralId = "UN1" },
            new() { URN = "100003", UrbanRuralId = "UF1" },
            new() { URN = "100004", UrbanRuralId = "UN1" },
            new() { URN = "100005", UrbanRuralId = "UF1" },
            new() { URN = "100006", UrbanRuralId = "UN1" },
            new() { URN = "100007", UrbanRuralId = "UF1" },
            new() { URN = "100008", UrbanRuralId = "UN1" },
            new() { URN = "100009", UrbanRuralId = "UF1" }
        );
        _performanceRepo.SetupEstablishmentPerformance(
           new() { Id = "100002", EngMaths59_Tot_Est_Current_Pct = "10", Attainment8_Tot_Est_Current_Num = "z" },
           new() { Id = "100003", EngMaths59_Tot_Est_Current_Pct = "z", Attainment8_Tot_Est_Current_Num = "z" },
           new() { Id = "100004", EngMaths59_Tot_Est_Current_Pct = "30", Attainment8_Tot_Est_Current_Num = "z" },
           new() { Id = "100005", EngMaths59_Tot_Est_Current_Pct = "20", Attainment8_Tot_Est_Current_Num = "z" },
           new() { Id = "100006", EngMaths59_Tot_Est_Current_Pct = "z", Attainment8_Tot_Est_Current_Num = "10" },
           new() { Id = "100007", EngMaths59_Tot_Est_Current_Pct = "z", Attainment8_Tot_Est_Current_Num = "z" },
           new() { Id = "100008", EngMaths59_Tot_Est_Current_Pct = "z", Attainment8_Tot_Est_Current_Num = "30" },
           new() { Id = "100009", EngMaths59_Tot_Est_Current_Pct = "z", Attainment8_Tot_Est_Current_Num = "20" }
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

        var response = await _sut.Execute(Request("100001", page: page, resultsPerPage: 5, sortBy: sortBy, filterBy: new()
        {
            ["ur"] = filterValues
        }));

        response.ResultsPage.TotalCount.Should().Be(expectedTotalCount);
        response.ResultsPage.TotalPages.Should().Be(expectedTotalPages);
        response.ResultsPage.ItemsPerPage.Should().Be(5);
        response.ResultsPage.CurrentPage.Should().Be(expectedCurrentPage);
        response.ResultsPage.Count.Should().Be(expectedUrnsOnPage.Length);

        response.ResultsPage.Select(r => r.SimilarSchool.URN)
            .Should().Equal(expectedUrnsOnPage);
    }

    private FindSimilarSchoolsRequest Request(string urn, Dictionary<string, IEnumerable<string>>? filterBy = null, string? sortBy = null, string? page = null, int? resultsPerPage = null) =>
        resultsPerPage is int rpp
            ? new(urn, filterBy ?? [], sortBy ?? "", page ?? "", rpp)
            : new(urn, filterBy ?? [], sortBy ?? "", page ?? "");
}