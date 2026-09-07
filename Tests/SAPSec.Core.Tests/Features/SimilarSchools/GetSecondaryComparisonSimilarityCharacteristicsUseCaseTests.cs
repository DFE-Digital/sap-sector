using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Data.Dto.SimilarSchools.Secondary;
using SAPSec.Test.Common.InMemory;

namespace SAPSec.Core.Tests.Features.SimilarSchools;

public class GetSecondaryComparisonSimilarityCharacteristicsUseCaseTests
{
    private readonly InMemoryEstablishmentRepository _establishmentRepo = new();
    private readonly InMemorySimilarSchoolsSecondaryRepository _similarSchoolsRepo = new();

    private GetSecondaryComparisonSimilarityCharacteristicsUseCase CreateSut() => new(_establishmentRepo, _similarSchoolsRepo);

    [Fact]
    public async Task Execute_ReturnsRoundedComparisonValues()
    {
        const string currentUrn = "100001";
        const string similarUrn = "100002";

        _similarSchoolsRepo.SetupValues(
            BuildValues(
                currentUrn,
                ks2Avg: "113.5",
                eal: "19.44",
                polar4Quintile: "1.4",
                stability: "90.04",
                idaci: "0.1305",
                senSupport: "10.94",
                pupilCount: "100.5",
                ehcp: "2.14"),
            BuildValues(
                similarUrn,
                ks2Avg: "114.4",
                eal: "19.36",
                polar4Quintile: "2.6",
                stability: "91.66",
                idaci: "0.1314",
                senSupport: "11.04",
                pupilCount: "102.5",
                ehcp: "3.26"));

        var result = await CreateSut().Execute(new GetSecondaryComparisonSimilarityCharacteristicsRequest(currentUrn, similarUrn));

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
        Assert.Null(result.SimilarityCharacteristics.Ks2AverageScore.Similarity);
        Assert.Null(result.SimilarityCharacteristics.PupilPremiumEligibilityPercentage.Similarity);
        Assert.Null(result.SimilarityCharacteristics.PupilsWithEalPercentage.Similarity);
    }

    [Fact]
    public async Task Execute_ThrowsNotFound_WhenCurrentSchoolMissing()
    {
        const string currentUrn = "100001";
        const string similarUrn = "100002";

        _similarSchoolsRepo.SetupValues(BuildValues(similarUrn, ks2Avg: "120"));

        await Assert.ThrowsAsync<NotFoundException>(
            () => CreateSut().Execute(new GetSecondaryComparisonSimilarityCharacteristicsRequest(currentUrn, similarUrn)));
    }

    [Fact]
    public async Task Execute_ThrowsNotFound_WhenSimilarSchoolMissing()
    {
        const string currentUrn = "100001";
        const string similarUrn = "100002";

        _similarSchoolsRepo.SetupValues(BuildValues(currentUrn, ks2Avg: "100"));

        await Assert.ThrowsAsync<NotFoundException>(
            () => CreateSut().Execute(new GetSecondaryComparisonSimilarityCharacteristicsRequest(currentUrn, similarUrn)));
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
            KS2MRP = ks2Avg,
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
}
