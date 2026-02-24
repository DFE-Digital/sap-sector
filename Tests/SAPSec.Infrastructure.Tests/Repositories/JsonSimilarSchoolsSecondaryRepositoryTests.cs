using Moq;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Model;
using SAPSec.Core.Model.KS4.Performance;
using SAPSec.Infrastructure.Repositories;
using SAPSec.Infrastructure.Repositories.Json;

namespace SAPSec.Infrastructure.Tests.Repositories;

public class JsonSimilarSchoolsSecondaryRepositoryTests
{
    private readonly Mock<IJsonFile<SimilarSchoolsSecondaryGroupsRow>> _groupsRepo = new();
    private readonly Mock<IJsonFile<SimilarSchoolsSecondaryValuesRow>> _valuesRepo = new();
    private readonly Mock<IJsonFile<Establishment>> _establishmentRepo = new();
    private readonly Mock<IJsonFile<EstablishmentPerformance>> _performanceRepo = new();

    private JsonSimilarSchoolsSecondaryRepository CreateSut() =>
        new(
            _groupsRepo.Object,
            _valuesRepo.Object,
            _establishmentRepo.Object,
            _performanceRepo.Object);

    [Fact]
    public async Task GetSimilarSchoolUrnsAsync_ReturnsSimilarUrns()
    {
        var rows = new List<SimilarSchoolsSecondaryGroupsRow>
        {
            new() { URN = "A", NeighbourURN = "B", Dist = "0.1", Rank = "1" },
            new() { URN = "A", NeighbourURN = "C", Dist = "0.2", Rank = "2" },
            new() { URN = "X", NeighbourURN = "Y", Dist = "0.3", Rank = "1" }
        };
        _groupsRepo.Setup(r => r.ReadAllAsync()).ReturnsAsync(rows);

        var sut = CreateSut();

        var result = await sut.GetSimilarSchoolUrnsAsync("A");

        Assert.Equal(2, result.Count);
        Assert.Contains("B", result);
        Assert.Contains("C", result);
    }

    [Fact]
    public async Task GetSimilarSchoolsGroupAsync_ReturnsCurrentAndSimilarSchools()
    {
        var current = new Establishment
        {
            URN = "A",
            EstablishmentName = "Current School",
            LAId = "100",
            LAName = "LA",
            Street = "Street",
            Town = "Town",
            Postcode = "PC1 1AA",
            Easting = "430000",
            Northing = "380000",
            UrbanRuralId = "A1",
            UrbanRuralName = "Urban"
        };
        var similar = new Establishment
        {
            URN = "B",
            EstablishmentName = "Similar School",
            LAId = "100",
            LAName = "LA",
            Street = "Street",
            Town = "Town",
            Postcode = "PC1 1AA",
            Easting = "431000",
            Northing = "381000",
            UrbanRuralId = "A1",
            UrbanRuralName = "Urban"
        };
        _establishmentRepo.Setup(r => r.ReadAllAsync()).ReturnsAsync(new List<Establishment> { current, similar });

        _groupsRepo.Setup(r => r.ReadAllAsync()).ReturnsAsync(new List<SimilarSchoolsSecondaryGroupsRow>
        {
            new() { URN = "A", NeighbourURN = "B", Dist = "0.1", Rank = "1" }
        });

        _performanceRepo.Setup(r => r.ReadAllAsync()).ReturnsAsync(new List<EstablishmentPerformance>
        {
            new() { Id = "A", Attainment8_Tot_Est_Current_Num = "50" },
            new() { Id = "B", Attainment8_Tot_Est_Current_Num = "45" }
        });

        var sut = CreateSut();

        var (currentSchool, similarSchools) = await sut.GetSimilarSchoolsGroupAsync("A");

        Assert.Equal("A", currentSchool.URN);
        Assert.Equal("Current School", currentSchool.Name);
        Assert.Single(similarSchools);
        Assert.Equal("B", similarSchools.First().URN);
    }

    [Fact]
    public async Task GetSecondaryValuesByUrnsAsync_ReturnsParsedValues_AndFillsMissingUrns()
    {
        _valuesRepo.Setup(r => r.ReadAllAsync()).ReturnsAsync(new List<SimilarSchoolsSecondaryValuesRow>
        {
            new()
            {
                URN = "A",
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

        var result = await sut.GetSecondaryValuesByUrnsAsync(new[] { "A", "B" });

        Assert.Equal(2, result.Count);
        Assert.True(result.ContainsKey("A"));
        Assert.True(result.ContainsKey("B"));

        Assert.Equal(104.5m, result["A"].Ks2Rp);
        Assert.Equal(3, result["A"].Polar4QuintilePupils);
        Assert.Equal(500, result["A"].NumberOfPupils);

        Assert.Equal(0m, result["B"].Ks2Rp);
        Assert.Equal(0, result["B"].NumberOfPupils);
    }
}
