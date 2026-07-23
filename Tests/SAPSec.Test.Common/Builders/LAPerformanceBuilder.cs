using SAPSec.Data.Dto.KS2.Performance;

namespace SAPSec.Test.Common.Builders;

public class LAPerformanceBuilder(string laId)
{
    string RwmExpected_Tot_Cohort_LA_Current_Num = string.Empty;
    string RwmExpected_Tot_Cohort_LA_Previous_Num = string.Empty;
    string RwmExpected_Tot_Cohort_LA_Previous2_Num = string.Empty;
    string RwmExpected_Reading_Tot_Cohort_LA_Current_Num = string.Empty;
    string RwmExpected_Reading_Tot_Cohort_LA_Previous_Num = string.Empty;
    string RwmExpected_Reading_Tot_Cohort_LA_Previous2_Num = string.Empty;
    string RwmExpected_Writing_Tot_Cohort_LA_Current_Num = string.Empty;
    string RwmExpected_Writing_Tot_Cohort_LA_Previous_Num = string.Empty;
    string RwmExpected_Writing_Tot_Cohort_LA_Previous2_Num = string.Empty;
    string RwmExpected_Maths_Tot_Cohort_LA_Current_Num = string.Empty;
    string RwmExpected_Maths_Tot_Cohort_LA_Previous_Num = string.Empty;
    string RwmExpected_Maths_Tot_Cohort_LA_Previous2_Num = string.Empty;

    public LAPerformanceBuilder WithRwmExpected(string current, string prev, string prev2)
    {
        RwmExpected_Tot_Cohort_LA_Current_Num = current;
        RwmExpected_Tot_Cohort_LA_Previous_Num = prev;
        RwmExpected_Tot_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithRwmExpectedReading(string current, string prev, string prev2)
    {
        RwmExpected_Reading_Tot_Cohort_LA_Current_Num = current;
        RwmExpected_Reading_Tot_Cohort_LA_Previous_Num = prev;
        RwmExpected_Reading_Tot_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithRwmExpectedWriting(string current, string prev, string prev2)
    {
        RwmExpected_Writing_Tot_Cohort_LA_Current_Num = current;
        RwmExpected_Writing_Tot_Cohort_LA_Previous_Num = prev;
        RwmExpected_Writing_Tot_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformanceBuilder WithRwmExpectedMaths(string current, string prev, string prev2)
    {
        RwmExpected_Maths_Tot_Cohort_LA_Current_Num = current;
        RwmExpected_Maths_Tot_Cohort_LA_Previous_Num = prev;
        RwmExpected_Maths_Tot_Cohort_LA_Previous2_Num = prev2;

        return this;
    }

    public LAPerformance Build() =>
        new LAPerformance()
        {
            Id = laId,
            RwmExpected_Tot_Cohort_LA_Current_Num = RwmExpected_Tot_Cohort_LA_Current_Num,
            RwmExpected_Tot_Cohort_LA_Previous_Num = RwmExpected_Tot_Cohort_LA_Previous_Num,
            RwmExpected_Tot_Cohort_LA_Previous2_Num = RwmExpected_Tot_Cohort_LA_Previous2_Num,
            RwmExpected_Reading_Tot_Cohort_LA_Current_Num = RwmExpected_Reading_Tot_Cohort_LA_Current_Num,
            RwmExpected_Reading_Tot_Cohort_LA_Previous_Num = RwmExpected_Reading_Tot_Cohort_LA_Previous_Num,
            RwmExpected_Reading_Tot_Cohort_LA_Previous2_Num = RwmExpected_Reading_Tot_Cohort_LA_Previous2_Num,
            RwmExpected_Writing_Tot_Cohort_LA_Current_Num = RwmExpected_Writing_Tot_Cohort_LA_Current_Num,
            RwmExpected_Writing_Tot_Cohort_LA_Previous_Num = RwmExpected_Writing_Tot_Cohort_LA_Previous_Num,
            RwmExpected_Writing_Tot_Cohort_LA_Previous2_Num = RwmExpected_Writing_Tot_Cohort_LA_Previous2_Num,
            RwmExpected_Maths_Tot_Cohort_LA_Current_Num = RwmExpected_Maths_Tot_Cohort_LA_Current_Num,
            RwmExpected_Maths_Tot_Cohort_LA_Previous_Num = RwmExpected_Maths_Tot_Cohort_LA_Previous_Num,
            RwmExpected_Maths_Tot_Cohort_LA_Previous2_Num = RwmExpected_Maths_Tot_Cohort_LA_Previous2_Num,
        };
}
