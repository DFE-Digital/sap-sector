using Moq;
using SAPSec.Data.Dto.SimilarSchools.Primary;
using SAPSec.Infrastructure.Json;

namespace SAPSec.Infrastructure.Tests.Repositories;

public class JsonSimilarSchoolsPrimaryRepositoryTests
{
    private readonly Mock<IJsonFile<SimilarSchoolsPrimaryGroupsEntry>> _groupsRepo = new();
    private readonly Mock<IJsonFile<SimilarSchoolsPrimaryValuesEntry>> _valuesRepo = new();

    private JsonSimilarSchoolsPrimaryRepository CreateSut() =>
        new(_groupsRepo.Object, _valuesRepo.Object);

    [Fact]
    public async Task GetSimilarSchoolsGroupAsync_ReturnsNeighbourUrns()
    {
        _groupsRepo.Setup(r => r.ReadAllAsync()).ReturnsAsync(new List<SimilarSchoolsPrimaryGroupsEntry>
        {
            new() { URN = "123456", NeighbourURN = "654321", Dist = "0.1", Rank = "1" },
            new() { URN = "123456", NeighbourURN = "654322", Dist = "0.2", Rank = "2" },
            new() { URN = "111111", NeighbourURN = "222222", Dist = "0.3", Rank = "1" }
        });

        var sut = CreateSut();

        var result = await sut.GetSimilarSchoolsGroupAsync("123456");

        result.Should().HaveCount(2);
        result.Select(r => r.NeighbourURN).Should().BeEquivalentTo("654321", "654322");
    }

    [Fact]
    public async Task GetPrimaryValuesByUrnsAsync_ReturnsMatchedValues()
    {
        _valuesRepo.Setup(r => r.ReadAllAsync()).ReturnsAsync(new List<SimilarSchoolsPrimaryValuesEntry>
        {
            new()
            {
                URN = "123456",
                PPPerc = "40.2",
                Polar4QuintilePupils = "3",
                PStability = "89.1",
                PercentSchSupport = "12.5",
                PercentEAL = "10.1",
                IdaciPupils = "0.316",
                PercentageStatementOrEhp = "2.1",
                NumberOfPupils = "300",
                ReadMatAverage = "98.4",
                Ks1PriorRwmAverage = "12.3"
            }
        });

        var sut = CreateSut();

        var result = await sut.GetPrimaryValuesByUrnsAsync(["123456", "654321"]);

        result.Should().ContainSingle();
        result.Single().URN.Should().Be("123456");
        result.Single().Ks1PriorRwmAverage.Should().Be("12.3");
    }

    [Fact]
    public async Task GetAllUrnsInSimilarSchoolsDataSet_ReturnsAllUrns()
    {
        _valuesRepo.Setup(r => r.ReadAllAsync()).ReturnsAsync(new List<SimilarSchoolsPrimaryValuesEntry>
        {
            new() { URN = "123456" },
            new() { URN = "654321" }
        });

        var sut = CreateSut();

        var result = await sut.GetAllUrnsInSimilarSchoolsDataSet();

        result.Should().BeEquivalentTo("123456", "654321");
    }
}
