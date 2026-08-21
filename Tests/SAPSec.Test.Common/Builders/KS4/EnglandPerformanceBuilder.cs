using SAPSec.Data.Dto.KS4.Performance;

namespace SAPSec.Test.Common.Builders.KS4;

public class EnglandPerformanceBuilder()
{
    string Attainment8_Tot_Eng_Current_Num = string.Empty;
    string Attainment8_Tot_Eng_Previous_Num = string.Empty;
    string Attainment8_Tot_Eng_Previous2_Num = string.Empty;
    string EngMaths49_Tot_Eng_Current_Pct = string.Empty;
    string EngMaths49_Tot_Eng_Previous_Pct = string.Empty;
    string EngMaths49_Tot_Eng_Previous2_Pct = string.Empty;
    string EngMaths59_Tot_Eng_Current_Pct = string.Empty;
    string EngMaths59_Tot_Eng_Previous_Pct = string.Empty;
    string EngMaths59_Tot_Eng_Previous2_Pct = string.Empty;
    string EngLang49_Tot_Eng_Current_Pct = string.Empty;
    string EngLang49_Tot_Eng_Previous_Pct = string.Empty;
    string EngLang49_Tot_Eng_Previous2_Pct = string.Empty;
    string EngLang59_Tot_Eng_Current_Pct = string.Empty;
    string EngLang59_Tot_Eng_Previous_Pct = string.Empty;
    string EngLang59_Tot_Eng_Previous2_Pct = string.Empty;
    string EngLang79_Tot_Eng_Current_Pct = string.Empty;
    string EngLang79_Tot_Eng_Previous_Pct = string.Empty;
    string EngLang79_Tot_Eng_Previous2_Pct = string.Empty;
    string EngLit49_Tot_Eng_Current_Pct = string.Empty;
    string EngLit49_Tot_Eng_Previous_Pct = string.Empty;
    string EngLit49_Tot_Eng_Previous2_Pct = string.Empty;
    string EngLit59_Tot_Eng_Current_Pct = string.Empty;
    string EngLit59_Tot_Eng_Previous_Pct = string.Empty;
    string EngLit59_Tot_Eng_Previous2_Pct = string.Empty;
    string EngLit79_Tot_Eng_Current_Pct = string.Empty;
    string EngLit79_Tot_Eng_Previous_Pct = string.Empty;
    string EngLit79_Tot_Eng_Previous2_Pct = string.Empty;
    string Maths49_Tot_Eng_Current_Pct = string.Empty;
    string Maths49_Tot_Eng_Previous_Pct = string.Empty;
    string Maths49_Tot_Eng_Previous2_Pct = string.Empty;
    string Maths59_Tot_Eng_Current_Pct = string.Empty;
    string Maths59_Tot_Eng_Previous_Pct = string.Empty;
    string Maths59_Tot_Eng_Previous2_Pct = string.Empty;
    string Maths79_Tot_Eng_Current_Pct = string.Empty;
    string Maths79_Tot_Eng_Previous_Pct = string.Empty;
    string Maths79_Tot_Eng_Previous2_Pct = string.Empty;
    string CombSci49_Tot_Eng_Current_Pct = string.Empty;
    string CombSci49_Tot_Eng_Previous_Pct = string.Empty;
    string CombSci49_Tot_Eng_Previous2_Pct = string.Empty;
    string CombSci59_Tot_Eng_Current_Pct = string.Empty;
    string CombSci59_Tot_Eng_Previous_Pct = string.Empty;
    string CombSci59_Tot_Eng_Previous2_Pct = string.Empty;
    string CombSci79_Tot_Eng_Current_Pct = string.Empty;
    string CombSci79_Tot_Eng_Previous_Pct = string.Empty;
    string CombSci79_Tot_Eng_Previous2_Pct = string.Empty;
    string Bio49_Tot_Eng_Current_Pct = string.Empty;
    string Bio49_Tot_Eng_Previous_Pct = string.Empty;
    string Bio49_Tot_Eng_Previous2_Pct = string.Empty;
    string Bio59_Tot_Eng_Current_Pct = string.Empty;
    string Bio59_Tot_Eng_Previous_Pct = string.Empty;
    string Bio59_Tot_Eng_Previous2_Pct = string.Empty;
    string Bio79_Tot_Eng_Current_Pct = string.Empty;
    string Bio79_Tot_Eng_Previous_Pct = string.Empty;
    string Bio79_Tot_Eng_Previous2_Pct = string.Empty;
    string Chem49_Tot_Eng_Current_Pct = string.Empty;
    string Chem49_Tot_Eng_Previous_Pct = string.Empty;
    string Chem49_Tot_Eng_Previous2_Pct = string.Empty;
    string Chem59_Tot_Eng_Current_Pct = string.Empty;
    string Chem59_Tot_Eng_Previous_Pct = string.Empty;
    string Chem59_Tot_Eng_Previous2_Pct = string.Empty;
    string Chem79_Tot_Eng_Current_Pct = string.Empty;
    string Chem79_Tot_Eng_Previous_Pct = string.Empty;
    string Chem79_Tot_Eng_Previous2_Pct = string.Empty;
    string Physics49_Tot_Eng_Current_Pct = string.Empty;
    string Physics49_Tot_Eng_Previous_Pct = string.Empty;
    string Physics49_Tot_Eng_Previous2_Pct = string.Empty;
    string Physics59_Tot_Eng_Current_Pct = string.Empty;
    string Physics59_Tot_Eng_Previous_Pct = string.Empty;
    string Physics59_Tot_Eng_Previous2_Pct = string.Empty;
    string Physics79_Tot_Eng_Current_Pct = string.Empty;
    string Physics79_Tot_Eng_Previous_Pct = string.Empty;
    string Physics79_Tot_Eng_Previous2_Pct = string.Empty;

    public EnglandPerformanceBuilder WithAttainment8(string current, string prev, string prev2)
    {
        Attainment8_Tot_Eng_Current_Num = current;
        Attainment8_Tot_Eng_Previous_Num = prev;
        Attainment8_Tot_Eng_Previous2_Num = prev2;

        return this;
    }

    public EnglandPerformanceBuilder WithEngMaths49(string current, string prev, string prev2)
    {
        EngMaths49_Tot_Eng_Current_Pct = current;
        EngMaths49_Tot_Eng_Previous_Pct = prev;
        EngMaths49_Tot_Eng_Previous2_Pct = prev2;

        return this;
    }

    public EnglandPerformanceBuilder WithEngMaths59(string current, string prev, string prev2)
    {
        EngMaths59_Tot_Eng_Current_Pct = current;
        EngMaths59_Tot_Eng_Previous_Pct = prev;
        EngMaths59_Tot_Eng_Previous2_Pct = prev2;

        return this;
    }

    public EnglandPerformanceBuilder WithEngLang49(string current, string prev, string prev2)
    {
        EngLang49_Tot_Eng_Current_Pct = current;
        EngLang49_Tot_Eng_Previous_Pct = prev;
        EngLang49_Tot_Eng_Previous2_Pct = prev2;

        return this;
    }

    public EnglandPerformanceBuilder WithEngLang59(string current, string prev, string prev2)
    {
        EngLang59_Tot_Eng_Current_Pct = current;
        EngLang59_Tot_Eng_Previous_Pct = prev;
        EngLang59_Tot_Eng_Previous2_Pct = prev2;

        return this;
    }

    public EnglandPerformanceBuilder WithEngLang79(string current, string prev, string prev2)
    {
        EngLang79_Tot_Eng_Current_Pct = current;
        EngLang79_Tot_Eng_Previous_Pct = prev;
        EngLang79_Tot_Eng_Previous2_Pct = prev2;

        return this;
    }

    public EnglandPerformanceBuilder WithEngLit49(string current, string prev, string prev2)
    {
        EngLit49_Tot_Eng_Current_Pct = current;
        EngLit49_Tot_Eng_Previous_Pct = prev;
        EngLit49_Tot_Eng_Previous2_Pct = prev2;

        return this;
    }

    public EnglandPerformanceBuilder WithEngLit59(string current, string prev, string prev2)
    {
        EngLit59_Tot_Eng_Current_Pct = current;
        EngLit59_Tot_Eng_Previous_Pct = prev;
        EngLit59_Tot_Eng_Previous2_Pct = prev2;

        return this;
    }

    public EnglandPerformanceBuilder WithEngLit79(string current, string prev, string prev2)
    {
        EngLit79_Tot_Eng_Current_Pct = current;
        EngLit79_Tot_Eng_Previous_Pct = prev;
        EngLit79_Tot_Eng_Previous2_Pct = prev2;

        return this;
    }

    public EnglandPerformanceBuilder WithMaths49(string current, string prev, string prev2)
    {
        Maths49_Tot_Eng_Current_Pct = current;
        Maths49_Tot_Eng_Previous_Pct = prev;
        Maths49_Tot_Eng_Previous2_Pct = prev2;

        return this;
    }

    public EnglandPerformanceBuilder WithMaths59(string current, string prev, string prev2)
    {
        Maths59_Tot_Eng_Current_Pct = current;
        Maths59_Tot_Eng_Previous_Pct = prev;
        Maths59_Tot_Eng_Previous2_Pct = prev2;

        return this;
    }

    public EnglandPerformanceBuilder WithMaths79(string current, string prev, string prev2)
    {
        Maths79_Tot_Eng_Current_Pct = current;
        Maths79_Tot_Eng_Previous_Pct = prev;
        Maths79_Tot_Eng_Previous2_Pct = prev2;

        return this;
    }

    public EnglandPerformanceBuilder WithCombSci49(string current, string prev, string prev2)
    {
        CombSci49_Tot_Eng_Current_Pct = current;
        CombSci49_Tot_Eng_Previous_Pct = prev;
        CombSci49_Tot_Eng_Previous2_Pct = prev2;

        return this;
    }

    public EnglandPerformanceBuilder WithCombSci59(string current, string prev, string prev2)
    {
        CombSci59_Tot_Eng_Current_Pct = current;
        CombSci59_Tot_Eng_Previous_Pct = prev;
        CombSci59_Tot_Eng_Previous2_Pct = prev2;

        return this;
    }

    public EnglandPerformanceBuilder WithCombSci79(string current, string prev, string prev2)
    {
        CombSci79_Tot_Eng_Current_Pct = current;
        CombSci79_Tot_Eng_Previous_Pct = prev;
        CombSci79_Tot_Eng_Previous2_Pct = prev2;

        return this;
    }

    public EnglandPerformanceBuilder WithBio49(string current, string prev, string prev2)
    {
        Bio49_Tot_Eng_Current_Pct = current;
        Bio49_Tot_Eng_Previous_Pct = prev;
        Bio49_Tot_Eng_Previous2_Pct = prev2;

        return this;
    }

    public EnglandPerformanceBuilder WithBio59(string current, string prev, string prev2)
    {
        Bio59_Tot_Eng_Current_Pct = current;
        Bio59_Tot_Eng_Previous_Pct = prev;
        Bio59_Tot_Eng_Previous2_Pct = prev2;

        return this;
    }

    public EnglandPerformanceBuilder WithBio79(string current, string prev, string prev2)
    {
        Bio79_Tot_Eng_Current_Pct = current;
        Bio79_Tot_Eng_Previous_Pct = prev;
        Bio79_Tot_Eng_Previous2_Pct = prev2;

        return this;
    }

    public EnglandPerformanceBuilder WithChem49(string current, string prev, string prev2)
    {
        Chem49_Tot_Eng_Current_Pct = current;
        Chem49_Tot_Eng_Previous_Pct = prev;
        Chem49_Tot_Eng_Previous2_Pct = prev2;

        return this;
    }

    public EnglandPerformanceBuilder WithChem59(string current, string prev, string prev2)
    {
        Chem59_Tot_Eng_Current_Pct = current;
        Chem59_Tot_Eng_Previous_Pct = prev;
        Chem59_Tot_Eng_Previous2_Pct = prev2;

        return this;
    }

    public EnglandPerformanceBuilder WithChem79(string current, string prev, string prev2)
    {
        Chem79_Tot_Eng_Current_Pct = current;
        Chem79_Tot_Eng_Previous_Pct = prev;
        Chem79_Tot_Eng_Previous2_Pct = prev2;

        return this;
    }

    public EnglandPerformanceBuilder WithPhysics49(string current, string prev, string prev2)
    {
        Physics49_Tot_Eng_Current_Pct = current;
        Physics49_Tot_Eng_Previous_Pct = prev;
        Physics49_Tot_Eng_Previous2_Pct = prev2;

        return this;
    }

    public EnglandPerformanceBuilder WithPhysics59(string current, string prev, string prev2)
    {
        Physics59_Tot_Eng_Current_Pct = current;
        Physics59_Tot_Eng_Previous_Pct = prev;
        Physics59_Tot_Eng_Previous2_Pct = prev2;

        return this;
    }

    public EnglandPerformanceBuilder WithPhysics79(string current, string prev, string prev2)
    {
        Physics79_Tot_Eng_Current_Pct = current;
        Physics79_Tot_Eng_Previous_Pct = prev;
        Physics79_Tot_Eng_Previous2_Pct = prev2;

        return this;
    }

    public EnglandPerformance Build() =>
        new()
        {
            Id = "National",
            Attainment8_Tot_Eng_Current_Num = Attainment8_Tot_Eng_Current_Num,
            Attainment8_Tot_Eng_Previous_Num = Attainment8_Tot_Eng_Previous_Num,
            Attainment8_Tot_Eng_Previous2_Num = Attainment8_Tot_Eng_Previous2_Num,
            EngMaths49_Tot_Eng_Current_Pct = EngMaths49_Tot_Eng_Current_Pct,
            EngMaths49_Tot_Eng_Previous_Pct = EngMaths49_Tot_Eng_Previous_Pct,
            EngMaths49_Tot_Eng_Previous2_Pct = EngMaths49_Tot_Eng_Previous2_Pct,
            EngMaths59_Tot_Eng_Current_Pct = EngMaths59_Tot_Eng_Current_Pct,
            EngMaths59_Tot_Eng_Previous_Pct = EngMaths59_Tot_Eng_Previous_Pct,
            EngMaths59_Tot_Eng_Previous2_Pct = EngMaths59_Tot_Eng_Previous2_Pct,
            EngLang49_Tot_Eng_Current_Pct = EngLang49_Tot_Eng_Current_Pct,
            EngLang49_Tot_Eng_Previous_Pct = EngLang49_Tot_Eng_Previous_Pct,
            EngLang49_Tot_Eng_Previous2_Pct = EngLang49_Tot_Eng_Previous2_Pct,
            EngLang59_Tot_Eng_Current_Pct = EngLang59_Tot_Eng_Current_Pct,
            EngLang59_Tot_Eng_Previous_Pct = EngLang59_Tot_Eng_Previous_Pct,
            EngLang59_Tot_Eng_Previous2_Pct = EngLang59_Tot_Eng_Previous2_Pct,
            EngLang79_Tot_Eng_Current_Pct = EngLang79_Tot_Eng_Current_Pct,
            EngLang79_Tot_Eng_Previous_Pct = EngLang79_Tot_Eng_Previous_Pct,
            EngLang79_Tot_Eng_Previous2_Pct = EngLang79_Tot_Eng_Previous2_Pct,
            EngLit49_Tot_Eng_Current_Pct = EngLit49_Tot_Eng_Current_Pct,
            EngLit49_Tot_Eng_Previous_Pct = EngLit49_Tot_Eng_Previous_Pct,
            EngLit49_Tot_Eng_Previous2_Pct = EngLit49_Tot_Eng_Previous2_Pct,
            EngLit59_Tot_Eng_Current_Pct = EngLit59_Tot_Eng_Current_Pct,
            EngLit59_Tot_Eng_Previous_Pct = EngLit59_Tot_Eng_Previous_Pct,
            EngLit59_Tot_Eng_Previous2_Pct = EngLit59_Tot_Eng_Previous2_Pct,
            EngLit79_Tot_Eng_Current_Pct = EngLit79_Tot_Eng_Current_Pct,
            EngLit79_Tot_Eng_Previous_Pct = EngLit79_Tot_Eng_Previous_Pct,
            EngLit79_Tot_Eng_Previous2_Pct = EngLit79_Tot_Eng_Previous2_Pct,
            Maths49_Tot_Eng_Current_Pct = Maths49_Tot_Eng_Current_Pct,
            Maths49_Tot_Eng_Previous_Pct = Maths49_Tot_Eng_Previous_Pct,
            Maths49_Tot_Eng_Previous2_Pct = Maths49_Tot_Eng_Previous2_Pct,
            Maths59_Tot_Eng_Current_Pct = Maths59_Tot_Eng_Current_Pct,
            Maths59_Tot_Eng_Previous_Pct = Maths59_Tot_Eng_Previous_Pct,
            Maths59_Tot_Eng_Previous2_Pct = Maths59_Tot_Eng_Previous2_Pct,
            Maths79_Tot_Eng_Current_Pct = Maths79_Tot_Eng_Current_Pct,
            Maths79_Tot_Eng_Previous_Pct = Maths79_Tot_Eng_Previous_Pct,
            Maths79_Tot_Eng_Previous2_Pct = Maths79_Tot_Eng_Previous2_Pct,
            CombSci49_Tot_Eng_Current_Pct = CombSci49_Tot_Eng_Current_Pct,
            CombSci49_Tot_Eng_Previous_Pct = CombSci49_Tot_Eng_Previous_Pct,
            CombSci49_Tot_Eng_Previous2_Pct = CombSci49_Tot_Eng_Previous2_Pct,
            CombSci59_Tot_Eng_Current_Pct = CombSci59_Tot_Eng_Current_Pct,
            CombSci59_Tot_Eng_Previous_Pct = CombSci59_Tot_Eng_Previous_Pct,
            CombSci59_Tot_Eng_Previous2_Pct = CombSci59_Tot_Eng_Previous2_Pct,
            CombSci79_Tot_Eng_Current_Pct = CombSci79_Tot_Eng_Current_Pct,
            CombSci79_Tot_Eng_Previous_Pct = CombSci79_Tot_Eng_Previous_Pct,
            CombSci79_Tot_Eng_Previous2_Pct = CombSci79_Tot_Eng_Previous2_Pct,
            Bio49_Tot_Eng_Current_Pct = Bio49_Tot_Eng_Current_Pct,
            Bio49_Tot_Eng_Previous_Pct = Bio49_Tot_Eng_Previous_Pct,
            Bio49_Tot_Eng_Previous2_Pct = Bio49_Tot_Eng_Previous2_Pct,
            Bio59_Tot_Eng_Current_Pct = Bio59_Tot_Eng_Current_Pct,
            Bio59_Tot_Eng_Previous_Pct = Bio59_Tot_Eng_Previous_Pct,
            Bio59_Tot_Eng_Previous2_Pct = Bio59_Tot_Eng_Previous2_Pct,
            Bio79_Tot_Eng_Current_Pct = Bio79_Tot_Eng_Current_Pct,
            Bio79_Tot_Eng_Previous_Pct = Bio79_Tot_Eng_Previous_Pct,
            Bio79_Tot_Eng_Previous2_Pct = Bio79_Tot_Eng_Previous2_Pct,
            Chem49_Tot_Eng_Current_Pct = Chem49_Tot_Eng_Current_Pct,
            Chem49_Tot_Eng_Previous_Pct = Chem49_Tot_Eng_Previous_Pct,
            Chem49_Tot_Eng_Previous2_Pct = Chem49_Tot_Eng_Previous2_Pct,
            Chem59_Tot_Eng_Current_Pct = Chem59_Tot_Eng_Current_Pct,
            Chem59_Tot_Eng_Previous_Pct = Chem59_Tot_Eng_Previous_Pct,
            Chem59_Tot_Eng_Previous2_Pct = Chem59_Tot_Eng_Previous2_Pct,
            Chem79_Tot_Eng_Current_Pct = Chem79_Tot_Eng_Current_Pct,
            Chem79_Tot_Eng_Previous_Pct = Chem79_Tot_Eng_Previous_Pct,
            Chem79_Tot_Eng_Previous2_Pct = Chem79_Tot_Eng_Previous2_Pct,
            Physics49_Tot_Eng_Current_Pct = Physics49_Tot_Eng_Current_Pct,
            Physics49_Tot_Eng_Previous_Pct = Physics49_Tot_Eng_Previous_Pct,
            Physics49_Tot_Eng_Previous2_Pct = Physics49_Tot_Eng_Previous2_Pct,
            Physics59_Tot_Eng_Current_Pct = Physics59_Tot_Eng_Current_Pct,
            Physics59_Tot_Eng_Previous_Pct = Physics59_Tot_Eng_Previous_Pct,
            Physics59_Tot_Eng_Previous2_Pct = Physics59_Tot_Eng_Previous2_Pct,
            Physics79_Tot_Eng_Current_Pct = Physics79_Tot_Eng_Current_Pct,
            Physics79_Tot_Eng_Previous_Pct = Physics79_Tot_Eng_Previous_Pct,
            Physics79_Tot_Eng_Previous2_Pct = Physics79_Tot_Eng_Previous2_Pct,
        };
}
