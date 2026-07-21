using SAPSec.Data.Dto.KS2.Performance;

namespace SAPSec.Test.Common.Builders;

public class EnglandPerformanceBuilder()
{
    string RwmExpected_Tot_Cohort_Eng_Current_Num = string.Empty;
    string RwmExpected_Tot_Cohort_Eng_Previous_Num = string.Empty;
    string RwmExpected_Tot_Cohort_Eng_Previous2_Num = string.Empty;
    string ReadingScaledScore_Tot_Cohort_Eng_Current_Num = string.Empty;
    string ReadingScaledScore_Tot_Cohort_Eng_Previous_Num = string.Empty;
    string ReadingScaledScore_Tot_Cohort_Eng_Previous2_Num = string.Empty;

    public EnglandPerformanceBuilder WithRwmExpected(string current, string prev, string prev2)
    {
        RwmExpected_Tot_Cohort_Eng_Current_Num = current;
        RwmExpected_Tot_Cohort_Eng_Previous_Num = prev;
        RwmExpected_Tot_Cohort_Eng_Previous2_Num = prev2;

        return this;
    }

    public EnglandPerformanceBuilder WithReadingScore(string current, string prev, string prev2)
    {
        ReadingScaledScore_Tot_Cohort_Eng_Current_Num = current;
        ReadingScaledScore_Tot_Cohort_Eng_Previous_Num = prev;
        ReadingScaledScore_Tot_Cohort_Eng_Previous2_Num = prev2;

        return this;
    }

    public EnglandPerformance Build() =>
        new EnglandPerformance()
        {
            Id = "National",
            RwmExpected_Tot_Cohort_Eng_Current_Num = RwmExpected_Tot_Cohort_Eng_Current_Num,
            RwmExpected_Tot_Cohort_Eng_Previous_Num = RwmExpected_Tot_Cohort_Eng_Previous_Num,
            RwmExpected_Tot_Cohort_Eng_Previous2_Num = RwmExpected_Tot_Cohort_Eng_Previous2_Num,
            ReadingScaledScore_Tot_Cohort_Eng_Current_Num = ReadingScaledScore_Tot_Cohort_Eng_Current_Num,
            ReadingScaledScore_Tot_Cohort_Eng_Previous_Num = ReadingScaledScore_Tot_Cohort_Eng_Previous_Num,
            ReadingScaledScore_Tot_Cohort_Eng_Previous2_Num = ReadingScaledScore_Tot_Cohort_Eng_Previous2_Num,
        };
}
