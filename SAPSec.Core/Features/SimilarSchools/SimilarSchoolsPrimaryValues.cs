using SAPSec.Data.Dto.SimilarSchools.Primary;

namespace SAPSec.Core.Features.SimilarSchools;

public class SimilarSchoolsPrimaryValues
{
    public string Urn { get; init; } = default!;
    public decimal ReadMatAverage { get; init; }
    public decimal Ks1PriorRwmAverage { get; init; }
    public decimal PupilPremiumEligibilityPercentage { get; init; }
    public decimal PupilsWithEalPercentage { get; init; }
    public decimal Polar4Quintile { get; init; }
    public decimal PupilStabilityRate { get; init; }
    public decimal AverageIdaciScore { get; init; }
    public decimal PupilsWithSenSupportPercentage { get; init; }
    public decimal PupilCount { get; init; }
    public decimal PupilsWithEhcPlanPercentage { get; init; }

    public static SimilarSchoolsPrimaryValues FromData(SimilarSchoolsPrimaryValuesEntry data)
    {
        return new()
        {
            Urn = data.URN,
            ReadMatAverage = SimilarSchoolsDecimalParsing.ParseNullableDecimal(data.ReadMatAverage),
            Ks1PriorRwmAverage = SimilarSchoolsDecimalParsing.ParseNullableDecimal(data.Ks1PriorRwmAverage),
            PupilPremiumEligibilityPercentage = SimilarSchoolsDecimalParsing.ParseNullableDecimal(data.PPPerc),
            PupilsWithEalPercentage = SimilarSchoolsDecimalParsing.ParseNullableDecimal(data.PercentEAL),
            Polar4Quintile = SimilarSchoolsDecimalParsing.ParseNullableDecimal(data.Polar4QuintilePupils),
            PupilStabilityRate = SimilarSchoolsDecimalParsing.ParseNullableDecimal(data.PStability),
            AverageIdaciScore = SimilarSchoolsDecimalParsing.ParseNullableDecimal(data.IdaciPupils),
            PupilsWithSenSupportPercentage = SimilarSchoolsDecimalParsing.ParseNullableDecimal(data.PercentSchSupport),
            PupilCount = SimilarSchoolsDecimalParsing.ParseNullableDecimal(data.NumberOfPupils),
            PupilsWithEhcPlanPercentage = SimilarSchoolsDecimalParsing.ParseNullableDecimal(data.PercentageStatementOrEhp)
        };
    }

    public static IEnumerable<SimilarSchoolsPrimaryValues> FromData(IEnumerable<SimilarSchoolsPrimaryValuesEntry> data)
    {
        return data.Select(FromData);
    }
}
