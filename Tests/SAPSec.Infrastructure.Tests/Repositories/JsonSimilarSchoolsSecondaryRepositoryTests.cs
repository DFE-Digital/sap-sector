using Moq;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Model;
using SAPSec.Core.Model.KS4.Performance;
using SAPSec.Infrastructure.Json;

namespace SAPSec.Infrastructure.Tests.Repositories;

public class JsonSimilarSchoolsSecondaryRepositoryTests
{
    private readonly Mock<IJsonFile<SimilarSchoolsSecondaryGroupsRow>> _groupsRepo = new();
    private readonly Mock<IJsonFile<SimilarSchoolsSecondaryValuesRow>> _valuesRepo = new();
    private readonly Mock<IJsonFile<Establishment>> _establishmentRepo = new();
    private readonly Mock<IJsonFile<EstablishmentPerformance>> _performanceRepo = new();
    private readonly Mock<IJsonFile<SimilarSchoolsSecondaryNationalSD>> _nationalSdRepo = new();

    private JsonSimilarSchoolsSecondaryRepository CreateSut() =>
        new(
            _groupsRepo.Object,
            _valuesRepo.Object,
            _establishmentRepo.Object,
            _performanceRepo.Object,
            _nationalSdRepo.Object);

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
    public async Task GetSecondaryValuesByUrnsAsync_ReturnsParsedValues()
    {
        _valuesRepo.Setup(r => r.ReadAllAsync()).ReturnsAsync(new List<SimilarSchoolsSecondaryValuesRow>
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
        Assert.Contains(result, v => v.Urn == "123456");

        var a = result.Single(v => v.Urn == "123456");
        Assert.Equal(104.5m, a.Ks2ReadingScore);
        Assert.Equal(3m, a.Polar4Quintile);
        Assert.Equal(500m, a.PupilCount);

    }

    [Fact]
    public async Task GetSimilarSchoolsSecondaryNationalSdAsync_ReturnsFirstRow()
    {
        var rows = new List<SimilarSchoolsSecondaryNationalSD>
        {
            new()
            {
                Ks2AverageScore = 2.45m,
                PupilPremiumEligibilityPercentage = 10m,
                PupilsWithEalPercentage = 5m,
                Polar4Quintile = 1.1m,
                PupilStabilityRate = 6m,
                AverageIdaciScore = 0.08m,
                PupilsWithSenSupportPercentage = 3m,
                PupilCount = 400m,
                PupilsWithEhcPlanPercentage = 1.5m
            }
        };
        _nationalSdRepo.Setup(r => r.ReadAllAsync()).ReturnsAsync(rows);

        var sut = CreateSut();

        var result = await sut.GetSimilarSchoolsSecondaryNationalSdAsync();

        Assert.Equal(2.45m, result.Ks2AverageScore);
    }

    [Fact]
    public async Task GetSimilarSchoolsSecondaryNationalSdAsync_Throws_WhenEmpty()
    {
        _nationalSdRepo.Setup(r => r.ReadAllAsync()).ReturnsAsync(Array.Empty<SimilarSchoolsSecondaryNationalSD>());

        var sut = CreateSut();

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.GetSimilarSchoolsSecondaryNationalSdAsync());
    }
}
