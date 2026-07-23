using SAPSec.Data.Dto.Absence;

namespace SAPSec.Test.Common.Builders;

public class EstablishmentAbsenceBuilder(string urn)
{
    string Abs_Tot_Est_Current_Pct = string.Empty;
    string Abs_Tot_Est_Previous_Pct = string.Empty;
    string Abs_Tot_Est_Previous2_Pct = string.Empty;
    string Abs_Persistent_Est_Current_Pct = string.Empty;
    string Abs_Persistent_Est_Previous_Pct = string.Empty;
    string Abs_Persistent_Est_Previous2_Pct = string.Empty;

    public EstablishmentAbsenceBuilder WithAbsence(
                 string overallCurrent, 
                 string overallPrevious, 
                 string overallPrevious2,
                 string persistentCurrent,
                 string persistentPrevious,
                 string persistentPrevious2)
    {
        Abs_Tot_Est_Current_Pct = overallCurrent;
        Abs_Tot_Est_Previous_Pct = overallPrevious;
        Abs_Tot_Est_Previous2_Pct = overallPrevious2;
        Abs_Persistent_Est_Current_Pct = persistentCurrent                      ;
        Abs_Persistent_Est_Previous_Pct = persistentPrevious;
        Abs_Persistent_Est_Previous2_Pct = persistentPrevious2;

        return this;
    }

    //public EstablishmentAbsenceBuilder WithPersistentAbsence(string current, string prev, string prev2)
    //{
    //    Abs_Persistent_Est_Current_Pct = current;
    //    Abs_Persistent_Est_Previous_Pct = prev;
    //    Abs_Persistent_Est_Previous2_Pct = prev2;

    //    return this;
    //}

    public EstablishmentAbsence Build() =>
        new EstablishmentAbsence()
        {
            Id = urn,
            Abs_Tot_Est_Current_Pct = Abs_Tot_Est_Current_Pct,
            Abs_Tot_Est_Previous_Pct = Abs_Tot_Est_Previous_Pct,
            Abs_Tot_Est_Previous2_Pct = Abs_Tot_Est_Previous2_Pct,
            Abs_Persistent_Est_Current_Pct = Abs_Persistent_Est_Current_Pct,
            Abs_Persistent_Est_Previous_Pct = Abs_Persistent_Est_Previous_Pct,
            Abs_Persistent_Est_Previous2_Pct = Abs_Persistent_Est_Previous2_Pct,
        };
}
