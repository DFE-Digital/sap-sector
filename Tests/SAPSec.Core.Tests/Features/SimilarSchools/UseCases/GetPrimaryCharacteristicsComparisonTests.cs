using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Data.Dto.SimilarSchools.Primary;
using SAPSec.Test.Common.InMemory;

namespace SAPSec.Core.Tests.Features.SimilarSchools.UseCases;

public class GetPrimaryCharacteristicsComparisonTests
{
    private readonly InMemorySimilarSchoolsPrimaryRepository _repo = new();

    private GetPrimaryCharacteristicsComparison CreateSut() => new(_repo);

    [Fact]
    public async Task Execute_ThrowsNotFound_WhenCurrentSchoolMissing()
    {
        var currentUrn = "100001";
        var similarUrn = "100002";

        _repo.SetupValues(
            BuildValues(similarUrn, ks1Prior: "100"));

        var sut = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(
            () => sut.Execute(new GetPrimaryCharacteristicsComparisonRequest(currentUrn, similarUrn)));
    }

    [Fact]
    public async Task Execute_ThrowsNotFound_WhenSimilarSchoolMissing()
    {
        var currentUrn = "100001";
        var similarUrn = "100002";

        _repo.SetupValues(
            BuildValues(currentUrn, ks1Prior: "100"));

        var sut = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(
            () => sut.Execute(new GetPrimaryCharacteristicsComparisonRequest(currentUrn, similarUrn)));
    }

    [Fact]
    public async Task Execute_RoundsKs1PriorRwmAverage_ToWholeNumber()
    {
        var currentUrn = "100001";
        var similarUrn = "100002";

        _repo.SetupValues(
            BuildValues(currentUrn, ks1Prior: "100.4"),
            BuildValues(similarUrn, ks1Prior: "100.6"));

        var sut = CreateSut();

        var result = await sut.Execute(new GetPrimaryCharacteristicsComparisonRequest(currentUrn, similarUrn));

        Assert.Equal(100m, result.Ks1PriorRwmAverage.CurrentSchoolValue);
        Assert.Equal(101m, result.Ks1PriorRwmAverage.SimilarSchoolValue);
    }

    [Fact]
    public async Task Execute_RoundsPercentages_ToOneDecimalPlace()
    {
        var currentUrn = "100001";
        var similarUrn = "100002";

        _repo.SetupValues(
            BuildValues(currentUrn, ks1Prior: "100", eal: "19.44"),
            BuildValues(similarUrn, ks1Prior: "100", eal: "19.36"));

        var sut = CreateSut();

        var result = await sut.Execute(new GetPrimaryCharacteristicsComparisonRequest(currentUrn, similarUrn));

        Assert.Equal(19.4m, result.PupilsWithEalPercentage.CurrentSchoolValue);
        Assert.Equal(19.4m, result.PupilsWithEalPercentage.SimilarSchoolValue);
    }

    [Fact]
    public async Task Execute_RoundsIdaci_ToThreeDecimalPlaces()
    {
        var currentUrn = "100001";
        var similarUrn = "100002";

        _repo.SetupValues(
            BuildValues(currentUrn, ks1Prior: "100", idaci: "0.1305"),
            BuildValues(similarUrn, ks1Prior: "100", idaci: "0.1314"));

        var sut = CreateSut();

        var result = await sut.Execute(new GetPrimaryCharacteristicsComparisonRequest(currentUrn, similarUrn));

        Assert.Equal(0.131m, result.AverageIdaciScore.CurrentSchoolValue);
        Assert.Equal(0.131m, result.AverageIdaciScore.SimilarSchoolValue);
    }

    [Fact]
    public async Task Execute_RoundsPupilCountAndPolar4Quintile_ToWholeNumbers()
    {
        var currentUrn = "100001";
        var similarUrn = "100002";

        _repo.SetupValues(
            BuildValues(currentUrn, ks1Prior: "100", pupilCount: "100.5", polar4Quintile: "1.4"),
            BuildValues(similarUrn, ks1Prior: "102", pupilCount: "102.5", polar4Quintile: "2.6"));

        var sut = CreateSut();

        var result = await sut.Execute(new GetPrimaryCharacteristicsComparisonRequest(currentUrn, similarUrn));

        Assert.Equal(101, result.PupilCount.CurrentSchoolValue);
        Assert.Equal(103, result.PupilCount.SimilarSchoolValue);

        Assert.Equal(1, result.Polar4Quintile.CurrentSchoolValue);
        Assert.Equal(3, result.Polar4Quintile.SimilarSchoolValue);
    }

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
