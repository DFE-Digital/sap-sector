using Moq;
using SAPSec.Core;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Features.SimilarSchools.UseCases;

namespace SAPSec.Core.Tests.Features.SimilarSchools.UseCases;

public class GetCharacteristicsComparisonTests
{
    private readonly Mock<ISimilarSchoolsSecondaryRepository> _repo = new();

    private GetCharacteristicsComparison CreateSut() => new(_repo.Object);

    [Theory]
    [InlineData(0.2, SchoolSimilarity.Similar)]
    [InlineData(0.3, SchoolSimilarity.Similar)]
    [InlineData(0.5, SchoolSimilarity.LessSimilar)]
    [InlineData(0.7, SchoolSimilarity.LessSimilar)]
    [InlineData(0.8, SchoolSimilarity.NotSimilar)]
    [InlineData(-0.2, SchoolSimilarity.Similar)]
    [InlineData(-0.5, SchoolSimilarity.LessSimilar)]
    [InlineData(-0.8, SchoolSimilarity.NotSimilar)]
    public async Task Execute_ComputesSimilarityLabels_ByThreshold(double diffInSd, SchoolSimilarity expected)
    {
        var currentUrn = "100";
        var similarUrn = "200";

        var sd = 10m;
        var diff = (decimal)diffInSd * sd;

        var current = BuildValues(currentUrn, ks2Avg: 100m);
        var similar = BuildValues(similarUrn, ks2Avg: 100m + diff);

        _repo.Setup(r => r.GetSecondaryValuesByUrnsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new[] { current, similar });
        _repo.Setup(r => r.GetSimilarSchoolsSecondaryStandardDeviationsAsync())
            .ReturnsAsync(BuildStandardDeviations(ks2AvgSd: sd));

        var sut = CreateSut();

        var result = await sut.Execute(new GetCharacteristicsComparisonRequest(currentUrn, similarUrn));

        Assert.Equal(expected, result.Ks2AverageScore.Similarity);
    }

    [Fact]
    public async Task Execute_ReturnsNotSimilar_WhenSdIsZero()
    {
        var currentUrn = "100";
        var similarUrn = "200";

        var current = BuildValues(currentUrn, ks2Avg: 100m);
        var similar = BuildValues(similarUrn, ks2Avg: 120m);

        _repo.Setup(r => r.GetSecondaryValuesByUrnsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new[] { current, similar });
        _repo.Setup(r => r.GetSimilarSchoolsSecondaryStandardDeviationsAsync())
            .ReturnsAsync(BuildStandardDeviations(ks2AvgSd: 0m));

        var sut = CreateSut();

        var result = await sut.Execute(new GetCharacteristicsComparisonRequest(currentUrn, similarUrn));

        Assert.Equal(SchoolSimilarity.NotSimilar, result.Ks2AverageScore.Similarity);
    }

    [Fact]
    public async Task Execute_ReturnsNotSimilar_WhenSdIsNegative()
    {
        var currentUrn = "100";
        var similarUrn = "200";

        var current = BuildValues(currentUrn, ks2Avg: 100m);
        var similar = BuildValues(similarUrn, ks2Avg: 120m);

        _repo.Setup(r => r.GetSecondaryValuesByUrnsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new[] { current, similar });
        _repo.Setup(r => r.GetSimilarSchoolsSecondaryStandardDeviationsAsync())
            .ReturnsAsync(BuildStandardDeviations(ks2AvgSd: -10m));

        var sut = CreateSut();

        var result = await sut.Execute(new GetCharacteristicsComparisonRequest(currentUrn, similarUrn));

        Assert.Equal(SchoolSimilarity.NotSimilar, result.Ks2AverageScore.Similarity);
    }

    [Fact]
    public async Task Execute_ThrowsNotFound_WhenCurrentSchoolMissing()
    {
        var currentUrn = "100";
        var similarUrn = "200";

        var similar = BuildValues(similarUrn, ks2Avg: 120m);

        _repo.Setup(r => r.GetSecondaryValuesByUrnsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new[] { similar });
        _repo.Setup(r => r.GetSimilarSchoolsSecondaryStandardDeviationsAsync())
            .ReturnsAsync(BuildStandardDeviations(ks2AvgSd: 10m));

        var sut = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(
            () => sut.Execute(new GetCharacteristicsComparisonRequest(currentUrn, similarUrn)));
    }

    [Fact]
    public async Task Execute_ThrowsNotFound_WhenSimilarSchoolMissing()
    {
        var currentUrn = "100";
        var similarUrn = "200";

        var current = BuildValues(currentUrn, ks2Avg: 100m);

        _repo.Setup(r => r.GetSecondaryValuesByUrnsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new[] { current });
        _repo.Setup(r => r.GetSimilarSchoolsSecondaryStandardDeviationsAsync())
            .ReturnsAsync(BuildStandardDeviations(ks2AvgSd: 10m));

        var sut = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(
            () => sut.Execute(new GetCharacteristicsComparisonRequest(currentUrn, similarUrn)));
    }

    [Fact]
    public async Task Execute_ReturnsRoundedIntComparisons_ForPupilCountAndPolar4()
    {
        var currentUrn = "100";
        var similarUrn = "200";

        var current = BuildValues(currentUrn, ks2Avg: 100m, pupilCount: 100.5m, polar4Quintile: 1.4m);
        var similar = BuildValues(similarUrn, ks2Avg: 102m, pupilCount: 102.5m, polar4Quintile: 2.6m);

        _repo.Setup(r => r.GetSecondaryValuesByUrnsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new[] { current, similar });
        _repo.Setup(r => r.GetSimilarSchoolsSecondaryStandardDeviationsAsync())
            .ReturnsAsync(new SimilarSchoolsSecondaryStandardDeviations
            {
                Ks2AverageScore = 10m,
                PupilPremiumEligibilityPercentage = 10m,
                PupilsWithEalPercentage = 10m,
                Polar4Quintile = 1m,
                PupilStabilityRate = 10m,
                AverageIdaciScore = 10m,
                PupilsWithSenSupportPercentage = 10m,
                PupilCount = 2m,
                PupilsWithEhcPlanPercentage = 10m
            });

        var sut = CreateSut();

        var result = await sut.Execute(new GetCharacteristicsComparisonRequest(currentUrn, similarUrn));

        Assert.Equal(101, result.PupilCount.CurrentSchoolValue);
        Assert.Equal(103, result.PupilCount.SimilarSchoolValue);
        Assert.Equal(SchoolSimilarity.NotSimilar, result.PupilCount.Similarity);

        Assert.Equal(1, result.Polar4Quintile.CurrentSchoolValue);
        Assert.Equal(3, result.Polar4Quintile.SimilarSchoolValue);
        Assert.Equal(SchoolSimilarity.NotSimilar, result.Polar4Quintile.Similarity);
    }

    [Fact]
    public async Task Execute_ComputesAllMetricSimilarities()
    {
        var currentUrn = "100";
        var similarUrn = "200";

        var current = BuildValues(
            currentUrn,
            ks2Avg: 100m,
            pp: 50m,
            eal: 20m,
            polar4Quintile: 2m,
            stability: 90m,
            idaci: 0.3m,
            senSupport: 10m,
            pupilCount: 600m,
            ehcp: 2m);

        var similar = BuildValues(
            similarUrn,
            ks2Avg: 102m,          // diff 2 / sd10 => Similar
            pp: 56m,               // diff 6 / sd10 => LessSimilar
            eal: 30m,              // diff 10 / sd10 => NotSimilar
            polar4Quintile: 3m,    // diff 1 / sd1 => NotSimilar
            stability: 91m,        // diff 1 / sd10 => Similar
            idaci: 0.8m,           // diff 0.5 / sd1 => LessSimilar
            senSupport: 15m,       // diff 5 / sd10 => LessSimilar
            pupilCount: 603m,      // diff 3 / sd10 => Similar
            ehcp: 6m);             // diff 4 / sd10 => LessSimilar

        _repo.Setup(r => r.GetSecondaryValuesByUrnsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new[] { current, similar });
        _repo.Setup(r => r.GetSimilarSchoolsSecondaryStandardDeviationsAsync())
            .ReturnsAsync(new SimilarSchoolsSecondaryStandardDeviations
            {
                Ks2AverageScore = 10m,
                PupilPremiumEligibilityPercentage = 10m,
                PupilsWithEalPercentage = 10m,
                Polar4Quintile = 1m,
                PupilStabilityRate = 10m,
                AverageIdaciScore = 1m,
                PupilsWithSenSupportPercentage = 10m,
                PupilCount = 10m,
                PupilsWithEhcPlanPercentage = 10m
            });

        var sut = CreateSut();

        var result = await sut.Execute(new GetCharacteristicsComparisonRequest(currentUrn, similarUrn));

        Assert.Equal(SchoolSimilarity.Similar, result.Ks2AverageScore.Similarity);
        Assert.Equal(SchoolSimilarity.LessSimilar, result.PupilPremiumEligibilityPercentage.Similarity);
        Assert.Equal(SchoolSimilarity.NotSimilar, result.PupilsWithEalPercentage.Similarity);
        Assert.Equal(SchoolSimilarity.NotSimilar, result.Polar4Quintile.Similarity);
        Assert.Equal(SchoolSimilarity.Similar, result.PupilStabilityRate.Similarity);
        Assert.Equal(SchoolSimilarity.LessSimilar, result.AverageIdaciScore.Similarity);
        Assert.Equal(SchoolSimilarity.LessSimilar, result.PupilsWithSenSupportPercentage.Similarity);
        Assert.Equal(SchoolSimilarity.Similar, result.PupilCount.Similarity);
        Assert.Equal(SchoolSimilarity.LessSimilar, result.PupilsWithEhcPlanPercentage.Similarity);
    }

    [Fact]
    public async Task Execute_UsesNationalSd_ByDefault()
    {
        var currentUrn = "100";
        var similarUrn = "200";

        var current = BuildValues(currentUrn, ks2Avg: 100m);
        var similar = BuildValues(similarUrn, ks2Avg: 106m);

        _repo.Setup(r => r.GetSecondaryValuesByUrnsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new[] { current, similar });
        _repo.Setup(r => r.GetSimilarSchoolsSecondaryStandardDeviationsAsync())
            .ReturnsAsync(BuildStandardDeviations(ks2AvgSd: 20m));

        var sut = CreateSut();

        var result = await sut.Execute(new GetCharacteristicsComparisonRequest(currentUrn, similarUrn));

        Assert.Equal(SchoolSimilarity.Similar, result.Ks2AverageScore.Similarity);
        _repo.Verify(r => r.GetSimilarSchoolsSecondaryStandardDeviationsAsync(), Times.Once);
        _repo.Verify(r => r.GetSimilarSchoolUrnsAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Execute_UsesNationalSd_WhenExplicitlyRequested()
    {
        var currentUrn = "100";
        var similarUrn = "200";

        var current = BuildValues(currentUrn, ks2Avg: 100m);
        var similar = BuildValues(similarUrn, ks2Avg: 106m);

        _repo.Setup(r => r.GetSecondaryValuesByUrnsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new[] { current, similar });
        _repo.Setup(r => r.GetSimilarSchoolsSecondaryStandardDeviationsAsync())
            .ReturnsAsync(BuildStandardDeviations(ks2AvgSd: 20m));

        var sut = CreateSut();

        var result = await sut.Execute(new GetCharacteristicsComparisonRequest(
            currentUrn,
            similarUrn,
            SimilarityCalculationMethod.National));

        Assert.Equal(SchoolSimilarity.Similar, result.Ks2AverageScore.Similarity);
        _repo.Verify(r => r.GetSimilarSchoolsSecondaryStandardDeviationsAsync(), Times.Once);
        _repo.Verify(r => r.GetSimilarSchoolUrnsAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Execute_UsesGroupSd_WhenRequested()
    {
        var currentUrn = "100";
        var similarUrn = "200";

        var current = BuildValues(currentUrn, ks2Avg: 100m);
        var similar = BuildValues(similarUrn, ks2Avg: 106m);

        var groupUrns = new[] { "200", "300", "400", "500", "600" };
        var groupValues = new[]
        {
            BuildValues("200", ks2Avg: 100m),
            BuildValues("300", ks2Avg: 102m),
            BuildValues("400", ks2Avg: 104m),
            BuildValues("500", ks2Avg: 106m),
            BuildValues("600", ks2Avg: 108m)
        };

        _repo.Setup(r => r.GetSecondaryValuesByUrnsAsync(It.Is<IEnumerable<string>>(u =>
                u.Contains(currentUrn) && u.Contains(similarUrn) && u.Count() == 2)))
            .ReturnsAsync(new[] { current, similar });
        _repo.Setup(r => r.GetSimilarSchoolUrnsAsync(currentUrn))
            .ReturnsAsync(groupUrns);
        _repo.Setup(r => r.GetSecondaryValuesByUrnsAsync(It.Is<IEnumerable<string>>(u =>
                u.SequenceEqual(groupUrns))))
            .ReturnsAsync(groupValues);

        var sut = CreateSut();

        var result = await sut.Execute(new GetCharacteristicsComparisonRequest(
            currentUrn,
            similarUrn,
            SimilarityCalculationMethod.Group));

        Assert.Equal(SchoolSimilarity.NotSimilar, result.Ks2AverageScore.Similarity);
        _repo.Verify(r => r.GetSimilarSchoolUrnsAsync(currentUrn), Times.Once);
        _repo.Verify(r => r.GetSimilarSchoolsSecondaryStandardDeviationsAsync(), Times.Never);
    }

    [Fact]
    public async Task Execute_ReturnsNotSimilar_WhenGroupSdIsZero()
    {
        var currentUrn = "100";
        var similarUrn = "200";

        var current = BuildValues(currentUrn, ks2Avg: 100m);
        var similar = BuildValues(similarUrn, ks2Avg: 120m);

        var groupUrns = new[] { "300", "400", "500" };
        var groupValues = new[]
        {
            BuildValues("300", ks2Avg: 110m),
            BuildValues("400", ks2Avg: 110m),
            BuildValues("500", ks2Avg: 110m)
        };

        _repo.Setup(r => r.GetSecondaryValuesByUrnsAsync(It.Is<IEnumerable<string>>(u =>
                u.Contains(currentUrn) && u.Contains(similarUrn) && u.Count() == 2)))
            .ReturnsAsync(new[] { current, similar });
        _repo.Setup(r => r.GetSimilarSchoolUrnsAsync(currentUrn))
            .ReturnsAsync(groupUrns);
        _repo.Setup(r => r.GetSecondaryValuesByUrnsAsync(It.Is<IEnumerable<string>>(u =>
                u.SequenceEqual(groupUrns))))
            .ReturnsAsync(groupValues);

        var sut = CreateSut();

        var result = await sut.Execute(new GetCharacteristicsComparisonRequest(
            currentUrn,
            similarUrn,
            SimilarityCalculationMethod.Group));

        Assert.Equal(SchoolSimilarity.NotSimilar, result.Ks2AverageScore.Similarity);
    }

    private static SimilarSchoolsSecondaryValues BuildValues(
        string urn,
        decimal ks2Avg,
        decimal pp = 0m,
        decimal eal = 0m,
        decimal polar4Quintile = 0m,
        decimal stability = 0m,
        decimal idaci = 0m,
        decimal senSupport = 0m,
        decimal pupilCount = 0m,
        decimal ehcp = 0m)
    {
        return new SimilarSchoolsSecondaryValues
        {
            Urn = urn,
            Ks2ReadingScore = ks2Avg,
            Ks2MathsScore = ks2Avg,
            PupilPremiumEligibilityPercentage = pp,
            PupilsWithEalPercentage = eal,
            Polar4Quintile = polar4Quintile,
            PupilStabilityRate = stability,
            AverageIdaciScore = idaci,
            PupilsWithSenSupportPercentage = senSupport,
            PupilCount = pupilCount,
            PupilsWithEhcPlanPercentage = ehcp
        };
    }

    private static SimilarSchoolsSecondaryStandardDeviations BuildStandardDeviations(decimal ks2AvgSd)
    {
        return new SimilarSchoolsSecondaryStandardDeviations
        {
            Ks2AverageScore = ks2AvgSd,
            PupilPremiumEligibilityPercentage = 1m,
            PupilsWithEalPercentage = 1m,
            Polar4Quintile = 1m,
            PupilStabilityRate = 1m,
            AverageIdaciScore = 1m,
            PupilsWithSenSupportPercentage = 1m,
            PupilCount = 1m,
            PupilsWithEhcPlanPercentage = 1m
        };
    }
}
