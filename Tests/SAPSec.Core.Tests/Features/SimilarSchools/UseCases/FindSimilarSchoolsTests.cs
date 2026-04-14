using SAPSec.Core.Features.Filtering;
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

        response.ResultsPage.Select(r => r.SimilarSchool.URN)
            .Should().BeEquivalentTo("100003", "100004");
    }

    [Theory]
    [InlineData("dist", new[] { "5" }, new[] { "100002" })]
    [InlineData("dist", new[] { "10" }, new[] { "100002", "100003", "100004" })]
    [InlineData("dist", new[] { "25" }, new[] { "100002", "100003", "100004", "100005", "100006" })]
    [InlineData("dist", new[] { "50" }, new[] { "100002", "100003", "100004", "100005", "100006", "100007", "100008" })]
    [InlineData("dist", new[] { "100" }, new[] { "100002", "100003", "100004", "100005", "100006", "100007", "100008", "100009", "100010" })]
    [InlineData("dist", new[] { "over100" }, new[] { "100011" })]
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

        response.ResultsPage.Select(r => r.SimilarSchool.URN)
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

        response.ResultsPage.Select(r => r.SimilarSchool.URN)
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

        response.ResultsPage.Select(r => r.SimilarSchool.URN)
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
                Type = FilterType.SingleValue,
                Options = new[]
                {
                    new { Key = "5", Name = "Up to 5 miles", Selected = false, Count = 1 },
                    new { Key = "10", Name = "Up to 10 miles", Selected = true, Count = 3 },
                    new { Key = "25", Name = "Up to 25 miles", Selected = false, Count = 5 },
                    new { Key = "50", Name = "Up to 50 miles", Selected = false, Count = 7 },
                    new { Key = "100", Name = "Up to 100 miles", Selected = false, Count = 9 },
                    new { Key = "Over100", Name = "More than 100 miles", Selected = false, Count = 1 },
                }
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
                Type = FilterType.SingleValue,
                Options = new[]
                {
                    new { Key = "5", Name = "Up to 5 miles", Selected = false, Count = 1 },
                    new { Key = "10", Name = "Up to 10 miles", Selected = true, Count = 1 },
                    new { Key = "25", Name = "Up to 25 miles", Selected = false, Count = 2 },
                    new { Key = "50", Name = "Up to 50 miles", Selected = false, Count = 2 },
                    new { Key = "100", Name = "Up to 100 miles", Selected = false, Count = 2 },
                    new { Key = "Over100", Name = "More than 100 miles", Selected = false, Count = 1 },
                }
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
            new() { URN = "100002", Easting = 100000, Northing = 180000 },
            new() { URN = "100003", Easting = 180000, Northing = 100000 }
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
                Type = FilterType.SingleValue,
                Options = new[]
                {
                    new { Key = "50", Name = "Up to 50 miles", Selected = false, Count = 2 },
                    new { Key = "100", Name = "Up to 100 miles", Selected = false, Count = 2 }
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
                Type = FilterType.SingleValue,
                Options = new[]
                {
                    new { Key = "50", Name = "Up to 50 miles", Selected = false, Count = 1 },
                    new { Key = "100", Name = "Up to 100 miles", Selected = true, Count = 1 }
                }
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

        response.ResultsPage.Select(r => r.SimilarSchool.URN)
            .Should().BeEquivalentTo(expectedUrns);
    }

    [Fact]
    public async Task FilterBy_UrbanRural_FilterOptions()
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
                Type = FilterType.MultipleValue,
                Options = new[]
                {
                    new { Key = "UN1", Name = "Urban: Nearer", Selected = true, Count = 2 },
                    new { Key = "UF1", Name = "Urban: Further", Selected = true, Count = 1 },
                    new { Key = "RLN1", Name = "Larger rural: Nearer", Selected = false, Count = 3 },
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
                Type = FilterType.MultipleValue,
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
                Type = FilterType.MultipleValue,
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

    [Fact(Skip = "TODO")]
    public async Task FilterBy_AllOtherFilters()
    {
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
            o => o.Should().BeEquivalentTo(new { Key = "Att8", Selected = true }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "EngMat", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "EngLang", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "EngLit", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Maths", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "CombSci", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Bio", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Chem", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Phys", Selected = false }, o => o.ExcludingMissingMembers())
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
            o => o.Should().BeEquivalentTo(new { Key = "Att8", Selected = true }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "EngMat", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "EngLang", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "EngLit", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Maths", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "CombSci", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Bio", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Chem", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Phys", Selected = false }, o => o.ExcludingMissingMembers())
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
            o => o.Should().BeEquivalentTo(new { Key = "Att8", Selected = true }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "EngMat", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "EngLang", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "EngLit", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Maths", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "CombSci", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Bio", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Chem", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Phys", Selected = false }, o => o.ExcludingMissingMembers())
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
            o => o.Should().BeEquivalentTo(new { Key = "Att8", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "EngMat", Selected = true }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "EngLang", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "EngLit", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Maths", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "CombSci", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Bio", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Chem", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Phys", Selected = false }, o => o.ExcludingMissingMembers())
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
            o => o.Should().BeEquivalentTo(new { Key = "Att8", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "EngMat", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "EngLang", Selected = true }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "EngLit", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Maths", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "CombSci", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Bio", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Chem", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Phys", Selected = false }, o => o.ExcludingMissingMembers())
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
            o => o.Should().BeEquivalentTo(new { Key = "Att8", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "EngMat", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "EngLang", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "EngLit", Selected = true }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Maths", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "CombSci", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Bio", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Chem", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Phys", Selected = false }, o => o.ExcludingMissingMembers())
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
            o => o.Should().BeEquivalentTo(new { Key = "Att8", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "EngMat", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "EngLang", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "EngLit", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Maths", Selected = true }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "CombSci", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Bio", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Chem", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Phys", Selected = false }, o => o.ExcludingMissingMembers())
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
            o => o.Should().BeEquivalentTo(new { Key = "Att8", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "EngMat", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "EngLang", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "EngLit", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Maths", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "CombSci", Selected = true }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Bio", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Chem", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Phys", Selected = false }, o => o.ExcludingMissingMembers())
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
            o => o.Should().BeEquivalentTo(new { Key = "Att8", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "EngMat", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "EngLang", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "EngLit", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Maths", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "CombSci", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Bio", Selected = true }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Chem", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Phys", Selected = false }, o => o.ExcludingMissingMembers())
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
            o => o.Should().BeEquivalentTo(new { Key = "Att8", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "EngMat", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "EngLang", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "EngLit", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Maths", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "CombSci", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Bio", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Chem", Selected = true }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Phys", Selected = false }, o => o.ExcludingMissingMembers())
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
            o => o.Should().BeEquivalentTo(new { Key = "Att8", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "EngMat", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "EngLang", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "EngLit", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Maths", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "CombSci", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Bio", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Chem", Selected = false }, o => o.ExcludingMissingMembers()),
            o => o.Should().BeEquivalentTo(new { Key = "Phys", Selected = true }, o => o.ExcludingMissingMembers())
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