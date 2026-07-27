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

    public EstablishmentAbsenceBuilder WithOverallAbsence(
                 string current, 
                 string previous, 
                 string previous2)
    {
        Abs_Tot_Est_Current_Pct = current;
        Abs_Tot_Est_Previous_Pct = previous;
        Abs_Tot_Est_Previous2_Pct = previous2;

        return this;
    }

    public EstablishmentAbsenceBuilder WithPersistentAbsence(
               string current,
               string previous,
               string previous2)
    {
        Abs_Persistent_Est_Current_Pct = current;
        Abs_Persistent_Est_Previous_Pct = previous;
        Abs_Persistent_Est_Previous2_Pct = previous2;

        return this;
    }

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
