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
    string Abs_Tot_Boy_Est_Current_Pct = string.Empty;
    string Abs_Tot_Boy_Est_Previous_Pct = string.Empty;
    string Abs_Tot_Boy_Est_Previous2_Pct = string.Empty;
    string Abs_Tot_Grl_Est_Current_Pct = string.Empty;
    string Abs_Tot_Grl_Est_Previous_Pct = string.Empty;
    string Abs_Tot_Grl_Est_Previous2_Pct = string.Empty;
    string Abs_Tot_Dis_Est_Current_Pct = string.Empty;
    string Abs_Tot_Dis_Est_Previous_Pct = string.Empty;
    string Abs_Tot_Dis_Est_Previous2_Pct = string.Empty;
    string Abs_Tot_NDi_Est_Current_Pct = string.Empty;
    string Abs_Tot_NDi_Est_Previous_Pct = string.Empty;
    string Abs_Tot_NDi_Est_Previous2_Pct = string.Empty;
    string Abs_Tot_EAL_Est_Current_Pct = string.Empty;
    string Abs_Tot_EAL_Est_Previous_Pct = string.Empty;
    string Abs_Tot_EAL_Est_Previous2_Pct = string.Empty;
    string Abs_Tot_EFL_Est_Current_Pct = string.Empty;
    string Abs_Tot_EFL_Est_Previous_Pct = string.Empty;
    string Abs_Tot_EFL_Est_Previous2_Pct = string.Empty;
    string Abs_Persistent_Boy_Est_Current_Pct = string.Empty;
    string Abs_Persistent_Boy_Est_Previous_Pct = string.Empty;
    string Abs_Persistent_Boy_Est_Previous2_Pct = string.Empty;
    string Abs_Persistent_Grl_Est_Current_Pct = string.Empty;
    string Abs_Persistent_Grl_Est_Previous_Pct = string.Empty;
    string Abs_Persistent_Grl_Est_Previous2_Pct = string.Empty;
    string Abs_Persistent_Dis_Est_Current_Pct = string.Empty;
    string Abs_Persistent_Dis_Est_Previous_Pct = string.Empty;
    string Abs_Persistent_Dis_Est_Previous2_Pct = string.Empty;
    string Abs_Persistent_NDi_Est_Current_Pct = string.Empty;
    string Abs_Persistent_NDi_Est_Previous_Pct = string.Empty;
    string Abs_Persistent_NDi_Est_Previous2_Pct = string.Empty;
    string Abs_Persistent_EAL_Est_Current_Pct = string.Empty;
    string Abs_Persistent_EAL_Est_Previous_Pct = string.Empty;
    string Abs_Persistent_EAL_Est_Previous2_Pct = string.Empty;
    string Abs_Persistent_EFL_Est_Current_Pct = string.Empty;
    string Abs_Persistent_EFL_Est_Previous_Pct = string.Empty;
    string Abs_Persistent_EFL_Est_Previous2_Pct = string.Empty;

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

    public EstablishmentAbsenceBuilder WithOverallAbsenceBoys(
        string current,
        string previous,
        string previous2)
    {
        Abs_Tot_Boy_Est_Current_Pct = current;
        Abs_Tot_Boy_Est_Previous_Pct = previous;
        Abs_Tot_Boy_Est_Previous2_Pct = previous2;

        return this;
    }

    public EstablishmentAbsenceBuilder WithOverallAbsenceGirls(
        string current,
        string previous,
        string previous2)
    {
        Abs_Tot_Grl_Est_Current_Pct = current;
        Abs_Tot_Grl_Est_Previous_Pct = previous;
        Abs_Tot_Grl_Est_Previous2_Pct = previous2;

        return this;
    }

    public EstablishmentAbsenceBuilder WithOverallAbsenceFsm(
        string current,
        string previous,
        string previous2)
    {
        Abs_Tot_Dis_Est_Current_Pct = current;
        Abs_Tot_Dis_Est_Previous_Pct = previous;
        Abs_Tot_Dis_Est_Previous2_Pct = previous2;

        return this;
    }

    public EstablishmentAbsenceBuilder WithOverallAbsenceNonFsm(
        string current,
        string previous,
        string previous2)
    {
        Abs_Tot_NDi_Est_Current_Pct = current;
        Abs_Tot_NDi_Est_Previous_Pct = previous;
        Abs_Tot_NDi_Est_Previous2_Pct = previous2;

        return this;
    }

    public EstablishmentAbsenceBuilder WithOverallAbsenceEal(
        string current,
        string previous,
        string previous2)
    {
        Abs_Tot_EAL_Est_Current_Pct = current;
        Abs_Tot_EAL_Est_Previous_Pct = previous;
        Abs_Tot_EAL_Est_Previous2_Pct = previous2;

        return this;
    }

    public EstablishmentAbsenceBuilder WithOverallAbsenceEfl(
        string current,
        string previous,
        string previous2)
    {
        Abs_Tot_EFL_Est_Current_Pct = current;
        Abs_Tot_EFL_Est_Previous_Pct = previous;
        Abs_Tot_EFL_Est_Previous2_Pct = previous2;

        return this;
    }

    public EstablishmentAbsenceBuilder WithPersistentAbsenceBoys(
        string current,
        string previous,
        string previous2)
    {
        Abs_Persistent_Boy_Est_Current_Pct = current;
        Abs_Persistent_Boy_Est_Previous_Pct = previous;
        Abs_Persistent_Boy_Est_Previous2_Pct = previous2;

        return this;
    }

    public EstablishmentAbsenceBuilder WithPersistentAbsenceGirls(
        string current,
        string previous,
        string previous2)
    {
        Abs_Persistent_Grl_Est_Current_Pct = current;
        Abs_Persistent_Grl_Est_Previous_Pct = previous;
        Abs_Persistent_Grl_Est_Previous2_Pct = previous2;

        return this;
    }

    public EstablishmentAbsenceBuilder WithPersistentAbsenceFsm(
        string current,
        string previous,
        string previous2)
    {
        Abs_Persistent_Dis_Est_Current_Pct = current;
        Abs_Persistent_Dis_Est_Previous_Pct = previous;
        Abs_Persistent_Dis_Est_Previous2_Pct = previous2;

        return this;
    }

    public EstablishmentAbsenceBuilder WithPersistentAbsenceNonFsm(
        string current,
        string previous,
        string previous2)
    {
        Abs_Persistent_NDi_Est_Current_Pct = current;
        Abs_Persistent_NDi_Est_Previous_Pct = previous;
        Abs_Persistent_NDi_Est_Previous2_Pct = previous2;

        return this;
    }

    public EstablishmentAbsenceBuilder WithPersistentAbsenceEal(
        string current,
        string previous,
        string previous2)
    {
        Abs_Persistent_EAL_Est_Current_Pct = current;
        Abs_Persistent_EAL_Est_Previous_Pct = previous;
        Abs_Persistent_EAL_Est_Previous2_Pct = previous2;

        return this;
    }

    public EstablishmentAbsenceBuilder WithPersistentAbsenceEfl(
        string current,
        string previous,
        string previous2)
    {
        Abs_Persistent_EFL_Est_Current_Pct = current;
        Abs_Persistent_EFL_Est_Previous_Pct = previous;
        Abs_Persistent_EFL_Est_Previous2_Pct = previous2;

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
            Abs_Tot_Boy_Est_Current_Pct = Abs_Tot_Boy_Est_Current_Pct,
            Abs_Tot_Boy_Est_Previous_Pct = Abs_Tot_Boy_Est_Previous_Pct,
            Abs_Tot_Boy_Est_Previous2_Pct = Abs_Tot_Boy_Est_Previous2_Pct,
            Abs_Tot_Grl_Est_Current_Pct = Abs_Tot_Grl_Est_Current_Pct,
            Abs_Tot_Grl_Est_Previous_Pct = Abs_Tot_Grl_Est_Previous_Pct,
            Abs_Tot_Grl_Est_Previous2_Pct = Abs_Tot_Grl_Est_Previous2_Pct,
            Abs_Tot_Dis_Est_Current_Pct = Abs_Tot_Dis_Est_Current_Pct,
            Abs_Tot_Dis_Est_Previous_Pct = Abs_Tot_Dis_Est_Previous_Pct,
            Abs_Tot_Dis_Est_Previous2_Pct = Abs_Tot_Dis_Est_Previous2_Pct,
            Abs_Tot_NDi_Est_Current_Pct = Abs_Tot_NDi_Est_Current_Pct,
            Abs_Tot_NDi_Est_Previous_Pct = Abs_Tot_NDi_Est_Previous_Pct,
            Abs_Tot_NDi_Est_Previous2_Pct = Abs_Tot_NDi_Est_Previous2_Pct,
            Abs_Tot_EAL_Est_Current_Pct = Abs_Tot_EAL_Est_Current_Pct,
            Abs_Tot_EAL_Est_Previous_Pct = Abs_Tot_EAL_Est_Previous_Pct,
            Abs_Tot_EAL_Est_Previous2_Pct = Abs_Tot_EAL_Est_Previous2_Pct,
            Abs_Tot_EFL_Est_Current_Pct = Abs_Tot_EFL_Est_Current_Pct,
            Abs_Tot_EFL_Est_Previous_Pct = Abs_Tot_EFL_Est_Previous_Pct,
            Abs_Tot_EFL_Est_Previous2_Pct = Abs_Tot_EFL_Est_Previous2_Pct,
            Abs_Persistent_Boy_Est_Current_Pct = Abs_Persistent_Boy_Est_Current_Pct,
            Abs_Persistent_Boy_Est_Previous_Pct = Abs_Persistent_Boy_Est_Previous_Pct,
            Abs_Persistent_Boy_Est_Previous2_Pct = Abs_Persistent_Boy_Est_Previous2_Pct,
            Abs_Persistent_Grl_Est_Current_Pct = Abs_Persistent_Grl_Est_Current_Pct,
            Abs_Persistent_Grl_Est_Previous_Pct = Abs_Persistent_Grl_Est_Previous_Pct,
            Abs_Persistent_Grl_Est_Previous2_Pct = Abs_Persistent_Grl_Est_Previous2_Pct,
            Abs_Persistent_Dis_Est_Current_Pct = Abs_Persistent_Dis_Est_Current_Pct,
            Abs_Persistent_Dis_Est_Previous_Pct = Abs_Persistent_Dis_Est_Previous_Pct,
            Abs_Persistent_Dis_Est_Previous2_Pct = Abs_Persistent_Dis_Est_Previous2_Pct,
            Abs_Persistent_NDi_Est_Current_Pct = Abs_Persistent_NDi_Est_Current_Pct,
            Abs_Persistent_NDi_Est_Previous_Pct = Abs_Persistent_NDi_Est_Previous_Pct,
            Abs_Persistent_NDi_Est_Previous2_Pct = Abs_Persistent_NDi_Est_Previous2_Pct,
            Abs_Persistent_EAL_Est_Current_Pct = Abs_Persistent_EAL_Est_Current_Pct,
            Abs_Persistent_EAL_Est_Previous_Pct = Abs_Persistent_EAL_Est_Previous_Pct,
            Abs_Persistent_EAL_Est_Previous2_Pct = Abs_Persistent_EAL_Est_Previous2_Pct,
            Abs_Persistent_EFL_Est_Current_Pct = Abs_Persistent_EFL_Est_Current_Pct,
            Abs_Persistent_EFL_Est_Previous_Pct = Abs_Persistent_EFL_Est_Previous_Pct,
            Abs_Persistent_EFL_Est_Previous2_Pct = Abs_Persistent_EFL_Est_Previous2_Pct,
        };
}
