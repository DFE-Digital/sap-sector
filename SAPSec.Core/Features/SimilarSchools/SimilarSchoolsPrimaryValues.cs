using SAPSec.Core.Features.Measures;
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
            ReadMatAverage = MeasureHelper.ParseNullableDecimal(data.ReadMatAverage) ?? 0,
            Ks1PriorRwmAverage = MeasureHelper.ParseNullableDecimal(data.Ks1PriorRwmAverage) ?? 0,
            PupilPremiumEligibilityPercentage = MeasureHelper.ParseNullableDecimal(data.PPPerc) ?? 0,
            PupilsWithEalPercentage = MeasureHelper.ParseNullableDecimal(data.PercentEAL) ?? 0,
            Polar4Quintile = MeasureHelper.ParseNullableDecimal(data.Polar4QuintilePupils) ?? 0,
            PupilStabilityRate = MeasureHelper.ParseNullableDecimal(data.PStability) ?? 0,
            AverageIdaciScore = MeasureHelper.ParseNullableDecimal(data.IdaciPupils) ?? 0,
            PupilsWithSenSupportPercentage = MeasureHelper.ParseNullableDecimal(data.PercentSchSupport) ?? 0,
            PupilCount = MeasureHelper.ParseNullableDecimal(data.NumberOfPupils) ?? 0,
            PupilsWithEhcPlanPercentage = MeasureHelper.ParseNullableDecimal(data.PercentageStatementOrEhp) ?? 0
        };
    }

    public static IEnumerable<SimilarSchoolsPrimaryValues> FromData(IEnumerable<SimilarSchoolsPrimaryValuesEntry> data)
    {
        return data.Select(FromData);
    }
}
