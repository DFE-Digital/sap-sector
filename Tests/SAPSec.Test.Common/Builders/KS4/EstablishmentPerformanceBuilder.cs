using SAPSec.Data.Dto.KS4.Performance;

namespace SAPSec.Test.Common.Builders.KS4;

public class EstablishmentPerformanceBuilder(string urn)
{
    string Attainment8_Tot_Est_Current_Num = string.Empty;
    string Attainment8_Tot_Est_Previous_Num = string.Empty;
    string Attainment8_Tot_Est_Previous2_Num = string.Empty;
    string EngMaths49_Tot_Est_Current_Pct = string.Empty;
    string EngMaths49_Tot_Est_Previous_Pct = string.Empty;
    string EngMaths49_Tot_Est_Previous2_Pct = string.Empty;
    string EngMaths59_Tot_Est_Current_Pct = string.Empty;
    string EngMaths59_Tot_Est_Previous_Pct = string.Empty;
    string EngMaths59_Tot_Est_Previous2_Pct = string.Empty;
    string EngLang49_Sum_Est_Current_Pct = string.Empty;
    string EngLang49_Sum_Est_Previous_Pct = string.Empty;
    string EngLang49_Sum_Est_Previous2_Pct = string.Empty;
    string EngLang59_Sum_Est_Current_Pct = string.Empty;
    string EngLang59_Sum_Est_Previous_Pct = string.Empty;
    string EngLang59_Sum_Est_Previous2_Pct = string.Empty;
    string EngLang79_Sum_Est_Current_Pct = string.Empty;
    string EngLang79_Sum_Est_Previous_Pct = string.Empty;
    string EngLang79_Sum_Est_Previous2_Pct = string.Empty;
    string EngLit49_Sum_Est_Current_Pct = string.Empty;
    string EngLit49_Sum_Est_Previous_Pct = string.Empty;
    string EngLit49_Sum_Est_Previous2_Pct = string.Empty;
    string EngLit59_Sum_Est_Current_Pct = string.Empty;
    string EngLit59_Sum_Est_Previous_Pct = string.Empty;
    string EngLit59_Sum_Est_Previous2_Pct = string.Empty;
    string EngLit79_Sum_Est_Current_Pct = string.Empty;
    string EngLit79_Sum_Est_Previous_Pct = string.Empty;
    string EngLit79_Sum_Est_Previous2_Pct = string.Empty;
    string Maths49_Sum_Est_Current_Pct = string.Empty;
    string Maths49_Sum_Est_Previous_Pct = string.Empty;
    string Maths49_Sum_Est_Previous2_Pct = string.Empty;
    string Maths59_Sum_Est_Current_Pct = string.Empty;
    string Maths59_Sum_Est_Previous_Pct = string.Empty;
    string Maths59_Sum_Est_Previous2_Pct = string.Empty;
    string Maths79_Sum_Est_Current_Pct = string.Empty;
    string Maths79_Sum_Est_Previous_Pct = string.Empty;
    string Maths79_Sum_Est_Previous2_Pct = string.Empty;
    string CombSci49_Sum_Est_Current_Pct = string.Empty;
    string CombSci49_Sum_Est_Previous_Pct = string.Empty;
    string CombSci49_Sum_Est_Previous2_Pct = string.Empty;
    string CombSci59_Sum_Est_Current_Pct = string.Empty;
    string CombSci59_Sum_Est_Previous_Pct = string.Empty;
    string CombSci59_Sum_Est_Previous2_Pct = string.Empty;
    string CombSci79_Sum_Est_Current_Pct = string.Empty;
    string CombSci79_Sum_Est_Previous_Pct = string.Empty;
    string CombSci79_Sum_Est_Previous2_Pct = string.Empty;
    string Bio49_Sum_Est_Current_Pct = string.Empty;
    string Bio49_Sum_Est_Previous_Pct = string.Empty;
    string Bio49_Sum_Est_Previous2_Pct = string.Empty;
    string Bio59_Sum_Est_Current_Pct = string.Empty;
    string Bio59_Sum_Est_Previous_Pct = string.Empty;
    string Bio59_Sum_Est_Previous2_Pct = string.Empty;
    string Bio79_Sum_Est_Current_Pct = string.Empty;
    string Bio79_Sum_Est_Previous_Pct = string.Empty;
    string Bio79_Sum_Est_Previous2_Pct = string.Empty;
    string Chem49_Sum_Est_Current_Pct = string.Empty;
    string Chem49_Sum_Est_Previous_Pct = string.Empty;
    string Chem49_Sum_Est_Previous2_Pct = string.Empty;
    string Chem59_Sum_Est_Current_Pct = string.Empty;
    string Chem59_Sum_Est_Previous_Pct = string.Empty;
    string Chem59_Sum_Est_Previous2_Pct = string.Empty;
    string Chem79_Sum_Est_Current_Pct = string.Empty;
    string Chem79_Sum_Est_Previous_Pct = string.Empty;
    string Chem79_Sum_Est_Previous2_Pct = string.Empty;
    string Physics49_Sum_Est_Current_Pct = string.Empty;
    string Physics49_Sum_Est_Previous_Pct = string.Empty;
    string Physics49_Sum_Est_Previous2_Pct = string.Empty;
    string Physics59_Sum_Est_Current_Pct = string.Empty;
    string Physics59_Sum_Est_Previous_Pct = string.Empty;
    string Physics59_Sum_Est_Previous2_Pct = string.Empty;
    string Physics79_Sum_Est_Current_Pct = string.Empty;
    string Physics79_Sum_Est_Previous_Pct = string.Empty;
    string Physics79_Sum_Est_Previous2_Pct = string.Empty;

    public EstablishmentPerformanceBuilder WithAttainment8(string current, string prev, string prev2)
    {
        Attainment8_Tot_Est_Current_Num = current;
        Attainment8_Tot_Est_Previous_Num = prev;
        Attainment8_Tot_Est_Previous2_Num = prev2;

        return this;
    }

    public EstablishmentPerformanceBuilder WithEngMaths49(string current, string prev, string prev2)
    {
        EngMaths49_Tot_Est_Current_Pct = current;
        EngMaths49_Tot_Est_Previous_Pct = prev;
        EngMaths49_Tot_Est_Previous2_Pct = prev2;

        return this;
    }

    public EstablishmentPerformanceBuilder WithEngMaths59(string current, string prev, string prev2)
    {
        EngMaths59_Tot_Est_Current_Pct = current;
        EngMaths59_Tot_Est_Previous_Pct = prev;
        EngMaths59_Tot_Est_Previous2_Pct = prev2;

        return this;
    }

    public EstablishmentPerformanceBuilder WithEngLang49(string current, string prev, string prev2)
    {
        EngLang49_Sum_Est_Current_Pct = current;
        EngLang49_Sum_Est_Previous_Pct = prev;
        EngLang49_Sum_Est_Previous2_Pct = prev2;

        return this;
    }

    public EstablishmentPerformanceBuilder WithEngLang59(string current, string prev, string prev2)
    {
        EngLang59_Sum_Est_Current_Pct = current;
        EngLang59_Sum_Est_Previous_Pct = prev;
        EngLang59_Sum_Est_Previous2_Pct = prev2;

        return this;
    }

    public EstablishmentPerformanceBuilder WithEngLang79(string current, string prev, string prev2)
    {
        EngLang79_Sum_Est_Current_Pct = current;
        EngLang79_Sum_Est_Previous_Pct = prev;
        EngLang79_Sum_Est_Previous2_Pct = prev2;

        return this;
    }

    public EstablishmentPerformanceBuilder WithEngLit49(string current, string prev, string prev2)
    {
        EngLit49_Sum_Est_Current_Pct = current;
        EngLit49_Sum_Est_Previous_Pct = prev;
        EngLit49_Sum_Est_Previous2_Pct = prev2;

        return this;
    }

    public EstablishmentPerformanceBuilder WithEngLit59(string current, string prev, string prev2)
    {
        EngLit59_Sum_Est_Current_Pct = current;
        EngLit59_Sum_Est_Previous_Pct = prev;
        EngLit59_Sum_Est_Previous2_Pct = prev2;

        return this;
    }

    public EstablishmentPerformanceBuilder WithEngLit79(string current, string prev, string prev2)
    {
        EngLit79_Sum_Est_Current_Pct = current;
        EngLit79_Sum_Est_Previous_Pct = prev;
        EngLit79_Sum_Est_Previous2_Pct = prev2;

        return this;
    }

    public EstablishmentPerformanceBuilder WithMaths49(string current, string prev, string prev2)
    {
        Maths49_Sum_Est_Current_Pct = current;
        Maths49_Sum_Est_Previous_Pct = prev;
        Maths49_Sum_Est_Previous2_Pct = prev2;

        return this;
    }

    public EstablishmentPerformanceBuilder WithMaths59(string current, string prev, string prev2)
    {
        Maths59_Sum_Est_Current_Pct = current;
        Maths59_Sum_Est_Previous_Pct = prev;
        Maths59_Sum_Est_Previous2_Pct = prev2;

        return this;
    }

    public EstablishmentPerformanceBuilder WithMaths79(string current, string prev, string prev2)
    {
        Maths79_Sum_Est_Current_Pct = current;
        Maths79_Sum_Est_Previous_Pct = prev;
        Maths79_Sum_Est_Previous2_Pct = prev2;

        return this;
    }

    public EstablishmentPerformanceBuilder WithCombSci49(string current, string prev, string prev2)
    {
        CombSci49_Sum_Est_Current_Pct = current;
        CombSci49_Sum_Est_Previous_Pct = prev;
        CombSci49_Sum_Est_Previous2_Pct = prev2;

        return this;
    }

    public EstablishmentPerformanceBuilder WithCombSci59(string current, string prev, string prev2)
    {
        CombSci59_Sum_Est_Current_Pct = current;
        CombSci59_Sum_Est_Previous_Pct = prev;
        CombSci59_Sum_Est_Previous2_Pct = prev2;

        return this;
    }

    public EstablishmentPerformanceBuilder WithCombSci79(string current, string prev, string prev2)
    {
        CombSci79_Sum_Est_Current_Pct = current;
        CombSci79_Sum_Est_Previous_Pct = prev;
        CombSci79_Sum_Est_Previous2_Pct = prev2;

        return this;
    }

    public EstablishmentPerformanceBuilder WithBio49(string current, string prev, string prev2)
    {
        Bio49_Sum_Est_Current_Pct = current;
        Bio49_Sum_Est_Previous_Pct = prev;
        Bio49_Sum_Est_Previous2_Pct = prev2;

        return this;
    }

    public EstablishmentPerformanceBuilder WithBio59(string current, string prev, string prev2)
    {
        Bio59_Sum_Est_Current_Pct = current;
        Bio59_Sum_Est_Previous_Pct = prev;
        Bio59_Sum_Est_Previous2_Pct = prev2;

        return this;
    }

    public EstablishmentPerformanceBuilder WithBio79(string current, string prev, string prev2)
    {
        Bio79_Sum_Est_Current_Pct = current;
        Bio79_Sum_Est_Previous_Pct = prev;
        Bio79_Sum_Est_Previous2_Pct = prev2;

        return this;
    }

    public EstablishmentPerformanceBuilder WithChem49(string current, string prev, string prev2)
    {
        Chem49_Sum_Est_Current_Pct = current;
        Chem49_Sum_Est_Previous_Pct = prev;
        Chem49_Sum_Est_Previous2_Pct = prev2;

        return this;
    }

    public EstablishmentPerformanceBuilder WithChem59(string current, string prev, string prev2)
    {
        Chem59_Sum_Est_Current_Pct = current;
        Chem59_Sum_Est_Previous_Pct = prev;
        Chem59_Sum_Est_Previous2_Pct = prev2;

        return this;
    }

    public EstablishmentPerformanceBuilder WithChem79(string current, string prev, string prev2)
    {
        Chem79_Sum_Est_Current_Pct = current;
        Chem79_Sum_Est_Previous_Pct = prev;
        Chem79_Sum_Est_Previous2_Pct = prev2;

        return this;
    }

    public EstablishmentPerformanceBuilder WithPhysics49(string current, string prev, string prev2)
    {
        Physics49_Sum_Est_Current_Pct = current;
        Physics49_Sum_Est_Previous_Pct = prev;
        Physics49_Sum_Est_Previous2_Pct = prev2;

        return this;
    }

    public EstablishmentPerformanceBuilder WithPhysics59(string current, string prev, string prev2)
    {
        Physics59_Sum_Est_Current_Pct = current;
        Physics59_Sum_Est_Previous_Pct = prev;
        Physics59_Sum_Est_Previous2_Pct = prev2;

        return this;
    }

    public EstablishmentPerformanceBuilder WithPhysics79(string current, string prev, string prev2)
    {
        Physics79_Sum_Est_Current_Pct = current;
        Physics79_Sum_Est_Previous_Pct = prev;
        Physics79_Sum_Est_Previous2_Pct = prev2;

        return this;
    }

    public EstablishmentPerformance Build() =>
        new()
        {
            Id = urn,
            Attainment8_Tot_Est_Current_Num = Attainment8_Tot_Est_Current_Num,
            Attainment8_Tot_Est_Previous_Num = Attainment8_Tot_Est_Previous_Num,
            Attainment8_Tot_Est_Previous2_Num = Attainment8_Tot_Est_Previous2_Num,
            EngMaths49_Tot_Est_Current_Pct = EngMaths49_Tot_Est_Current_Pct,
            EngMaths49_Tot_Est_Previous_Pct = EngMaths49_Tot_Est_Previous_Pct,
            EngMaths49_Tot_Est_Previous2_Pct = EngMaths49_Tot_Est_Previous2_Pct,
            EngMaths59_Tot_Est_Current_Pct = EngMaths59_Tot_Est_Current_Pct,
            EngMaths59_Tot_Est_Previous_Pct = EngMaths59_Tot_Est_Previous_Pct,
            EngMaths59_Tot_Est_Previous2_Pct = EngMaths59_Tot_Est_Previous2_Pct,
            EngLang49_Sum_Est_Current_Pct = EngLang49_Sum_Est_Current_Pct,
            EngLang49_Sum_Est_Previous_Pct = EngLang49_Sum_Est_Previous_Pct,
            EngLang49_Sum_Est_Previous2_Pct = EngLang49_Sum_Est_Previous2_Pct,
            EngLang59_Sum_Est_Current_Pct = EngLang59_Sum_Est_Current_Pct,
            EngLang59_Sum_Est_Previous_Pct = EngLang59_Sum_Est_Previous_Pct,
            EngLang59_Sum_Est_Previous2_Pct = EngLang59_Sum_Est_Previous2_Pct,
            EngLang79_Sum_Est_Current_Pct = EngLang79_Sum_Est_Current_Pct,
            EngLang79_Sum_Est_Previous_Pct = EngLang79_Sum_Est_Previous_Pct,
            EngLang79_Sum_Est_Previous2_Pct = EngLang79_Sum_Est_Previous2_Pct,
            EngLit49_Sum_Est_Current_Pct = EngLit49_Sum_Est_Current_Pct,
            EngLit49_Sum_Est_Previous_Pct = EngLit49_Sum_Est_Previous_Pct,
            EngLit49_Sum_Est_Previous2_Pct = EngLit49_Sum_Est_Previous2_Pct,
            EngLit59_Sum_Est_Current_Pct = EngLit59_Sum_Est_Current_Pct,
            EngLit59_Sum_Est_Previous_Pct = EngLit59_Sum_Est_Previous_Pct,
            EngLit59_Sum_Est_Previous2_Pct = EngLit59_Sum_Est_Previous2_Pct,
            EngLit79_Sum_Est_Current_Pct = EngLit79_Sum_Est_Current_Pct,
            EngLit79_Sum_Est_Previous_Pct = EngLit79_Sum_Est_Previous_Pct,
            EngLit79_Sum_Est_Previous2_Pct = EngLit79_Sum_Est_Previous2_Pct,
            Maths49_Sum_Est_Current_Pct = Maths49_Sum_Est_Current_Pct,
            Maths49_Sum_Est_Previous_Pct = Maths49_Sum_Est_Previous_Pct,
            Maths49_Sum_Est_Previous2_Pct = Maths49_Sum_Est_Previous2_Pct,
            Maths59_Sum_Est_Current_Pct = Maths59_Sum_Est_Current_Pct,
            Maths59_Sum_Est_Previous_Pct = Maths59_Sum_Est_Previous_Pct,
            Maths59_Sum_Est_Previous2_Pct = Maths59_Sum_Est_Previous2_Pct,
            Maths79_Sum_Est_Current_Pct = Maths79_Sum_Est_Current_Pct,
            Maths79_Sum_Est_Previous_Pct = Maths79_Sum_Est_Previous_Pct,
            Maths79_Sum_Est_Previous2_Pct = Maths79_Sum_Est_Previous2_Pct,
            CombSci49_Sum_Est_Current_Pct = CombSci49_Sum_Est_Current_Pct,
            CombSci49_Sum_Est_Previous_Pct = CombSci49_Sum_Est_Previous_Pct,
            CombSci49_Sum_Est_Previous2_Pct = CombSci49_Sum_Est_Previous2_Pct,
            CombSci59_Sum_Est_Current_Pct = CombSci59_Sum_Est_Current_Pct,
            CombSci59_Sum_Est_Previous_Pct = CombSci59_Sum_Est_Previous_Pct,
            CombSci59_Sum_Est_Previous2_Pct = CombSci59_Sum_Est_Previous2_Pct,
            CombSci79_Sum_Est_Current_Pct = CombSci79_Sum_Est_Current_Pct,
            CombSci79_Sum_Est_Previous_Pct = CombSci79_Sum_Est_Previous_Pct,
            CombSci79_Sum_Est_Previous2_Pct = CombSci79_Sum_Est_Previous2_Pct,
            Bio49_Sum_Est_Current_Pct = Bio49_Sum_Est_Current_Pct,
            Bio49_Sum_Est_Previous_Pct = Bio49_Sum_Est_Previous_Pct,
            Bio49_Sum_Est_Previous2_Pct = Bio49_Sum_Est_Previous2_Pct,
            Bio59_Sum_Est_Current_Pct = Bio59_Sum_Est_Current_Pct,
            Bio59_Sum_Est_Previous_Pct = Bio59_Sum_Est_Previous_Pct,
            Bio59_Sum_Est_Previous2_Pct = Bio59_Sum_Est_Previous2_Pct,
            Bio79_Sum_Est_Current_Pct = Bio79_Sum_Est_Current_Pct,
            Bio79_Sum_Est_Previous_Pct = Bio79_Sum_Est_Previous_Pct,
            Bio79_Sum_Est_Previous2_Pct = Bio79_Sum_Est_Previous2_Pct,
            Chem49_Sum_Est_Current_Pct = Chem49_Sum_Est_Current_Pct,
            Chem49_Sum_Est_Previous_Pct = Chem49_Sum_Est_Previous_Pct,
            Chem49_Sum_Est_Previous2_Pct = Chem49_Sum_Est_Previous2_Pct,
            Chem59_Sum_Est_Current_Pct = Chem59_Sum_Est_Current_Pct,
            Chem59_Sum_Est_Previous_Pct = Chem59_Sum_Est_Previous_Pct,
            Chem59_Sum_Est_Previous2_Pct = Chem59_Sum_Est_Previous2_Pct,
            Chem79_Sum_Est_Current_Pct = Chem79_Sum_Est_Current_Pct,
            Chem79_Sum_Est_Previous_Pct = Chem79_Sum_Est_Previous_Pct,
            Chem79_Sum_Est_Previous2_Pct = Chem79_Sum_Est_Previous2_Pct,
            Physics49_Sum_Est_Current_Pct = Physics49_Sum_Est_Current_Pct,
            Physics49_Sum_Est_Previous_Pct = Physics49_Sum_Est_Previous_Pct,
            Physics49_Sum_Est_Previous2_Pct = Physics49_Sum_Est_Previous2_Pct,
            Physics59_Sum_Est_Current_Pct = Physics59_Sum_Est_Current_Pct,
            Physics59_Sum_Est_Previous_Pct = Physics59_Sum_Est_Previous_Pct,
            Physics59_Sum_Est_Previous2_Pct = Physics59_Sum_Est_Previous2_Pct,
            Physics79_Sum_Est_Current_Pct = Physics79_Sum_Est_Current_Pct,
            Physics79_Sum_Est_Previous_Pct = Physics79_Sum_Est_Previous_Pct,
            Physics79_Sum_Est_Previous2_Pct = Physics79_Sum_Est_Previous2_Pct,
        };
}
