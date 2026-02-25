using Moq;
using SAPSec.Core;
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
        var current = new SimilarSchoolsSecondaryValues { Urn = "123456", Ks2ReadingScore = 1m, Ks2MathsScore = 2m };
        var similar = new SimilarSchoolsSecondaryValues { Urn = "654321", Ks2ReadingScore = 3m, Ks2MathsScore = 4m };

        _repo.Setup(r => r.GetSecondaryValuesByUrnsAsync(It.Is<IEnumerable<string>>(
                u => u.Contains("123456") && u.Contains("654321"))))
            .ReturnsAsync(new List<SimilarSchoolsSecondaryValues> { current, similar });

        var sut = CreateSut();

        var result = await sut.Execute(new GetCharacteristicsComparisonRequest("123456", "654321"));

        Assert.Same(current, result.CurrentSchool);
        Assert.Same(similar, result.SimilarSchool);
    }

    [Fact]
    public async Task Execute_ComputesKs2AverageScore()
    {
        var current = new SimilarSchoolsSecondaryValues { Urn = "123456", Ks2ReadingScore = 104.5m, Ks2MathsScore = 105.5m };
        var similar = new SimilarSchoolsSecondaryValues { Urn = "654321", Ks2ReadingScore = 103.0m, Ks2MathsScore = 101.0m };

        _repo.Setup(r => r.GetSecondaryValuesByUrnsAsync(It.Is<IEnumerable<string>>(
                u => u.Contains("123456") && u.Contains("654321"))))
            .ReturnsAsync(new List<SimilarSchoolsSecondaryValues> { current, similar });

        var sut = CreateSut();

        var result = await sut.Execute(new GetCharacteristicsComparisonRequest("123456", "654321"));

        Assert.Equal(105.0m, result.CurrentSchool.Ks2AverageScore);
        Assert.Equal(102.0m, result.SimilarSchool.Ks2AverageScore);
    }

    [Fact]
    public async Task Execute_ThrowsWhenCurrentSchoolMissing()
    {
        _repo.Setup(r => r.GetSecondaryValuesByUrnsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<SimilarSchoolsSecondaryValues>
            {
                new SimilarSchoolsSecondaryValues { Urn = "654321" }
            });

        var sut = CreateSut();

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.Execute(new GetCharacteristicsComparisonRequest("123456", "654321")));

        Assert.Contains("123456", ex.Message);
    }

    [Fact]
    public async Task Execute_ThrowsWhenSimilarSchoolMissing()
    {
        _repo.Setup(r => r.GetSecondaryValuesByUrnsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<SimilarSchoolsSecondaryValues>
            {
                new SimilarSchoolsSecondaryValues { Urn = "123456" }
            });

        var sut = CreateSut();

        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            sut.Execute(new GetCharacteristicsComparisonRequest("123456", "654321")));

        Assert.Contains("654321", ex.Message);
    }
}
