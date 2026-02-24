using Moq;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Features.SimilarSchools.UseCases;

namespace SAPSec.Core.Tests.Features.SimilarSchools.UseCases;

public class GetCharacteristicsComparisonTests
{
    private readonly Mock<ISimilarSchoolsSecondaryRepository> _repo = new();

    private GetCharacteristicsComparison CreateSut() => new(_repo.Object);

    [Fact]
    public async Task Execute_ReturnsCurrentAndSimilarValues()
    {
        var current = new SimilarSchoolsSecondaryValues { Urn = "A", Ks2Rp = 1m, Ks2Mp = 2m };
        var similar = new SimilarSchoolsSecondaryValues { Urn = "B", Ks2Rp = 3m, Ks2Mp = 4m };

        _repo.Setup(r => r.GetSecondaryValuesByUrnsAsync(It.Is<IReadOnlyCollection<string>>(
                u => u.Contains("A") && u.Contains("B"))))
            .ReturnsAsync(new Dictionary<string, SimilarSchoolsSecondaryValues>
            {
                ["A"] = current,
                ["B"] = similar
            });

        var sut = CreateSut();

        var result = await sut.Execute(new GetCharacteristicsComparisonRequest("A", "B"));

        Assert.Same(current, result.CurrentSchool);
        Assert.Same(similar, result.SimilarSchool);
    }

    [Fact]
    public async Task Execute_ThrowsWhenCurrentSchoolMissing()
    {
        _repo.Setup(r => r.GetSecondaryValuesByUrnsAsync(It.IsAny<IReadOnlyCollection<string>>()))
            .ReturnsAsync(new Dictionary<string, SimilarSchoolsSecondaryValues>
            {
                ["B"] = new SimilarSchoolsSecondaryValues { Urn = "B" }
            });

        var sut = CreateSut();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.Execute(new GetCharacteristicsComparisonRequest("A", "B")));

        Assert.Contains("A", ex.Message);
    }

    [Fact]
    public async Task Execute_ThrowsWhenSimilarSchoolMissing()
    {
        _repo.Setup(r => r.GetSecondaryValuesByUrnsAsync(It.IsAny<IReadOnlyCollection<string>>()))
            .ReturnsAsync(new Dictionary<string, SimilarSchoolsSecondaryValues>
            {
                ["A"] = new SimilarSchoolsSecondaryValues { Urn = "A" }
            });

        var sut = CreateSut();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.Execute(new GetCharacteristicsComparisonRequest("A", "B")));

        Assert.Contains("B", ex.Message);
    }
}
