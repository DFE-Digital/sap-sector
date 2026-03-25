using SAPSec.Core.Model.Generated;

namespace SAPSec.Core.Features.SimilarSchools;

public class SimilarSchoolsSecondaryStandardDeviations
{
    public decimal? Ks2AverageScore { get; init; }
    public decimal? PupilPremiumEligibilityPercentage { get; init; }
    public decimal? PupilsWithEalPercentage { get; init; }
    public decimal? Polar4Quintile { get; init; }
    public decimal? PupilStabilityRate { get; init; }
    public decimal? AverageIdaciScore { get; init; }
    public decimal? PupilsWithSenSupportPercentage { get; init; }
    public decimal? PupilCount { get; init; }
    public decimal? PupilsWithEhcPlanPercentage { get; init; }

    public static SimilarSchoolsSecondaryStandardDeviations FromData(SimilarSchoolsSecondaryStandardDeviationsEntry data)
    {
        return new()
        {
            Ks2AverageScore = data.KS2AVG,
            PupilPremiumEligibilityPercentage = data.PPPerc,
            PupilsWithEalPercentage = data.PercentEAL,
            Polar4Quintile = data.Polar4QuintilePupils,
            PupilStabilityRate = data.PStability,
            AverageIdaciScore = data.IdaciPupils,
            PupilsWithSenSupportPercentage = data.PercentSchSupport,
            PupilCount = data.NumberOfPupils,
            PupilsWithEhcPlanPercentage = data.PercentageStatementOrEHP,
        };
    }
}
