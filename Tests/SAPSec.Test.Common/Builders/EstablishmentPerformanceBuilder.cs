using SAPSec.Data.Dto.KS2.Performance;

namespace SAPSec.Test.Common.Builders;

public class EstablishmentPerformanceBuilder(string urn)
{
    string RwmExpected_Tot_Cohort_Est_Current_Num = string.Empty;
    string RwmExpected_Tot_Cohort_Est_Previous_Num = string.Empty;
    string RwmExpected_Tot_Cohort_Est_Previous2_Num = string.Empty;
    string ReadingScaledScore_Tot_Cohort_Est_Current_Num = string.Empty;
    string ReadingScaledScore_Tot_Cohort_Est_Previous_Num = string.Empty;
    string ReadingScaledScore_Tot_Cohort_Est_Previous2_Num = string.Empty;

    public EstablishmentPerformanceBuilder WithRwmExpected(string current, string prev, string prev2)
    {
        RwmExpected_Tot_Cohort_Est_Current_Num = current;
        RwmExpected_Tot_Cohort_Est_Previous_Num = prev;
        RwmExpected_Tot_Cohort_Est_Previous2_Num = prev2;

        return this;
    }

    public EstablishmentPerformanceBuilder WithReadingScore(string current, string prev, string prev2)
    {
        ReadingScaledScore_Tot_Cohort_Est_Current_Num = current;
        ReadingScaledScore_Tot_Cohort_Est_Previous_Num = prev;
        ReadingScaledScore_Tot_Cohort_Est_Previous2_Num = prev2;

        return this;
    }

    public EstablishmentPerformance Build() =>
        new EstablishmentPerformance()
        {
            Id = urn,
            RwmExpected_Tot_Cohort_Est_Current_Num = RwmExpected_Tot_Cohort_Est_Current_Num,
            RwmExpected_Tot_Cohort_Est_Previous_Num = RwmExpected_Tot_Cohort_Est_Previous_Num,
            RwmExpected_Tot_Cohort_Est_Previous2_Num = RwmExpected_Tot_Cohort_Est_Previous2_Num,
            ReadingScaledScore_Tot_Cohort_Est_Current_Num = ReadingScaledScore_Tot_Cohort_Est_Current_Num,
            ReadingScaledScore_Tot_Cohort_Est_Previous_Num = ReadingScaledScore_Tot_Cohort_Est_Previous_Num,
            ReadingScaledScore_Tot_Cohort_Est_Previous2_Num = ReadingScaledScore_Tot_Cohort_Est_Previous2_Num,
        };
}
