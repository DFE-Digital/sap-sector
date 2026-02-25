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
    public async Task GetSimilarSchoolUrnsAsync_ReturnsNeighbourUrns()
    {
        var rows = new List<SimilarSchoolsSecondaryGroupsRow>
        {
            new() { URN = "123456", NeighbourURN = "654321", Dist = "0.1", Rank = "1" },
            new() { URN = "123456", NeighbourURN = "654322", Dist = "0.2", Rank = "2" },
            new() { URN = "111111", NeighbourURN = "222222", Dist = "0.3", Rank = "1" }
        };
        _groupsRepo.Setup(r => r.ReadAllAsync()).ReturnsAsync(rows);

        var sut = CreateSut();

        var result = await sut.GetSimilarSchoolUrnsAsync("123456");

        Assert.Equal(2, result.Count);
        Assert.Contains("654321", result);
        Assert.Contains("654322", result);
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
            Easting = "430000",
            Northing = "380000",
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
            Easting = "431000",
            Northing = "381000",
            UrbanRuralId = "A1",
            UrbanRuralName = "Urban"
        };
        _establishmentRepo.Setup(r => r.ReadAllAsync()).ReturnsAsync(new List<Establishment> { current, similar });

        _groupsRepo.Setup(r => r.ReadAllAsync()).ReturnsAsync(new List<SimilarSchoolsSecondaryGroupsRow>
        {
            new() { URN = "123456", NeighbourURN = "654321", Dist = "0.1", Rank = "1" }
        });

        _performanceRepo.Setup(r => r.ReadAllAsync()).ReturnsAsync(new List<EstablishmentPerformance>
        {
            new() { Id = "123456", Attainment8_Tot_Est_Current_Num = "50" },
            new() { Id = "654321", Attainment8_Tot_Est_Current_Num = "45" }
        });

        var sut = CreateSut();

        var (currentSchool, similarSchools) = await sut.GetSimilarSchoolsGroupAsync("123456");

        Assert.Equal("123456", currentSchool.URN);
        Assert.Equal("Current School", currentSchool.Name);
        Assert.Single(similarSchools);
        Assert.Equal("654321", similarSchools.First().URN);
    }

    [Fact]
    public async Task GetSecondaryValuesByUrnsAsync_ReturnsParsedValues_AndFillsMissingUrns()
    {
        _valuesRepo.Setup(r => r.ReadAllAsync()).ReturnsAsync(new List<SimilarSchoolsSecondaryValuesRow>
        {
            new()
            {
                URN = "123456",
                Ks2Rp = "104.5",
                Ks2Mp = "104.0",
                PpPerc = "50.5",
                PercentEal = "10.1",
                Polar4QuintilePupils = "3",
                PStability = "90",
                IdaciPupils = "0.316",
                PercentSchSupport = "12.5",
                NumberOfPupils = "500",
                PercentStatementOrEhp = "2.1",
                Att8Scr = "42"
            }
        });

        var sut = CreateSut();

        var result = await sut.GetSecondaryValuesByUrnsAsync(new[] { "123456", "654321" });

        Assert.Equal(2, result.Count);
        Assert.Contains(result, v => v.Urn == "123456");
        Assert.Contains(result, v => v.Urn == "654321");

        var a = result.Single(v => v.Urn == "123456");
        Assert.Equal(104.5m, a.Ks2ReadingScore);
        Assert.Equal(3, a.Polar4Quintile);
        Assert.Equal(500, a.PupilCount);

        var b = result.Single(v => v.Urn == "654321");
        Assert.Equal(0m, b.Ks2ReadingScore);
        Assert.Equal(0, b.PupilCount);
    }
}
