using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Test.Common.Builders;
using SAPSec.Test.Common.InMemory;

namespace SAPSec.Core.Tests.Features.SimilarSchools;

public class GetPrimaryComparisonSimilarityCharacteristicsUseCaseTests
{
    private readonly InMemoryEstablishmentRepository _establishmentRepo = new();
    private readonly InMemorySimilarSchoolsPrimaryRepository _similarSchoolsRepo = new();

    private GetPrimaryComparisonSimilarityCharacteristicsUseCase CreateSut() => new(_establishmentRepo, _similarSchoolsRepo);

    [Fact]
    public async Task Execute_WhenCurrentSchoolMissing_ThrowsNotFound()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100002", "Comparator School", x => x.Open().Primary()));

        _similarSchoolsRepo
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]))
            .SetupValues(Build.PrimaryValues("100002", ks1Prior: "100"));

        var sut = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(
            () => sut.Execute(new GetPrimaryComparisonSimilarityCharacteristicsRequest("100001", "100002")));
    }

    [Fact]
    public async Task Execute_WhenComparatorSchoolMissing_ThrowsNotFound()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Primary()));

        _similarSchoolsRepo
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]))
            .SetupValues(Build.PrimaryValues("100001", ks1Prior: "100"));

        var sut = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(
            () => sut.Execute(new GetPrimaryComparisonSimilarityCharacteristicsRequest("100001", "100002")));
    }

    [Fact]
    public async Task Execute_WhenComparatorSchoolIsNotInSimilarSchoolsGroupForCurrentSchool_ThrowsNotFound()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Primary()));

        _similarSchoolsRepo
            .SetupGroups(Build.PrimaryGroup("100001", []))
            .SetupValues(Build.PrimaryValues(["100001", "100002"]));

        var sut = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(
            () => sut.Execute(new GetPrimaryComparisonSimilarityCharacteristicsRequest("100001", "100002")));
    }

    [Fact]
    public async Task Execute_WhenSimilarSchoolValuesDoNotExistForCurrentSchool_ThrowsNotFound()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Primary()));

        _similarSchoolsRepo
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]))
            .SetupValues(Build.PrimaryValues(["100002"]));

        var sut = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(
            () => sut.Execute(new GetPrimaryComparisonSimilarityCharacteristicsRequest("100001", "100002")));
    }

    [Fact]
    public async Task Execute_WhenSimilarSchoolValuesDoNotExistForComparatorSchool_ThrowsNotFound()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Primary()));

        _similarSchoolsRepo
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]))
            .SetupValues(Build.PrimaryValues(["100001"]));

        var sut = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(
            () => sut.Execute(new GetPrimaryComparisonSimilarityCharacteristicsRequest("100001", "100002")));
    }

    [Fact]
    public async Task Execute_RoundsKs1PriorRwmAverage_ToWholeNumber()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Primary()));

        _similarSchoolsRepo
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]))
            .SetupValues(
                Build.PrimaryValues("100001", ks1Prior: "100.4"),
                Build.PrimaryValues("100002", ks1Prior: "100.6"));

        var sut = CreateSut();

        var result = await sut.Execute(new GetPrimaryComparisonSimilarityCharacteristicsRequest("100001", "100002"));

        Assert.Equal(100m, result.SimilarityCharacteristics.Ks1PriorRwmAverage.CurrentSchoolValue);
        Assert.Equal(101m, result.SimilarityCharacteristics.Ks1PriorRwmAverage.ComparatorSchoolValue);
    }

    [Fact]
    public async Task Execute_RoundsPercentages_ToOneDecimalPlace()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Primary()));

        _similarSchoolsRepo
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]))
            .SetupValues(
                Build.PrimaryValues("100001", ks1Prior: "100", eal: "19.44"),
                Build.PrimaryValues("100002", ks1Prior: "100", eal: "19.36"));

        var sut = CreateSut();

        var result = await sut.Execute(new GetPrimaryComparisonSimilarityCharacteristicsRequest("100001", "100002"));

        Assert.Equal(19.4m, result.SimilarityCharacteristics.PupilsWithEalPercentage.CurrentSchoolValue);
        Assert.Equal(19.4m, result.SimilarityCharacteristics.PupilsWithEalPercentage.ComparatorSchoolValue);
    }

    [Fact]
    public async Task Execute_RoundsIdaci_ToThreeDecimalPlaces()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Primary()));

        _similarSchoolsRepo
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]))
            .SetupValues(
                Build.PrimaryValues("100001", ks1Prior: "100", idaci: "0.1305"),
                Build.PrimaryValues("100002", ks1Prior: "100", idaci: "0.1314"));

        var sut = CreateSut();

        var result = await sut.Execute(new GetPrimaryComparisonSimilarityCharacteristicsRequest("100001", "100002"));

        Assert.Equal(0.131m, result.SimilarityCharacteristics.AverageIdaciScore.CurrentSchoolValue);
        Assert.Equal(0.131m, result.SimilarityCharacteristics.AverageIdaciScore.ComparatorSchoolValue);
    }

    [Fact]
    public async Task Execute_RoundsPupilCountAndPolar4Quintile_ToWholeNumbers()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Primary()));

        _similarSchoolsRepo
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]))
            .SetupValues(
                Build.PrimaryValues("100001", ks1Prior: "100", pupilCount: "100.5", polar4Quintile: "1.4"),
                Build.PrimaryValues("100002", ks1Prior: "102", pupilCount: "102.5", polar4Quintile: "2.6"));

        var sut = CreateSut();

        var result = await sut.Execute(new GetPrimaryComparisonSimilarityCharacteristicsRequest("100001", "100002"));

        Assert.Equal(101, result.SimilarityCharacteristics.PupilCount.CurrentSchoolValue);
        Assert.Equal(103, result.SimilarityCharacteristics.PupilCount.ComparatorSchoolValue);

        Assert.Equal(1, result.SimilarityCharacteristics.Polar4Quintile.CurrentSchoolValue);
        Assert.Equal(3, result.SimilarityCharacteristics.Polar4Quintile.ComparatorSchoolValue);
    }

    [Fact]
    public async Task Execute_RoundsRemainingPercentageProperties_ToOneDecimalPlace()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Primary()));

        _similarSchoolsRepo
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]))
            .SetupValues(
                Build.PrimaryValues("100001", ks1Prior: "100", pp: "22.24", stability: "83.16", senSupport: "12.84", ehcp: "3.14"),
                Build.PrimaryValues("100002", ks1Prior: "100", pp: "28.76", stability: "76.94", senSupport: "14.66", ehcp: "5.05"));

        var sut = CreateSut();

        var result = await sut.Execute(new GetPrimaryComparisonSimilarityCharacteristicsRequest("100001", "100002"));

        Assert.Equal(22.2m, result.SimilarityCharacteristics.PupilPremiumEligibilityPercentage.CurrentSchoolValue);
        Assert.Equal(28.8m, result.SimilarityCharacteristics.PupilPremiumEligibilityPercentage.ComparatorSchoolValue);

        Assert.Equal(83.2m, result.SimilarityCharacteristics.PupilStabilityRate.CurrentSchoolValue);
        Assert.Equal(76.9m, result.SimilarityCharacteristics.PupilStabilityRate.ComparatorSchoolValue);

        Assert.Equal(12.8m, result.SimilarityCharacteristics.PupilsWithSenSupportPercentage.CurrentSchoolValue);
        Assert.Equal(14.7m, result.SimilarityCharacteristics.PupilsWithSenSupportPercentage.ComparatorSchoolValue);

        Assert.Equal(3.1m, result.SimilarityCharacteristics.PupilsWithEhcPlanPercentage.CurrentSchoolValue);
        Assert.Equal(5.1m, result.SimilarityCharacteristics.PupilsWithEhcPlanPercentage.ComparatorSchoolValue);
    }

    [Fact]
    public async Task Execute_WhenSourceValuesEmpty_DefaultsAllPropertiesToZero()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Primary()));

        _similarSchoolsRepo
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]))
            .SetupValues(
                Build.PrimaryValues("100001", ""),
                Build.PrimaryValues("100002", ""));

        var sut = CreateSut();

        var result = await sut.Execute(new GetPrimaryComparisonSimilarityCharacteristicsRequest("100001", "100002"));

        AssertAllPropertiesAreZero(result);
    }

    [Fact]
    public async Task Execute_WhenSourceValuesInvalid_DefaultsAllPropertiesToZero()
    {
        _establishmentRepo.SetupEstablishments(
            Build.Establishment("100001", "Current School", x => x.Open().Primary()),
            Build.Establishment("100002", "Comparator School", x => x.Open().Primary()));

        _similarSchoolsRepo
            .SetupGroups(Build.PrimaryGroup("100001", ["100002"]))
            .SetupValues(
                Build.PrimaryValues("100001", "not-a-number"),
                Build.PrimaryValues("100002", "not-a-number"));

        var sut = CreateSut();

        var result = await sut.Execute(new GetPrimaryComparisonSimilarityCharacteristicsRequest("100001", "100002"));

        AssertAllPropertiesAreZero(result);
    }

    private static void AssertAllPropertiesAreZero(GetPrimaryComparisonSimilarityCharacteristicsResponse result)
    {
        Assert.Equal(0m, result.SimilarityCharacteristics.Ks1PriorRwmAverage.CurrentSchoolValue);
        Assert.Equal(0m, result.SimilarityCharacteristics.Ks1PriorRwmAverage.ComparatorSchoolValue);

        Assert.Equal(0m, result.SimilarityCharacteristics.PupilPremiumEligibilityPercentage.CurrentSchoolValue);
        Assert.Equal(0m, result.SimilarityCharacteristics.PupilPremiumEligibilityPercentage.ComparatorSchoolValue);

        Assert.Equal(0m, result.SimilarityCharacteristics.PupilsWithEalPercentage.CurrentSchoolValue);
        Assert.Equal(0m, result.SimilarityCharacteristics.PupilsWithEalPercentage.ComparatorSchoolValue);

        Assert.Equal(0, result.SimilarityCharacteristics.Polar4Quintile.CurrentSchoolValue);
        Assert.Equal(0, result.SimilarityCharacteristics.Polar4Quintile.ComparatorSchoolValue);

        Assert.Equal(0, result.SimilarityCharacteristics.PupilCount.CurrentSchoolValue);
        Assert.Equal(0, result.SimilarityCharacteristics.PupilCount.ComparatorSchoolValue);

        Assert.Equal(0m, result.SimilarityCharacteristics.PupilStabilityRate.CurrentSchoolValue);
        Assert.Equal(0m, result.SimilarityCharacteristics.PupilStabilityRate.ComparatorSchoolValue);

        Assert.Equal(0m, result.SimilarityCharacteristics.AverageIdaciScore.CurrentSchoolValue);
        Assert.Equal(0m, result.SimilarityCharacteristics.AverageIdaciScore.ComparatorSchoolValue);

        Assert.Equal(0m, result.SimilarityCharacteristics.PupilsWithSenSupportPercentage.CurrentSchoolValue);
        Assert.Equal(0m, result.SimilarityCharacteristics.PupilsWithSenSupportPercentage.ComparatorSchoolValue);

        Assert.Equal(0m, result.SimilarityCharacteristics.PupilsWithEhcPlanPercentage.CurrentSchoolValue);
        Assert.Equal(0m, result.SimilarityCharacteristics.PupilsWithEhcPlanPercentage.ComparatorSchoolValue);
    }
}
