using SAPSec.Data.Dto.SimilarSchools.Secondary;
using System.Globalization;

namespace SAPSec.Core.Features.SimilarSchools;

public class SimilarSchoolsSecondaryValues
{
    public string Urn { get; init; } = default!;
    public decimal Ks2AverageScore { get; init; }
    public decimal PupilPremiumEligibilityPercentage { get; init; }
    public decimal PupilsWithEalPercentage { get; init; }
    public decimal Polar4Quintile { get; init; }
    public decimal PupilStabilityRate { get; init; }
    public decimal AverageIdaciScore { get; init; }
    public decimal PupilsWithSenSupportPercentage { get; init; }
    public decimal PupilCount { get; init; }
    public decimal PupilsWithEhcPlanPercentage { get; init; }

    public static SimilarSchoolsSecondaryValues FromData(SimilarSchoolsSecondaryValuesEntry data)
    {
        return new()
        {
            Urn = data.URN,
            Ks2AverageScore = ParseNullableDecimal(data.KS2MRP),
            PupilPremiumEligibilityPercentage = ParseNullableDecimal(data.PPPerc),
            PupilsWithEalPercentage = ParseNullableDecimal(data.PercentEAL),
            Polar4Quintile = ParseNullableDecimal(data.Polar4QuintilePupils),
            PupilStabilityRate = ParseNullableDecimal(data.PStability),
            AverageIdaciScore = ParseNullableDecimal(data.IdaciPupils),
            PupilsWithSenSupportPercentage = ParseNullableDecimal(data.PercentSchSupport),
            PupilCount = ParseNullableDecimal(data.NumberOfPupils),
            PupilsWithEhcPlanPercentage = ParseNullableDecimal(data.PercentageStatementOrEHP),
        };
    }

    public static IEnumerable<SimilarSchoolsSecondaryValues> FromData(IEnumerable<SimilarSchoolsSecondaryValuesEntry> data)
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
