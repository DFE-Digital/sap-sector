using SAPSec.Data.Dto.Absence;

namespace SAPSec.Test.Common.Builders;

public class LAAbsenceBuilder(string laId)
{
    string Abs_Tot_LA_Current_Pct = string.Empty;
    string Abs_Tot_LA_Previous_Pct = string.Empty;
    string Abs_Tot_LA_Previous2_Pct = string.Empty;

    public LAAbsenceBuilder WithTotalAbsence(string current, string prev, string prev2)
    {
        Abs_Tot_LA_Current_Pct = current;
        Abs_Tot_LA_Previous_Pct = prev;
        Abs_Tot_LA_Previous2_Pct = prev2;

        return this;
    }

    public LAAbsence Build() =>
        new LAAbsence()
        {
            Id = laId,
            Abs_Tot_LA_Current_Pct = Abs_Tot_LA_Current_Pct,
            Abs_Tot_LA_Previous_Pct = Abs_Tot_LA_Previous_Pct,
            Abs_Tot_LA_Previous2_Pct = Abs_Tot_LA_Previous2_Pct,
        };
}
