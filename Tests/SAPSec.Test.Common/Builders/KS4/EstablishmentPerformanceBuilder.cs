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
        };
}
