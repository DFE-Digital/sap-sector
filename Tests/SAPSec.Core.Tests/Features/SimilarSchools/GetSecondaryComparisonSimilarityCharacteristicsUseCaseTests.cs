using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Common.InMemory;

namespace SAPSec.Core.Tests.Features.SimilarSchools;

public class GetSecondaryComparisonSimilarityCharacteristicsUseCaseTests
{
    private readonly InMemoryEstablishmentRepository _establishmentRepo = new();
    private readonly InMemorySimilarSchoolsSecondaryRepository _similarSchoolsRepo = new();

    private GetSecondaryComparisonSimilarityCharacteristicsUseCase CreateSut() => new(_establishmentRepo, _similarSchoolsRepo);

    [Fact]
    public async Task Execute_WhenCurrentSchoolMissing_ThrowsNotFound()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100002", "Comparator School", x => x.Open().Secondary()));

        _similarSchoolsRepo
            .SetupGroups(Build.SecondaryGroup("100001", ["100002"]))
            .SetupValues(Build.SecondaryValues(["100002"]));

        var sut = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(
            () => sut.Execute(new GetSecondaryComparisonSimilarityCharacteristicsRequest("100001", "100002")));
    }

    [Fact]
    public async Task Execute_WhenComparatorSchoolMissing_ThrowsNotFound()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Secondary()));

        _similarSchoolsRepo
            .SetupGroups(Build.SecondaryGroup("100001", ["100002"]))
            .SetupValues(Build.SecondaryValues(["100001"]));

        var sut = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(
            () => sut.Execute(new GetSecondaryComparisonSimilarityCharacteristicsRequest("100001", "100002")));
    }

    [Fact]
    public async Task Execute_WhenComparatorSchoolIsNotInSimilarSchoolsGroupForCurrentSchool_ThrowsNotFound()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Secondary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Secondary()));

        _similarSchoolsRepo
            .SetupGroups(Build.SecondaryGroup("100001", []))
            .SetupValues(Build.SecondaryValues(["100001", "100002"]));

        var sut = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(
            () => sut.Execute(new GetSecondaryComparisonSimilarityCharacteristicsRequest("100001", "100002")));
    }

    [Fact]
    public async Task Execute_WhenSimilarSchoolValuesDoNotExistForCurrentSchool_ThrowsNotFound()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Secondary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Secondary()));

        _similarSchoolsRepo
            .SetupGroups(Build.SecondaryGroup("100001", ["100002"]))
            .SetupValues(Build.SecondaryValues(["100002"]));

        var sut = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(
            () => sut.Execute(new GetSecondaryComparisonSimilarityCharacteristicsRequest("100001", "100002")));
    }

    [Fact]
    public async Task Execute_WhenSimilarSchoolValuesDoNotExistForComparatorSchool_ThrowsNotFound()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Secondary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Secondary()));

        _similarSchoolsRepo
            .SetupGroups(Build.SecondaryGroup("100001", ["100002"]))
            .SetupValues(Build.SecondaryValues(["100001"]));

        var sut = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(
            () => sut.Execute(new GetSecondaryComparisonSimilarityCharacteristicsRequest("100001", "100002")));
    }

    [Fact]
    public async Task Execute_ReturnsRoundedComparisonValues()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Secondary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Secondary()));

        _similarSchoolsRepo
            .SetupGroups(Build.SecondaryGroup("100001", ["100002"]))
            .SetupValues(
                Build.SecondaryValues(
                    "100001",
                    ks2Avg: "113.5",
                    eal: "19.44",
                    polar4Quintile: "1.4",
                    stability: "90.04",
                    idaci: "0.1305",
                    senSupport: "10.94",
                    pupilCount: "100.5",
                    ehcp: "2.14"),
                Build.SecondaryValues(
                    "100002",
                    ks2Avg: "114.4",
                    eal: "19.36",
                    polar4Quintile: "2.6",
                    stability: "91.66",
                    idaci: "0.1314",
                    senSupport: "11.04",
                    pupilCount: "102.5",
                    ehcp: "3.26"));

        var result = await CreateSut().Execute(new GetSecondaryComparisonSimilarityCharacteristicsRequest("100001", "100002"));

        Assert.Equal(114m, result.SimilarityCharacteristics.Ks2AverageScore.CurrentSchoolValue);
        Assert.Equal(114m, result.SimilarityCharacteristics.Ks2AverageScore.ComparatorSchoolValue);
        Assert.Equal(19.4m, result.SimilarityCharacteristics.PupilsWithEalPercentage.CurrentSchoolValue);
        Assert.Equal(19.4m, result.SimilarityCharacteristics.PupilsWithEalPercentage.ComparatorSchoolValue);
        Assert.Equal(1, result.SimilarityCharacteristics.Polar4Quintile.CurrentSchoolValue);
        Assert.Equal(3, result.SimilarityCharacteristics.Polar4Quintile.ComparatorSchoolValue);
        Assert.Equal(90.0m, result.SimilarityCharacteristics.PupilStabilityRate.CurrentSchoolValue);
        Assert.Equal(91.7m, result.SimilarityCharacteristics.PupilStabilityRate.ComparatorSchoolValue);
        Assert.Equal(0.131m, result.SimilarityCharacteristics.AverageIdaciScore.CurrentSchoolValue);
        Assert.Equal(0.131m, result.SimilarityCharacteristics.AverageIdaciScore.ComparatorSchoolValue);
        Assert.Equal(10.9m, result.SimilarityCharacteristics.PupilsWithSenSupportPercentage.CurrentSchoolValue);
        Assert.Equal(11.0m, result.SimilarityCharacteristics.PupilsWithSenSupportPercentage.ComparatorSchoolValue);
        Assert.Equal(101, result.SimilarityCharacteristics.PupilCount.CurrentSchoolValue);
        Assert.Equal(103, result.SimilarityCharacteristics.PupilCount.ComparatorSchoolValue);
        Assert.Equal(2.1m, result.SimilarityCharacteristics.PupilsWithEhcPlanPercentage.CurrentSchoolValue);
        Assert.Equal(3.3m, result.SimilarityCharacteristics.PupilsWithEhcPlanPercentage.ComparatorSchoolValue);
    }
}
