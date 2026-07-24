using SAPSec.Data.Dto.KS2.Performance;

namespace SAPSec.Test.Common.Builders;

public class EnglandPerformanceBuilder()
{
    string RwmExpected_Tot_Cohort_Eng_Current_Num = string.Empty;
    string RwmExpected_Tot_Cohort_Eng_Previous_Num = string.Empty;
    string RwmExpected_Tot_Cohort_Eng_Previous2_Num = string.Empty;
    string RwmExpected_Reading_Tot_Cohort_Eng_Current_Num = string.Empty;
    string RwmExpected_Reading_Tot_Cohort_Eng_Previous_Num = string.Empty;
    string RwmExpected_Reading_Tot_Cohort_Eng_Previous2_Num = string.Empty;
    string RwmExpected_Writing_Tot_Cohort_Eng_Current_Num = string.Empty;
    string RwmExpected_Writing_Tot_Cohort_Eng_Previous_Num = string.Empty;
    string RwmExpected_Writing_Tot_Cohort_Eng_Previous2_Num = string.Empty;
    string RwmExpected_Maths_Tot_Cohort_Eng_Current_Num = string.Empty;
    string RwmExpected_Maths_Tot_Cohort_Eng_Previous_Num = string.Empty;
    string RwmExpected_Maths_Tot_Cohort_Eng_Previous2_Num = string.Empty;

    public EnglandPerformanceBuilder WithRwmExpected(string current, string prev, string prev2)
    {
        RwmExpected_Tot_Cohort_Eng_Current_Num = current;
        RwmExpected_Tot_Cohort_Eng_Previous_Num = prev;
        RwmExpected_Tot_Cohort_Eng_Previous2_Num = prev2;

        return this;
    }

    public EnglandPerformanceBuilder WithRwmExpectedReading(string current, string prev, string prev2)
    {
        RwmExpected_Reading_Tot_Cohort_Eng_Current_Num = current;
        RwmExpected_Reading_Tot_Cohort_Eng_Previous_Num = prev;
        RwmExpected_Reading_Tot_Cohort_Eng_Previous2_Num = prev2;

        return this;
    }

    public EnglandPerformanceBuilder WithRwmExpectedWriting(string current, string prev, string prev2)
    {
        RwmExpected_Writing_Tot_Cohort_Eng_Current_Num = current;
        RwmExpected_Writing_Tot_Cohort_Eng_Previous_Num = prev;
        RwmExpected_Writing_Tot_Cohort_Eng_Previous2_Num = prev2;

        return this;
    }

    public EnglandPerformanceBuilder WithRwmExpectedMaths(string current, string prev, string prev2)
    {
        RwmExpected_Maths_Tot_Cohort_Eng_Current_Num = current;
        RwmExpected_Maths_Tot_Cohort_Eng_Previous_Num = prev;
        RwmExpected_Maths_Tot_Cohort_Eng_Previous2_Num = prev2;

        return this;
    }

    public EnglandPerformance Build() =>
        new EnglandPerformance()
        {
            Id = "National",
            RwmExpected_Tot_Cohort_Eng_Current_Num = RwmExpected_Tot_Cohort_Eng_Current_Num,
            RwmExpected_Tot_Cohort_Eng_Previous_Num = RwmExpected_Tot_Cohort_Eng_Previous_Num,
            RwmExpected_Tot_Cohort_Eng_Previous2_Num = RwmExpected_Tot_Cohort_Eng_Previous2_Num,
            RwmExpected_Reading_Tot_Cohort_Eng_Current_Num = RwmExpected_Reading_Tot_Cohort_Eng_Current_Num,
            RwmExpected_Reading_Tot_Cohort_Eng_Previous_Num = RwmExpected_Reading_Tot_Cohort_Eng_Previous_Num,
            RwmExpected_Reading_Tot_Cohort_Eng_Previous2_Num = RwmExpected_Reading_Tot_Cohort_Eng_Previous2_Num,
            RwmExpected_Writing_Tot_Cohort_Eng_Current_Num = RwmExpected_Writing_Tot_Cohort_Eng_Current_Num,
            RwmExpected_Writing_Tot_Cohort_Eng_Previous_Num = RwmExpected_Writing_Tot_Cohort_Eng_Previous_Num,
            RwmExpected_Writing_Tot_Cohort_Eng_Previous2_Num = RwmExpected_Writing_Tot_Cohort_Eng_Previous2_Num,
            RwmExpected_Maths_Tot_Cohort_Eng_Current_Num = RwmExpected_Maths_Tot_Cohort_Eng_Current_Num,
            RwmExpected_Maths_Tot_Cohort_Eng_Previous_Num = RwmExpected_Maths_Tot_Cohort_Eng_Previous_Num,
            RwmExpected_Maths_Tot_Cohort_Eng_Previous2_Num = RwmExpected_Maths_Tot_Cohort_Eng_Previous2_Num,
        };
}
