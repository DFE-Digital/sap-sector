using SAPSec.Data.Dto.Absence;

namespace SAPSec.Test.Common.Builders;

public class EnglandAbsenceBuilder()
{
    string Abs_Tot_Eng_Current_Pct = string.Empty;
    string Abs_Tot_Eng_Previous_Pct = string.Empty;
    string Abs_Tot_Eng_Previous2_Pct = string.Empty;

    public EnglandAbsenceBuilder WithTotalAbsence(string current, string prev, string prev2)
    {
        Abs_Tot_Eng_Current_Pct = current;
        Abs_Tot_Eng_Previous_Pct = prev;
        Abs_Tot_Eng_Previous2_Pct = prev2;

        return this;
    }

    public EnglandAbsence Build() =>
        new EnglandAbsence()
        {
            Id = "National",
            Abs_Tot_Eng_Current_Pct = Abs_Tot_Eng_Current_Pct,
            Abs_Tot_Eng_Previous_Pct = Abs_Tot_Eng_Previous_Pct,
            Abs_Tot_Eng_Previous2_Pct = Abs_Tot_Eng_Previous2_Pct,
        };
}
