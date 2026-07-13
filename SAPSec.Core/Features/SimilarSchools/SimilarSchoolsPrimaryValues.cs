using SAPSec.Data.Dto.SimilarSchools.Primary;
using System.Globalization;

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
            ReadMatAverage = ParseNullableDecimal(data.ReadMatAverage),
            Ks1PriorRwmAverage = ParseNullableDecimal(data.Ks1PriorRwmAverage),
            PupilPremiumEligibilityPercentage = ParseNullableDecimal(data.PPPerc),
            PupilsWithEalPercentage = ParseNullableDecimal(data.PercentEAL),
            Polar4Quintile = ParseNullableDecimal(data.Polar4QuintilePupils),
            PupilStabilityRate = ParseNullableDecimal(data.PStability),
            AverageIdaciScore = ParseNullableDecimal(data.IdaciPupils),
            PupilsWithSenSupportPercentage = ParseNullableDecimal(data.PercentSchSupport),
            PupilCount = ParseNullableDecimal(data.NumberOfPupils),
            PupilsWithEhcPlanPercentage = ParseNullableDecimal(data.PercentageStatementOrEhp)
        };
    }

    public static IEnumerable<SimilarSchoolsPrimaryValues> FromData(IEnumerable<SimilarSchoolsPrimaryValuesEntry> data)
    {
        return data.Select(FromData);
    }

    private static decimal ParseNullableDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        return decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : 0;
    }
}
