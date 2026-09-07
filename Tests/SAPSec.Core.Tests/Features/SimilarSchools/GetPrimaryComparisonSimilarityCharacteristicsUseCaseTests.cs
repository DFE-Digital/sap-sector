using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Data.Dto.SimilarSchools.Primary;
using SAPSec.Test.Common.InMemory;

namespace SAPSec.Core.Tests.Features.SimilarSchools;

public class GetPrimaryComparisonSimilarityCharacteristicsUseCaseTests
{
    private readonly InMemoryEstablishmentRepository _establishmentRepo = new();
    private readonly InMemorySimilarSchoolsPrimaryRepository _simiarlSchoolsRepo = new();

    private GetPrimaryComparisonSimilarityCharacteristicsUseCase CreateSut() => new(_establishmentRepo, _simiarlSchoolsRepo);

    [Fact]
    public async Task Execute_ThrowsNotFound_WhenCurrentSchoolMissing()
    {
        var currentUrn = "100001";
        var similarUrn = "100002";

        _simiarlSchoolsRepo.SetupValues(
            BuildValues(similarUrn, ks1Prior: "100"));

        var sut = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(
            () => sut.Execute(new GetPrimaryComparisonSimilarityCharacteristicsRequest(currentUrn, similarUrn)));
    }

    [Fact]
    public async Task Execute_ThrowsNotFound_WhenSimilarSchoolMissing()
    {
        var currentUrn = "100001";
        var similarUrn = "100002";

        _simiarlSchoolsRepo.SetupValues(
            BuildValues(currentUrn, ks1Prior: "100"));

        var sut = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(
            () => sut.Execute(new GetPrimaryComparisonSimilarityCharacteristicsRequest(currentUrn, similarUrn)));
    }

    [Fact]
    public async Task Execute_RoundsKs1PriorRwmAverage_ToWholeNumber()
    {
        var currentUrn = "100001";
        var similarUrn = "100002";

        _simiarlSchoolsRepo.SetupValues(
            BuildValues(currentUrn, ks1Prior: "100.4"),
            BuildValues(similarUrn, ks1Prior: "100.6"));

        var sut = CreateSut();

        var result = await sut.Execute(new GetPrimaryComparisonSimilarityCharacteristicsRequest(currentUrn, similarUrn));

        Assert.Equal(100m, result.SimilarityCharacteristics.Ks1PriorRwmAverage.CurrentSchoolValue);
        Assert.Equal(101m, result.SimilarityCharacteristics.Ks1PriorRwmAverage.ComparatorSchoolValue);
    }

    [Fact]
    public async Task Execute_RoundsPercentages_ToOneDecimalPlace()
    {
        var currentUrn = "100001";
        var similarUrn = "100002";

        _simiarlSchoolsRepo.SetupValues(
            BuildValues(currentUrn, ks1Prior: "100", eal: "19.44"),
            BuildValues(similarUrn, ks1Prior: "100", eal: "19.36"));

        var sut = CreateSut();

        var result = await sut.Execute(new GetPrimaryComparisonSimilarityCharacteristicsRequest(currentUrn, similarUrn));

        Assert.Equal(19.4m, result.SimilarityCharacteristics.PupilsWithEalPercentage.CurrentSchoolValue);
        Assert.Equal(19.4m, result.SimilarityCharacteristics.PupilsWithEalPercentage.ComparatorSchoolValue);
    }

    [Fact]
    public async Task Execute_RoundsIdaci_ToThreeDecimalPlaces()
    {
        var currentUrn = "100001";
        var similarUrn = "100002";

        _simiarlSchoolsRepo.SetupValues(
            BuildValues(currentUrn, ks1Prior: "100", idaci: "0.1305"),
            BuildValues(similarUrn, ks1Prior: "100", idaci: "0.1314"));

        var sut = CreateSut();

        var result = await sut.Execute(new GetPrimaryComparisonSimilarityCharacteristicsRequest(currentUrn, similarUrn));

        Assert.Equal(0.131m, result.SimilarityCharacteristics.AverageIdaciScore.CurrentSchoolValue);
        Assert.Equal(0.131m, result.SimilarityCharacteristics.AverageIdaciScore.ComparatorSchoolValue);
    }

    [Fact]
    public async Task Execute_RoundsPupilCountAndPolar4Quintile_ToWholeNumbers()
    {
        var currentUrn = "100001";
        var similarUrn = "100002";

        _simiarlSchoolsRepo.SetupValues(
            BuildValues(currentUrn, ks1Prior: "100", pupilCount: "100.5", polar4Quintile: "1.4"),
            BuildValues(similarUrn, ks1Prior: "102", pupilCount: "102.5", polar4Quintile: "2.6"));

        var sut = CreateSut();

        var result = await sut.Execute(new GetPrimaryComparisonSimilarityCharacteristicsRequest(currentUrn, similarUrn));

        Assert.Equal(101, result.SimilarityCharacteristics.PupilCount.CurrentSchoolValue);
        Assert.Equal(103, result.SimilarityCharacteristics.PupilCount.ComparatorSchoolValue);

        Assert.Equal(1, result.SimilarityCharacteristics.Polar4Quintile.CurrentSchoolValue);
        Assert.Equal(3, result.SimilarityCharacteristics.Polar4Quintile.ComparatorSchoolValue);
    }

    [Fact]
    public async Task Execute_RoundsRemainingPercentageProperties_ToOneDecimalPlace()
    {
        var currentUrn = "100001";
        var similarUrn = "100002";

        _simiarlSchoolsRepo.SetupValues(
            BuildValues(currentUrn, ks1Prior: "100", pp: "22.24", stability: "83.16", senSupport: "12.84", ehcp: "3.14"),
            BuildValues(similarUrn, ks1Prior: "100", pp: "28.76", stability: "76.94", senSupport: "14.66", ehcp: "5.05"));

        var sut = CreateSut();

        var result = await sut.Execute(new GetPrimaryComparisonSimilarityCharacteristicsRequest(currentUrn, similarUrn));

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
        var currentUrn = "100001";
        var similarUrn = "100002";

        _simiarlSchoolsRepo.SetupValues(
            BuildEmptyOrInvalidValues(currentUrn, ""),
            BuildEmptyOrInvalidValues(similarUrn, ""));

        var sut = CreateSut();

        var result = await sut.Execute(new GetPrimaryComparisonSimilarityCharacteristicsRequest(currentUrn, similarUrn));

        AssertAllPropertiesAreZero(result);
    }

    [Fact]
    public async Task Execute_WhenSourceValuesInvalid_DefaultsAllPropertiesToZero()
    {
        var currentUrn = "100001";
        var similarUrn = "100002";

        _simiarlSchoolsRepo.SetupValues(
            BuildEmptyOrInvalidValues(currentUrn, "not-a-number"),
            BuildEmptyOrInvalidValues(similarUrn, "not-a-number"));

        var sut = CreateSut();

        var result = await sut.Execute(new GetPrimaryComparisonSimilarityCharacteristicsRequest(currentUrn, similarUrn));

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

    private static SimilarSchoolsPrimaryValuesEntry BuildEmptyOrInvalidValues(string urn, string value) =>
        new()
        {
            URN = urn,
            Ks1PriorRwmAverage = value,
            PPPerc = value,
            PercentEAL = value,
            Polar4QuintilePupils = value,
            PStability = value,
            IdaciPupils = value,
            PercentSchSupport = value,
            NumberOfPupils = value,
            PercentageStatementOrEhp = value
        };

    private static SimilarSchoolsPrimaryValuesEntry BuildValues(
        string urn,
        string ks1Prior,
        string pp = "0",
        string eal = "0",
        string polar4Quintile = "0",
        string stability = "0",
        string idaci = "0",
        string senSupport = "0",
        string pupilCount = "0",
        string ehcp = "0")
    {
        return new SimilarSchoolsPrimaryValuesEntry
        {
            URN = urn,
            Ks1PriorRwmAverage = ks1Prior,
            PPPerc = pp,
            PercentEAL = eal,
            Polar4QuintilePupils = polar4Quintile,
            PStability = stability,
            IdaciPupils = idaci,
            PercentSchSupport = senSupport,
            NumberOfPupils = pupilCount,
            PercentageStatementOrEhp = ehcp
        };
    }
}
