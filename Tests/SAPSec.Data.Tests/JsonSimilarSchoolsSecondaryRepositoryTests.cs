using Moq;
using SAPSec.Data.Json;
using SAPSec.Data.Model.Generated;

namespace SAPSec.Data.Tests;

public class JsonSimilarSchoolsSecondaryRepositoryTests
{
    private readonly Mock<IJsonFile<SimilarSchoolsSecondaryGroupsEntry>> _groupsRepo = new();
    private readonly Mock<IJsonFile<SimilarSchoolsSecondaryValuesEntry>> _valuesRepo = new();
    private readonly Mock<IJsonFile<Establishment>> _establishmentRepo = new();
    private readonly Mock<IJsonFile<EstablishmentPerformance>> _performanceRepo = new();
    private readonly Mock<IJsonFile<SimilarSchoolsSecondaryStandardDeviationsEntry>> _standardDeviationsRepo = new();

    private JsonSimilarSchoolsSecondaryRepository CreateSut() =>
        new(
            _groupsRepo.Object,
            _valuesRepo.Object,
            _establishmentRepo.Object,
            _performanceRepo.Object,
            _standardDeviationsRepo.Object);

    [Fact]
    public async Task GetSimilarSchoolUrnsAsync_ReturnsNeighbourUrns()
    {
        var rows = new List<SimilarSchoolsSecondaryGroupsEntry>
        {
            new() { URN = "123456", NeighbourURN = "654321", Dist = "0.1", Rank = "1" },
            new() { URN = "123456", NeighbourURN = "654322", Dist = "0.2", Rank = "2" },
            new() { URN = "111111", NeighbourURN = "222222", Dist = "0.3", Rank = "1" }
        };
        _groupsRepo.Setup(r => r.ReadAllAsync()).ReturnsAsync(rows);

        var sut = CreateSut();

        var result = await sut.GetSimilarSchoolsGroupAsync("123456");

        Assert.Equal(2, result.Count);
        Assert.Contains("654321", result.Select(r => r.NeighbourURN));
        Assert.Contains("654322", result.Select(r => r.NeighbourURN));
    }

    [Fact]
    public async Task GetSimilarSchoolsGroupAsync_ReturnsCurrentAndSimilarSchools()
    {
        var current = new Establishment
        {
            URN = "123456",
            EstablishmentName = "Current School",
            LAId = "100",
            LAName = "LA",
            Street = "Street",
            Town = "Town",
            Postcode = "PC1 1AA",
            Easting = 430000,
            Northing = 380000,
            UrbanRuralId = "A1",
            UrbanRuralName = "Urban"
        };
        var similar = new Establishment
        {
            URN = "654321",
            EstablishmentName = "Similar School",
            LAId = "100",
            LAName = "LA",
            Street = "Street",
            Town = "Town",
            Postcode = "PC1 1AA",
            Easting = 431000,
            Northing = 381000,
            UrbanRuralId = "A1",
            UrbanRuralName = "Urban"
        };
        _establishmentRepo.Setup(r => r.ReadAllAsync()).ReturnsAsync(new List<Establishment> { current, similar });

        _groupsRepo.Setup(r => r.ReadAllAsync()).ReturnsAsync(new List<SimilarSchoolsSecondaryGroupsEntry>
        {
            new() { URN = "123456", NeighbourURN = "654321", Dist = "0.1", Rank = "1" }
        });

        _performanceRepo.Setup(r => r.ReadAllAsync()).ReturnsAsync(new List<EstablishmentPerformance>
        {
            new() { Id = "123456", Attainment8_Tot_Est_Current_Num = "50" },
            new() { Id = "654321", Attainment8_Tot_Est_Current_Num = "45" }
        });

        var sut = CreateSut();

        var group = await sut.GetSimilarSchoolsGroupAsync("123456");

        //Assert.Equal("123456", currentSchool.URN);
        //Assert.Equal("Current School", currentSchool.Name);
        Assert.Single(group);
        Assert.Equal("654321", group.First().NeighbourURN);
    }

    [Fact]
    public async Task GetSecondaryValuesByUrnsAsync_ReturnsParsedValues()
    {
        _valuesRepo.Setup(r => r.ReadAllAsync()).ReturnsAsync(new List<SimilarSchoolsSecondaryValuesEntry>
        {
            new()
            {
                URN = "123456",
                KS2RP = "104.5",
                KS2MP = "104.0",
                PPPerc = "50.5",
                PercentEAL = "10.1",
                Polar4QuintilePupils = "3",
                PStability = "90",
                IdaciPupils = "0.316",
                PercentSchSupport = "12.5",
                NumberOfPupils = "500",
                PercentageStatementOrEHP = "2.1",
                Att8Scr = "42"
            }
        });

        var sut = CreateSut();

        var result = await sut.GetSecondaryValuesByUrnsAsync(new[] { "123456", "654321" });

        Assert.Single(result);
        Assert.Contains(result, v => v.URN == "123456");

        var a = result.Single(v => v.URN == "123456");
        Assert.Equal("104.5", a.KS2RP);
        Assert.Equal("3", a.Polar4QuintilePupils);
        Assert.Equal("500", a.NumberOfPupils);
    }

    [Fact]
    public async Task GetSimilarSchoolsSecondaryStandardDeviationsAsync_ReturnsFirstRow()
    {
        var rows = new List<SimilarSchoolsSecondaryStandardDeviationsEntry>
        {
            new()
            {
                KS2AVG = 2.45M,
                PPPerc = 10M,
                PercentEAL = 5M,
                Polar4QuintilePupils = 1.1M,
                PStability = 6M,
                IdaciPupils = 0.08M,
                PercentSchSupport = 3M,
                NumberOfPupils = 400M,
                PercentageStatementOrEHP = 1.5M
            }
        };
        _standardDeviationsRepo.Setup(r => r.ReadAllAsync()).ReturnsAsync(rows);

        var sut = CreateSut();

        var result = await sut.GetSimilarSchoolsSecondaryStandardDeviationsAsync();

        Assert.Equal(2.45m, result.KS2AVG);
    }

    [Fact]
    public async Task GetSimilarSchoolsSecondaryStandardDeviationsAsync_Throws_WhenEmpty()
    {
        _standardDeviationsRepo.Setup(r => r.ReadAllAsync()).ReturnsAsync(Array.Empty<SimilarSchoolsSecondaryStandardDeviationsEntry>());

        var sut = CreateSut();

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.GetSimilarSchoolsSecondaryStandardDeviationsAsync());
    }
}
