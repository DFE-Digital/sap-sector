using SAPSec.Data.Dto.Absence;

namespace SAPSec.Test.Common.Builders;

public class LAAbsenceBuilder(string laId)
{
    string Abs_Tot_Secondary_LA_Current_Pct = string.Empty;
    string Abs_Tot_Secondary_LA_Previous_Pct = string.Empty;
    string Abs_Tot_Secondary_LA_Previous2_Pct = string.Empty;
    string Abs_Persistent_Secondary_LA_Current_Pct = string.Empty;
    string Abs_Persistent_Secondary_LA_Previous_Pct = string.Empty;
    string Abs_Persistent_Secondary_LA_Previous2_Pct = string.Empty;
    string Abs_Tot_Primary_LA_Current_Pct = string.Empty;
    string Abs_Tot_Primary_LA_Previous_Pct = string.Empty;
    string Abs_Tot_Primary_LA_Previous2_Pct = string.Empty;
    string Abs_Persistent_Primary_LA_Current_Pct = string.Empty;
    string Abs_Persistent_Primary_LA_Previous_Pct = string.Empty;
    string Abs_Persistent_Primary_LA_Previous2_Pct = string.Empty;
    string Abs_Tot_Boy_Primary_LA_Current_Pct = string.Empty;
    string Abs_Tot_Boy_Primary_LA_Previous_Pct = string.Empty;
    string Abs_Tot_Boy_Primary_LA_Previous2_Pct = string.Empty;
    string Abs_Tot_Boy_Secondary_LA_Current_Pct = string.Empty;
    string Abs_Tot_Boy_Secondary_LA_Previous_Pct = string.Empty;
    string Abs_Tot_Boy_Secondary_LA_Previous2_Pct = string.Empty;
    string Abs_Tot_Grl_Primary_LA_Current_Pct = string.Empty;
    string Abs_Tot_Grl_Primary_LA_Previous_Pct = string.Empty;
    string Abs_Tot_Grl_Primary_LA_Previous2_Pct = string.Empty;
    string Abs_Tot_Grl_Secondary_LA_Current_Pct = string.Empty;
    string Abs_Tot_Grl_Secondary_LA_Previous_Pct = string.Empty;
    string Abs_Tot_Grl_Secondary_LA_Previous2_Pct = string.Empty;
    string Abs_Tot_Dis_Primary_LA_Current_Pct = string.Empty;
    string Abs_Tot_Dis_Primary_LA_Previous_Pct = string.Empty;
    string Abs_Tot_Dis_Primary_LA_Previous2_Pct = string.Empty;
    string Abs_Tot_Dis_Secondary_LA_Current_Pct = string.Empty;
    string Abs_Tot_Dis_Secondary_LA_Previous_Pct = string.Empty;
    string Abs_Tot_Dis_Secondary_LA_Previous2_Pct = string.Empty;
    string Abs_Tot_NDi_Primary_LA_Current_Pct = string.Empty;
    string Abs_Tot_NDi_Primary_LA_Previous_Pct = string.Empty;
    string Abs_Tot_NDi_Primary_LA_Previous2_Pct = string.Empty;
    string Abs_Tot_NDi_Secondary_LA_Current_Pct = string.Empty;
    string Abs_Tot_NDi_Secondary_LA_Previous_Pct = string.Empty;
    string Abs_Tot_NDi_Secondary_LA_Previous2_Pct = string.Empty;
    string Abs_Tot_EAL_Primary_LA_Current_Pct = string.Empty;
    string Abs_Tot_EAL_Primary_LA_Previous_Pct = string.Empty;
    string Abs_Tot_EAL_Primary_LA_Previous2_Pct = string.Empty;
    string Abs_Tot_EAL_Secondary_LA_Current_Pct = string.Empty;
    string Abs_Tot_EAL_Secondary_LA_Previous_Pct = string.Empty;
    string Abs_Tot_EAL_Secondary_LA_Previous2_Pct = string.Empty;
    string Abs_Tot_EFL_Primary_LA_Current_Pct = string.Empty;
    string Abs_Tot_EFL_Primary_LA_Previous_Pct = string.Empty;
    string Abs_Tot_EFL_Primary_LA_Previous2_Pct = string.Empty;
    string Abs_Tot_EFL_Secondary_LA_Current_Pct = string.Empty;
    string Abs_Tot_EFL_Secondary_LA_Previous_Pct = string.Empty;
    string Abs_Tot_EFL_Secondary_LA_Previous2_Pct = string.Empty;
    string Abs_Persistent_Boy_Primary_LA_Current_Pct = string.Empty;
    string Abs_Persistent_Boy_Primary_LA_Previous_Pct = string.Empty;
    string Abs_Persistent_Boy_Primary_LA_Previous2_Pct = string.Empty;
    string Abs_Persistent_Boy_Secondary_LA_Current_Pct = string.Empty;
    string Abs_Persistent_Boy_Secondary_LA_Previous_Pct = string.Empty;
    string Abs_Persistent_Boy_Secondary_LA_Previous2_Pct = string.Empty;
    string Abs_Persistent_Grl_Primary_LA_Current_Pct = string.Empty;
    string Abs_Persistent_Grl_Primary_LA_Previous_Pct = string.Empty;
    string Abs_Persistent_Grl_Primary_LA_Previous2_Pct = string.Empty;
    string Abs_Persistent_Grl_Secondary_LA_Current_Pct = string.Empty;
    string Abs_Persistent_Grl_Secondary_LA_Previous_Pct = string.Empty;
    string Abs_Persistent_Grl_Secondary_LA_Previous2_Pct = string.Empty;
    string Abs_Persistent_Dis_Primary_LA_Current_Pct = string.Empty;
    string Abs_Persistent_Dis_Primary_LA_Previous_Pct = string.Empty;
    string Abs_Persistent_Dis_Primary_LA_Previous2_Pct = string.Empty;
    string Abs_Persistent_Dis_Secondary_LA_Current_Pct = string.Empty;
    string Abs_Persistent_Dis_Secondary_LA_Previous_Pct = string.Empty;
    string Abs_Persistent_Dis_Secondary_LA_Previous2_Pct = string.Empty;
    string Abs_Persistent_NDi_Primary_LA_Current_Pct = string.Empty;
    string Abs_Persistent_NDi_Primary_LA_Previous_Pct = string.Empty;
    string Abs_Persistent_NDi_Primary_LA_Previous2_Pct = string.Empty;
    string Abs_Persistent_NDi_Secondary_LA_Current_Pct = string.Empty;
    string Abs_Persistent_NDi_Secondary_LA_Previous_Pct = string.Empty;
    string Abs_Persistent_NDi_Secondary_LA_Previous2_Pct = string.Empty;
    string Abs_Persistent_EAL_Primary_LA_Current_Pct = string.Empty;
    string Abs_Persistent_EAL_Primary_LA_Previous_Pct = string.Empty;
    string Abs_Persistent_EAL_Primary_LA_Previous2_Pct = string.Empty;
    string Abs_Persistent_EAL_Secondary_LA_Current_Pct = string.Empty;
    string Abs_Persistent_EAL_Secondary_LA_Previous_Pct = string.Empty;
    string Abs_Persistent_EAL_Secondary_LA_Previous2_Pct = string.Empty;
    string Abs_Persistent_EFL_Primary_LA_Current_Pct = string.Empty;
    string Abs_Persistent_EFL_Primary_LA_Previous_Pct = string.Empty;
    string Abs_Persistent_EFL_Primary_LA_Previous2_Pct = string.Empty;
    string Abs_Persistent_EFL_Secondary_LA_Current_Pct = string.Empty;
    string Abs_Persistent_EFL_Secondary_LA_Previous_Pct = string.Empty;
    string Abs_Persistent_EFL_Secondary_LA_Previous2_Pct = string.Empty;

    public LAAbsenceBuilder WithOverallAbsenceSecondary(
                 string current, 
                 string previous, 
                 string previous2)
    {
        Abs_Tot_Secondary_LA_Current_Pct = current;
        Abs_Tot_Secondary_LA_Previous_Pct = previous;
        Abs_Tot_Secondary_LA_Previous2_Pct = previous2;

        return this;
    }

    public LAAbsenceBuilder WithPersistentAbsenceSecondary(
               string current,
               string previous,
               string previous2)
    {
        Abs_Persistent_Secondary_LA_Current_Pct = current;
        Abs_Persistent_Secondary_LA_Previous_Pct = previous;
        Abs_Persistent_Secondary_LA_Previous2_Pct = previous2;

        return this;
    }

    public LAAbsenceBuilder WithOverallAbsencePrimary(
             string current,
             string previous,
             string previous2)
    {
        Abs_Tot_Primary_LA_Current_Pct = current;
        Abs_Tot_Primary_LA_Previous_Pct = previous;
        Abs_Tot_Primary_LA_Previous2_Pct = previous2;

        return this;
    }

    public LAAbsenceBuilder WithPersistentAbsencePrimary(
               string current,
               string previous,
               string previous2)
    {
        Abs_Persistent_Primary_LA_Current_Pct = current;
        Abs_Persistent_Primary_LA_Previous_Pct = previous;
        Abs_Persistent_Primary_LA_Previous2_Pct = previous2;

        return this;
    }

    public LAAbsenceBuilder WithOverallAbsenceBoysPrimary(
        string current,
        string previous,
        string previous2)
    {
        Abs_Tot_Boy_Primary_LA_Current_Pct = current;
        Abs_Tot_Boy_Primary_LA_Previous_Pct = previous;
        Abs_Tot_Boy_Primary_LA_Previous2_Pct = previous2;

        return this;
    }

    public LAAbsenceBuilder WithOverallAbsenceBoysSecondary(
        string current,
        string previous,
        string previous2)
    {
        Abs_Tot_Boy_Secondary_LA_Current_Pct = current;
        Abs_Tot_Boy_Secondary_LA_Previous_Pct = previous;
        Abs_Tot_Boy_Secondary_LA_Previous2_Pct = previous2;

        return this;
    }

    public LAAbsenceBuilder WithOverallAbsenceGirlsPrimary(
        string current,
        string previous,
        string previous2)
    {
        Abs_Tot_Grl_Primary_LA_Current_Pct = current;
        Abs_Tot_Grl_Primary_LA_Previous_Pct = previous;
        Abs_Tot_Grl_Primary_LA_Previous2_Pct = previous2;

        return this;
    }

    public LAAbsenceBuilder WithOverallAbsenceGirlsSecondary(
        string current,
        string previous,
        string previous2)
    {
        Abs_Tot_Grl_Secondary_LA_Current_Pct = current;
        Abs_Tot_Grl_Secondary_LA_Previous_Pct = previous;
        Abs_Tot_Grl_Secondary_LA_Previous2_Pct = previous2;

        return this;
    }

    public LAAbsenceBuilder WithOverallAbsenceFsmPrimary(
        string current,
        string previous,
        string previous2)
    {
        Abs_Tot_Dis_Primary_LA_Current_Pct = current;
        Abs_Tot_Dis_Primary_LA_Previous_Pct = previous;
        Abs_Tot_Dis_Primary_LA_Previous2_Pct = previous2;

        return this;
    }

    public LAAbsenceBuilder WithOverallAbsenceFsmSecondary(
        string current,
        string previous,
        string previous2)
    {
        Abs_Tot_Dis_Secondary_LA_Current_Pct = current;
        Abs_Tot_Dis_Secondary_LA_Previous_Pct = previous;
        Abs_Tot_Dis_Secondary_LA_Previous2_Pct = previous2;

        return this;
    }

    public LAAbsenceBuilder WithOverallAbsenceNonFsmPrimary(
        string current,
        string previous,
        string previous2)
    {
        Abs_Tot_NDi_Primary_LA_Current_Pct = current;
        Abs_Tot_NDi_Primary_LA_Previous_Pct = previous;
        Abs_Tot_NDi_Primary_LA_Previous2_Pct = previous2;

        return this;
    }

    public LAAbsenceBuilder WithOverallAbsenceNonFsmSecondary(
        string current,
        string previous,
        string previous2)
    {
        Abs_Tot_NDi_Secondary_LA_Current_Pct = current;
        Abs_Tot_NDi_Secondary_LA_Previous_Pct = previous;
        Abs_Tot_NDi_Secondary_LA_Previous2_Pct = previous2;

        return this;
    }

    public LAAbsenceBuilder WithOverallAbsenceEalPrimary(
        string current,
        string previous,
        string previous2)
    {
        Abs_Tot_EAL_Primary_LA_Current_Pct = current;
        Abs_Tot_EAL_Primary_LA_Previous_Pct = previous;
        Abs_Tot_EAL_Primary_LA_Previous2_Pct = previous2;

        return this;
    }

    public LAAbsenceBuilder WithOverallAbsenceEalSecondary(
        string current,
        string previous,
        string previous2)
    {
        Abs_Tot_EAL_Secondary_LA_Current_Pct = current;
        Abs_Tot_EAL_Secondary_LA_Previous_Pct = previous;
        Abs_Tot_EAL_Secondary_LA_Previous2_Pct = previous2;

        return this;
    }

    public LAAbsenceBuilder WithOverallAbsenceEflPrimary(
        string current,
        string previous,
        string previous2)
    {
        Abs_Tot_EFL_Primary_LA_Current_Pct = current;
        Abs_Tot_EFL_Primary_LA_Previous_Pct = previous;
        Abs_Tot_EFL_Primary_LA_Previous2_Pct = previous2;

        return this;
    }

    public LAAbsenceBuilder WithOverallAbsenceEflSecondary(
        string current,
        string previous,
        string previous2)
    {
        Abs_Tot_EFL_Secondary_LA_Current_Pct = current;
        Abs_Tot_EFL_Secondary_LA_Previous_Pct = previous;
        Abs_Tot_EFL_Secondary_LA_Previous2_Pct = previous2;

        return this;
    }

    public LAAbsenceBuilder WithPersistentAbsenceBoysPrimary(
        string current,
        string previous,
        string previous2)
    {
        Abs_Persistent_Boy_Primary_LA_Current_Pct = current;
        Abs_Persistent_Boy_Primary_LA_Previous_Pct = previous;
        Abs_Persistent_Boy_Primary_LA_Previous2_Pct = previous2;

        return this;
    }

    public LAAbsenceBuilder WithPersistentAbsenceBoysSecondary(
        string current,
        string previous,
        string previous2)
    {
        Abs_Persistent_Boy_Secondary_LA_Current_Pct = current;
        Abs_Persistent_Boy_Secondary_LA_Previous_Pct = previous;
        Abs_Persistent_Boy_Secondary_LA_Previous2_Pct = previous2;

        return this;
    }

    public LAAbsenceBuilder WithPersistentAbsenceGirlsPrimary(
        string current,
        string previous,
        string previous2)
    {
        Abs_Persistent_Grl_Primary_LA_Current_Pct = current;
        Abs_Persistent_Grl_Primary_LA_Previous_Pct = previous;
        Abs_Persistent_Grl_Primary_LA_Previous2_Pct = previous2;

        return this;
    }

    public LAAbsenceBuilder WithPersistentAbsenceGirlsSecondary(
        string current,
        string previous,
        string previous2)
    {
        Abs_Persistent_Grl_Secondary_LA_Current_Pct = current;
        Abs_Persistent_Grl_Secondary_LA_Previous_Pct = previous;
        Abs_Persistent_Grl_Secondary_LA_Previous2_Pct = previous2;

        return this;
    }

    public LAAbsenceBuilder WithPersistentAbsenceFsmPrimary(
        string current,
        string previous,
        string previous2)
    {
        Abs_Persistent_Dis_Primary_LA_Current_Pct = current;
        Abs_Persistent_Dis_Primary_LA_Previous_Pct = previous;
        Abs_Persistent_Dis_Primary_LA_Previous2_Pct = previous2;

        return this;
    }

    public LAAbsenceBuilder WithPersistentAbsenceFsmSecondary(
        string current,
        string previous,
        string previous2)
    {
        Abs_Persistent_Dis_Secondary_LA_Current_Pct = current;
        Abs_Persistent_Dis_Secondary_LA_Previous_Pct = previous;
        Abs_Persistent_Dis_Secondary_LA_Previous2_Pct = previous2;

        return this;
    }

    public LAAbsenceBuilder WithPersistentAbsenceNonFsmPrimary(
        string current,
        string previous,
        string previous2)
    {
        Abs_Persistent_NDi_Primary_LA_Current_Pct = current;
        Abs_Persistent_NDi_Primary_LA_Previous_Pct = previous;
        Abs_Persistent_NDi_Primary_LA_Previous2_Pct = previous2;

        return this;
    }

    public LAAbsenceBuilder WithPersistentAbsenceNonFsmSecondary(
        string current,
        string previous,
        string previous2)
    {
        Abs_Persistent_NDi_Secondary_LA_Current_Pct = current;
        Abs_Persistent_NDi_Secondary_LA_Previous_Pct = previous;
        Abs_Persistent_NDi_Secondary_LA_Previous2_Pct = previous2;

        return this;
    }

    public LAAbsenceBuilder WithPersistentAbsenceEalPrimary(
        string current,
        string previous,
        string previous2)
    {
        Abs_Persistent_EAL_Primary_LA_Current_Pct = current;
        Abs_Persistent_EAL_Primary_LA_Previous_Pct = previous;
        Abs_Persistent_EAL_Primary_LA_Previous2_Pct = previous2;

        return this;
    }

    public LAAbsenceBuilder WithPersistentAbsenceEalSecondary(
        string current,
        string previous,
        string previous2)
    {
        Abs_Persistent_EAL_Secondary_LA_Current_Pct = current;
        Abs_Persistent_EAL_Secondary_LA_Previous_Pct = previous;
        Abs_Persistent_EAL_Secondary_LA_Previous2_Pct = previous2;

        return this;
    }

    public LAAbsenceBuilder WithPersistentAbsenceEflPrimary(
        string current,
        string previous,
        string previous2)
    {
        Abs_Persistent_EFL_Primary_LA_Current_Pct = current;
        Abs_Persistent_EFL_Primary_LA_Previous_Pct = previous;
        Abs_Persistent_EFL_Primary_LA_Previous2_Pct = previous2;

        return this;
    }

    public LAAbsenceBuilder WithPersistentAbsenceEflSecondary(
        string current,
        string previous,
        string previous2)
    {
        Abs_Persistent_EFL_Secondary_LA_Current_Pct = current;
        Abs_Persistent_EFL_Secondary_LA_Previous_Pct = previous;
        Abs_Persistent_EFL_Secondary_LA_Previous2_Pct = previous2;

        return this;
    }

    public LAAbsence Build() =>
        new LAAbsence()
        {
            Id = laId,
            Abs_Tot_Secondary_LA_Current_Pct = Abs_Tot_Secondary_LA_Current_Pct,
            Abs_Tot_Secondary_LA_Previous_Pct = Abs_Tot_Secondary_LA_Previous_Pct,
            Abs_Tot_Secondary_LA_Previous2_Pct = Abs_Tot_Secondary_LA_Previous2_Pct,
            Abs_Persistent_Secondary_LA_Current_Pct = Abs_Persistent_Secondary_LA_Current_Pct,
            Abs_Persistent_Secondary_LA_Previous_Pct = Abs_Persistent_Secondary_LA_Previous_Pct,
            Abs_Persistent_Secondary_LA_Previous2_Pct = Abs_Persistent_Secondary_LA_Previous2_Pct,
            Abs_Tot_Primary_LA_Current_Pct = Abs_Tot_Primary_LA_Current_Pct,
            Abs_Tot_Primary_LA_Previous_Pct = Abs_Tot_Primary_LA_Previous_Pct,
            Abs_Tot_Primary_LA_Previous2_Pct = Abs_Tot_Primary_LA_Previous2_Pct,
            Abs_Persistent_Primary_LA_Current_Pct = Abs_Persistent_Primary_LA_Current_Pct,
            Abs_Persistent_Primary_LA_Previous_Pct = Abs_Persistent_Primary_LA_Previous_Pct,
            Abs_Persistent_Primary_LA_Previous2_Pct = Abs_Persistent_Primary_LA_Previous2_Pct,
            Abs_Tot_Boy_Primary_LA_Current_Pct = Abs_Tot_Boy_Primary_LA_Current_Pct,
            Abs_Tot_Boy_Primary_LA_Previous_Pct = Abs_Tot_Boy_Primary_LA_Previous_Pct,
            Abs_Tot_Boy_Primary_LA_Previous2_Pct = Abs_Tot_Boy_Primary_LA_Previous2_Pct,
            Abs_Tot_Boy_Secondary_LA_Current_Pct = Abs_Tot_Boy_Secondary_LA_Current_Pct,
            Abs_Tot_Boy_Secondary_LA_Previous_Pct = Abs_Tot_Boy_Secondary_LA_Previous_Pct,
            Abs_Tot_Boy_Secondary_LA_Previous2_Pct = Abs_Tot_Boy_Secondary_LA_Previous2_Pct,
            Abs_Tot_Grl_Primary_LA_Current_Pct = Abs_Tot_Grl_Primary_LA_Current_Pct,
            Abs_Tot_Grl_Primary_LA_Previous_Pct = Abs_Tot_Grl_Primary_LA_Previous_Pct,
            Abs_Tot_Grl_Primary_LA_Previous2_Pct = Abs_Tot_Grl_Primary_LA_Previous2_Pct,
            Abs_Tot_Grl_Secondary_LA_Current_Pct = Abs_Tot_Grl_Secondary_LA_Current_Pct,
            Abs_Tot_Grl_Secondary_LA_Previous_Pct = Abs_Tot_Grl_Secondary_LA_Previous_Pct,
            Abs_Tot_Grl_Secondary_LA_Previous2_Pct = Abs_Tot_Grl_Secondary_LA_Previous2_Pct,
            Abs_Tot_Dis_Primary_LA_Current_Pct = Abs_Tot_Dis_Primary_LA_Current_Pct,
            Abs_Tot_Dis_Primary_LA_Previous_Pct = Abs_Tot_Dis_Primary_LA_Previous_Pct,
            Abs_Tot_Dis_Primary_LA_Previous2_Pct = Abs_Tot_Dis_Primary_LA_Previous2_Pct,
            Abs_Tot_Dis_Secondary_LA_Current_Pct = Abs_Tot_Dis_Secondary_LA_Current_Pct,
            Abs_Tot_Dis_Secondary_LA_Previous_Pct = Abs_Tot_Dis_Secondary_LA_Previous_Pct,
            Abs_Tot_Dis_Secondary_LA_Previous2_Pct = Abs_Tot_Dis_Secondary_LA_Previous2_Pct,
            Abs_Tot_NDi_Primary_LA_Current_Pct = Abs_Tot_NDi_Primary_LA_Current_Pct,
            Abs_Tot_NDi_Primary_LA_Previous_Pct = Abs_Tot_NDi_Primary_LA_Previous_Pct,
            Abs_Tot_NDi_Primary_LA_Previous2_Pct = Abs_Tot_NDi_Primary_LA_Previous2_Pct,
            Abs_Tot_NDi_Secondary_LA_Current_Pct = Abs_Tot_NDi_Secondary_LA_Current_Pct,
            Abs_Tot_NDi_Secondary_LA_Previous_Pct = Abs_Tot_NDi_Secondary_LA_Previous_Pct,
            Abs_Tot_NDi_Secondary_LA_Previous2_Pct = Abs_Tot_NDi_Secondary_LA_Previous2_Pct,
            Abs_Tot_EAL_Primary_LA_Current_Pct = Abs_Tot_EAL_Primary_LA_Current_Pct,
            Abs_Tot_EAL_Primary_LA_Previous_Pct = Abs_Tot_EAL_Primary_LA_Previous_Pct,
            Abs_Tot_EAL_Primary_LA_Previous2_Pct = Abs_Tot_EAL_Primary_LA_Previous2_Pct,
            Abs_Tot_EAL_Secondary_LA_Current_Pct = Abs_Tot_EAL_Secondary_LA_Current_Pct,
            Abs_Tot_EAL_Secondary_LA_Previous_Pct = Abs_Tot_EAL_Secondary_LA_Previous_Pct,
            Abs_Tot_EAL_Secondary_LA_Previous2_Pct = Abs_Tot_EAL_Secondary_LA_Previous2_Pct,
            Abs_Tot_EFL_Primary_LA_Current_Pct = Abs_Tot_EFL_Primary_LA_Current_Pct,
            Abs_Tot_EFL_Primary_LA_Previous_Pct = Abs_Tot_EFL_Primary_LA_Previous_Pct,
            Abs_Tot_EFL_Primary_LA_Previous2_Pct = Abs_Tot_EFL_Primary_LA_Previous2_Pct,
            Abs_Tot_EFL_Secondary_LA_Current_Pct = Abs_Tot_EFL_Secondary_LA_Current_Pct,
            Abs_Tot_EFL_Secondary_LA_Previous_Pct = Abs_Tot_EFL_Secondary_LA_Previous_Pct,
            Abs_Tot_EFL_Secondary_LA_Previous2_Pct = Abs_Tot_EFL_Secondary_LA_Previous2_Pct,
            Abs_Persistent_Boy_Primary_LA_Current_Pct = Abs_Persistent_Boy_Primary_LA_Current_Pct,
            Abs_Persistent_Boy_Primary_LA_Previous_Pct = Abs_Persistent_Boy_Primary_LA_Previous_Pct,
            Abs_Persistent_Boy_Primary_LA_Previous2_Pct = Abs_Persistent_Boy_Primary_LA_Previous2_Pct,
            Abs_Persistent_Boy_Secondary_LA_Current_Pct = Abs_Persistent_Boy_Secondary_LA_Current_Pct,
            Abs_Persistent_Boy_Secondary_LA_Previous_Pct = Abs_Persistent_Boy_Secondary_LA_Previous_Pct,
            Abs_Persistent_Boy_Secondary_LA_Previous2_Pct = Abs_Persistent_Boy_Secondary_LA_Previous2_Pct,
            Abs_Persistent_Grl_Primary_LA_Current_Pct = Abs_Persistent_Grl_Primary_LA_Current_Pct,
            Abs_Persistent_Grl_Primary_LA_Previous_Pct = Abs_Persistent_Grl_Primary_LA_Previous_Pct,
            Abs_Persistent_Grl_Primary_LA_Previous2_Pct = Abs_Persistent_Grl_Primary_LA_Previous2_Pct,
            Abs_Persistent_Grl_Secondary_LA_Current_Pct = Abs_Persistent_Grl_Secondary_LA_Current_Pct,
            Abs_Persistent_Grl_Secondary_LA_Previous_Pct = Abs_Persistent_Grl_Secondary_LA_Previous_Pct,
            Abs_Persistent_Grl_Secondary_LA_Previous2_Pct = Abs_Persistent_Grl_Secondary_LA_Previous2_Pct,
            Abs_Persistent_Dis_Primary_LA_Current_Pct = Abs_Persistent_Dis_Primary_LA_Current_Pct,
            Abs_Persistent_Dis_Primary_LA_Previous_Pct = Abs_Persistent_Dis_Primary_LA_Previous_Pct,
            Abs_Persistent_Dis_Primary_LA_Previous2_Pct = Abs_Persistent_Dis_Primary_LA_Previous2_Pct,
            Abs_Persistent_Dis_Secondary_LA_Current_Pct = Abs_Persistent_Dis_Secondary_LA_Current_Pct,
            Abs_Persistent_Dis_Secondary_LA_Previous_Pct = Abs_Persistent_Dis_Secondary_LA_Previous_Pct,
            Abs_Persistent_Dis_Secondary_LA_Previous2_Pct = Abs_Persistent_Dis_Secondary_LA_Previous2_Pct,
            Abs_Persistent_NDi_Primary_LA_Current_Pct = Abs_Persistent_NDi_Primary_LA_Current_Pct,
            Abs_Persistent_NDi_Primary_LA_Previous_Pct = Abs_Persistent_NDi_Primary_LA_Previous_Pct,
            Abs_Persistent_NDi_Primary_LA_Previous2_Pct = Abs_Persistent_NDi_Primary_LA_Previous2_Pct,
            Abs_Persistent_NDi_Secondary_LA_Current_Pct = Abs_Persistent_NDi_Secondary_LA_Current_Pct,
            Abs_Persistent_NDi_Secondary_LA_Previous_Pct = Abs_Persistent_NDi_Secondary_LA_Previous_Pct,
            Abs_Persistent_NDi_Secondary_LA_Previous2_Pct = Abs_Persistent_NDi_Secondary_LA_Previous2_Pct,
            Abs_Persistent_EAL_Primary_LA_Current_Pct = Abs_Persistent_EAL_Primary_LA_Current_Pct,
            Abs_Persistent_EAL_Primary_LA_Previous_Pct = Abs_Persistent_EAL_Primary_LA_Previous_Pct,
            Abs_Persistent_EAL_Primary_LA_Previous2_Pct = Abs_Persistent_EAL_Primary_LA_Previous2_Pct,
            Abs_Persistent_EAL_Secondary_LA_Current_Pct = Abs_Persistent_EAL_Secondary_LA_Current_Pct,
            Abs_Persistent_EAL_Secondary_LA_Previous_Pct = Abs_Persistent_EAL_Secondary_LA_Previous_Pct,
            Abs_Persistent_EAL_Secondary_LA_Previous2_Pct = Abs_Persistent_EAL_Secondary_LA_Previous2_Pct,
            Abs_Persistent_EFL_Primary_LA_Current_Pct = Abs_Persistent_EFL_Primary_LA_Current_Pct,
            Abs_Persistent_EFL_Primary_LA_Previous_Pct = Abs_Persistent_EFL_Primary_LA_Previous_Pct,
            Abs_Persistent_EFL_Primary_LA_Previous2_Pct = Abs_Persistent_EFL_Primary_LA_Previous2_Pct,
            Abs_Persistent_EFL_Secondary_LA_Current_Pct = Abs_Persistent_EFL_Secondary_LA_Current_Pct,
            Abs_Persistent_EFL_Secondary_LA_Previous_Pct = Abs_Persistent_EFL_Secondary_LA_Previous_Pct,
            Abs_Persistent_EFL_Secondary_LA_Previous2_Pct = Abs_Persistent_EFL_Secondary_LA_Previous2_Pct,
        };
}