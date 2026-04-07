using Moq;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Model.Generated;

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

        var current = BuildValues(currentUrn, ks2Avg: "100");
        var similar = BuildValues(similarUrn, ks2Avg: (100m + diff).ToString());

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

        var current = BuildValues(currentUrn, ks2Avg: "100");
        var similar = BuildValues(similarUrn, ks2Avg: "120");

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

        var current = BuildValues(currentUrn, ks2Avg: "100");
        var similar = BuildValues(similarUrn, ks2Avg: "120");

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

        var similar = BuildValues(similarUrn, ks2Avg: "120");

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

        var current = BuildValues(currentUrn, ks2Avg: "100");

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

        var current = BuildValues(currentUrn, ks2Avg: "100", pupilCount: "100.5", polar4Quintile: "1.4");
        var similar = BuildValues(similarUrn, ks2Avg: "102", pupilCount: "102.5", polar4Quintile: "2.6");

        _repo.Setup(r => r.GetSecondaryValuesByUrnsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new[] { current, similar });
        _repo.Setup(r => r.GetSimilarSchoolsSecondaryStandardDeviationsAsync())
            .ReturnsAsync(new SimilarSchoolsSecondaryStandardDeviationsEntry
            {
                KS2AVG = 10m,
                PPPerc = 10m,
                PercentEAL = 10m,
                Polar4QuintilePupils = 1m,
                PStability = 10m,
                IdaciPupils = 10m,
                PercentSchSupport = 10m,
                NumberOfPupils = 2m,
                PercentageStatementOrEHP = 10m
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
            ks2Avg: "100",
            pp: "50",
            eal: "20",
            polar4Quintile: "2",
            stability: "90",
            idaci: "0.3",
            senSupport: "10",
            pupilCount: "600",
            ehcp: "2");

        var similar = BuildValues(
            similarUrn,
            ks2Avg: "102",          // diff 2 / sd10 => Similar
            pp: "56",               // diff 6 / sd10 => LessSimilar
            eal: "30",              // diff 10 / sd10 => NotSimilar
            polar4Quintile: "3",    // diff 1 / sd1 => NotSimilar
            stability: "91",        // diff 1 / sd10 => Similar
            idaci: "0.8",           // diff 0.5 / sd1 => LessSimilar
            senSupport: "15",       // diff 5 / sd10 => LessSimilar
            pupilCount: "603",      // diff 3 / sd10 => Similar
            ehcp: "6");             // diff 4 / sd10 => LessSimilar

        _repo.Setup(r => r.GetSecondaryValuesByUrnsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new[] { current, similar });
        _repo.Setup(r => r.GetSimilarSchoolsSecondaryStandardDeviationsAsync())
            .ReturnsAsync(new SimilarSchoolsSecondaryStandardDeviationsEntry
            {
                KS2AVG = 10m,
                PPPerc = 10m,
                PercentEAL = 10m,
                Polar4QuintilePupils = 1m,
                PStability = 10m,
                IdaciPupils = 1m,
                PercentSchSupport = 10m,
                NumberOfPupils = 10m,
                PercentageStatementOrEHP = 10m
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

        var current = BuildValues(currentUrn, ks2Avg: "100");
        var similar = BuildValues(similarUrn, ks2Avg: "106");

        _repo.Setup(r => r.GetSecondaryValuesByUrnsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new[] { current, similar });
        _repo.Setup(r => r.GetSimilarSchoolsSecondaryStandardDeviationsAsync())
            .ReturnsAsync(BuildStandardDeviations(ks2AvgSd: 20m));

        var sut = CreateSut();

        var result = await sut.Execute(new GetCharacteristicsComparisonRequest(currentUrn, similarUrn));

        Assert.Equal(SchoolSimilarity.Similar, result.Ks2AverageScore.Similarity);
        _repo.Verify(r => r.GetSimilarSchoolsSecondaryStandardDeviationsAsync(), Times.Once);
        _repo.Verify(r => r.GetSimilarSchoolsGroupAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Execute_UsesNationalSd_WhenExplicitlyRequested()
    {
        var currentUrn = "100";
        var similarUrn = "200";

        var current = BuildValues(currentUrn, ks2Avg: "100");
        var similar = BuildValues(similarUrn, ks2Avg: "106");

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
        _repo.Verify(r => r.GetSimilarSchoolsGroupAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Execute_UsesGroupSd_WhenRequested()
    {
        var currentUrn = "100";
        var similarUrn = "200";

        var current = BuildValues(currentUrn, ks2Avg: "100");
        var similar = BuildValues(similarUrn, ks2Avg: "106");

        var groupUrns = new[] { "200", "300", "400", "500", "600" };
        var groupValues = new[]
        {
            BuildValues("200", ks2Avg: "100"),
            BuildValues("300", ks2Avg: "102"),
            BuildValues("400", ks2Avg: "104"),
            BuildValues("500", ks2Avg: "106"),
            BuildValues("600", ks2Avg: "108")
        };

        _repo.Setup(r => r.GetSecondaryValuesByUrnsAsync(It.Is<IEnumerable<string>>(u =>
                u.Contains(currentUrn) && u.Contains(similarUrn) && u.Count() == 2)))
            .ReturnsAsync(new[] { current, similar });
        _repo.Setup(r => r.GetSimilarSchoolsGroupAsync(currentUrn))
            .ReturnsAsync(groupUrns.Select(urn => new SimilarSchoolsSecondaryGroupsEntry { URN = currentUrn, NeighbourURN = urn }).ToList());
        _repo.Setup(r => r.GetSecondaryValuesByUrnsAsync(It.Is<IEnumerable<string>>(u =>
                u.SequenceEqual(groupUrns))))
            .ReturnsAsync(groupValues);

        var sut = CreateSut();

        var result = await sut.Execute(new GetCharacteristicsComparisonRequest(
            currentUrn,
            similarUrn,
            SimilarityCalculationMethod.Group));

        Assert.Equal(SchoolSimilarity.NotSimilar, result.Ks2AverageScore.Similarity);
        _repo.Verify(r => r.GetSimilarSchoolsGroupAsync(currentUrn), Times.Once);
        _repo.Verify(r => r.GetSimilarSchoolsSecondaryStandardDeviationsAsync(), Times.Never);
    }

    [Fact]
    public async Task Execute_ReturnsNotSimilar_WhenGroupSdIsZero()
    {
        var currentUrn = "100";
        var similarUrn = "200";

        var current = BuildValues(currentUrn, ks2Avg: "100");
        var similar = BuildValues(similarUrn, ks2Avg: "120");

        var groupUrns = new[] { "300", "400", "500" };
        var groupValues = new[]
        {
            BuildValues("300", ks2Avg: "110"),
            BuildValues("400", ks2Avg: "110"),
            BuildValues("500", ks2Avg: "110")
        };

        _repo.Setup(r => r.GetSecondaryValuesByUrnsAsync(It.Is<IEnumerable<string>>(u =>
                u.Contains(currentUrn) && u.Contains(similarUrn) && u.Count() == 2)))
            .ReturnsAsync(new[] { current, similar });
        _repo.Setup(r => r.GetSimilarSchoolsGroupAsync(currentUrn))
            .ReturnsAsync(groupUrns.Select(urn => new SimilarSchoolsSecondaryGroupsEntry { URN = currentUrn, NeighbourURN = urn }).ToList());
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

    private static SimilarSchoolsSecondaryValuesEntry BuildValues(
        string urn,
        string ks2Avg,
        string pp = "0",
        string eal = "0",
        string polar4Quintile = "0",
        string stability = "0",
        string idaci = "0",
        string senSupport = "0",
        string pupilCount = "0",
        string ehcp = "0")
    {
        return new SimilarSchoolsSecondaryValuesEntry
        {
            URN = urn,
            KS2RP = ks2Avg,
            KS2MP = ks2Avg,
            PPPerc = pp,
            PercentEAL = eal,
            Polar4QuintilePupils = polar4Quintile,
            PStability = stability,
            IdaciPupils = idaci,
            PercentSchSupport = senSupport,
            NumberOfPupils = pupilCount,
            PercentageStatementOrEHP = ehcp
        };
    }

    private static SimilarSchoolsSecondaryStandardDeviationsEntry BuildStandardDeviations(decimal ks2AvgSd)
    {
        return new SimilarSchoolsSecondaryStandardDeviationsEntry
        {
            KS2RP = ks2AvgSd,
            KS2MP = ks2AvgSd,
            KS2AVG = ks2AvgSd,
            PPPerc = 1m,
            PercentEAL = 1m,
            Polar4QuintilePupils = 1m,
            PStability = 1m,
            IdaciPupils = 1m,
            PercentSchSupport = 1m,
            NumberOfPupils = 1m,
            PercentageStatementOrEHP = 1m
        };
    }
}
