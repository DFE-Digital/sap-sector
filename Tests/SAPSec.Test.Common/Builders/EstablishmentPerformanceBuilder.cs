using SAPSec.Data.Dto.KS2.Performance;

namespace SAPSec.Test.Common.Builders;

public class EstablishmentPerformanceBuilder(string urn)
{
    string RwmExpected_Tot_Cohort_Est_Current_Num = string.Empty;
    string RwmExpected_Tot_Cohort_Est_Previous_Num = string.Empty;
    string RwmExpected_Tot_Cohort_Est_Previous2_Num = string.Empty;

    public EstablishmentPerformanceBuilder WithRwmExpected(string current, string previous, string previous2)
    {
        RwmExpected_Tot_Cohort_Est_Current_Num = current;
        RwmExpected_Tot_Cohort_Est_Previous_Num = previous;
        RwmExpected_Tot_Cohort_Est_Previous2_Num = previous2;

        return this;
    }

    public EstablishmentPerformance Build() =>
        new EstablishmentPerformance()
        {
            Id = urn,
            RwmExpected_Tot_Cohort_Est_Current_Num = RwmExpected_Tot_Cohort_Est_Current_Num,
            RwmExpected_Tot_Cohort_Est_Previous_Num = RwmExpected_Tot_Cohort_Est_Previous_Num,
            RwmExpected_Tot_Cohort_Est_Previous2_Num = RwmExpected_Tot_Cohort_Est_Previous2_Num,
        };
}
