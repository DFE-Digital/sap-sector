using SAPSec.Data.Dto.KS2.Performance;

namespace SAPSec.Test.Common.Builders;

public class EnglandPerformanceBuilder()
{
    string RwmExpected_Tot_Cohort_Eng_Current_Num = string.Empty;
    string RwmExpected_Tot_Cohort_Eng_Previous_Num = string.Empty;
    string RwmExpected_Tot_Cohort_Eng_Previous2_Num = string.Empty;

    public EnglandPerformanceBuilder WithRwmExpected(string current, string previous, string previous2)
    {
        RwmExpected_Tot_Cohort_Eng_Current_Num = current;
        RwmExpected_Tot_Cohort_Eng_Previous_Num = previous;
        RwmExpected_Tot_Cohort_Eng_Previous2_Num = previous2;

        return this;
    }

    public EnglandPerformance Build() =>
        new EnglandPerformance()
        {
            Id = "National",
            RwmExpected_Tot_Cohort_Eng_Current_Num = RwmExpected_Tot_Cohort_Eng_Current_Num,
            RwmExpected_Tot_Cohort_Eng_Previous_Num = RwmExpected_Tot_Cohort_Eng_Previous_Num,
            RwmExpected_Tot_Cohort_Eng_Previous2_Num = RwmExpected_Tot_Cohort_Eng_Previous2_Num,
        };
}
