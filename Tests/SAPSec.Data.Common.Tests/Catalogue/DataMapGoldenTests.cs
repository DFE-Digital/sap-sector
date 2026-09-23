using SAPSec.Data.Common.Catalogue;
using SAPSec.Data.Common.Catalogue.Definitions;

namespace SAPSec.Data.Common.Tests.Catalogue;

/// <summary>
/// Proves each dataset defined in code produces the same materialized views as its datamap.csv rows, apart from
/// deliberate data fixes listed here. Remove once datamap.csv is deleted (Story 3).
/// </summary>
public class DataMapGoldenTests
{
    private static readonly Dictionary<string, string[]> IntendedChanges = new()
    {
        [Ks4Performance.Type] =
        [
            // LA Progress 8: the CSV had one mislabelled property reading every breakdown topic for 2024-25.
            // Replaced by one property per year filtered to breakdown_topic = Total.
            "KS4_Performance/LA.Prog8_Avg_LA_Previous2_Num",
            "KS4_Performance/LA.Prog8_Tot_LA_Current_Num",
            "KS4_Performance/LA.Prog8_Tot_LA_Previous_Num",
            "KS4_Performance/LA.Prog8_Tot_LA_Previous2_Num",

            // England mobile pupils, English and maths grade 5+: read the grade 5+ column, not grade 4+.
            "KS4_Performance/England.EngMaths59_Mob_Eng_Current_Num",
            "KS4_Performance/England.EngMaths59_Mob_Eng_Previous_Num",
            "KS4_Performance/England.EngMaths59_Mob_Eng_Previous2_Num",
            "KS4_Performance/England.EngMaths59_Mob_Eng_Current_Pct",
            "KS4_Performance/England.EngMaths59_Mob_Eng_Previous_Pct",
            "KS4_Performance/England.EngMaths59_Mob_Eng_Previous2_Pct",

            // 2023-24 school grade 9–7: filter on the "9 to 7" band; the CSV filtered on grades 7, 8 and 9, which don't exist in the file.
            "KS4_Performance/Establishment.EngLang79_Sum_Est_Previous_Num",
            "KS4_Performance/Establishment.EngLang79_Sum_Est_Previous_Pct",
            "KS4_Performance/Establishment.EngLit79_Sum_Est_Previous_Num",
            "KS4_Performance/Establishment.EngLit79_Sum_Est_Previous_Pct",
            "KS4_Performance/Establishment.Maths79_Sum_Est_Previous_Num",
            "KS4_Performance/Establishment.Maths79_Sum_Est_Previous_Pct",

            // School performance tables: DfE relabelled breakdowns in 2024-25. The CSV used one label for both years, so
            // EAL was blank for 2024-25 and not-disadvantaged was blank for 2023-24.
            "KS4_Performance/Establishment.Attainment8_EAL_Est_Current_Num",
            "KS4_Performance/Establishment.EngMaths49_EAL_Est_Current_Num",
            "KS4_Performance/Establishment.EngMaths49_EAL_Est_Current_Pct",
            "KS4_Performance/Establishment.EngMaths59_EAL_Est_Current_Num",
            "KS4_Performance/Establishment.EngMaths59_EAL_Est_Current_Pct",
            "KS4_Performance/Establishment.Attainment8_NDi_Est_Previous_Num",
            "KS4_Performance/Establishment.EngMaths49_NDi_Est_Previous_Num",
            "KS4_Performance/Establishment.EngMaths49_NDi_Est_Previous_Pct",
            "KS4_Performance/Establishment.EngMaths59_NDi_Est_Previous_Num",
            "KS4_Performance/Establishment.EngMaths59_NDi_Est_Previous_Pct",
            ],
    };

    public static TheoryData<string> Types() => [.. CatalogueDefinitions.Types];

    [Theory]
    [MemberData(nameof(Types))]
    public void Catalogue_matches_datamap_csv_apart_from_intended_fixes(string type)
    {
        var csv = DataMapCsv.ForType(type);
        var catalogue = CatalogueDefinitions.Rows().Where(r => r.Type == type);

        DataMapEquivalence.Differences(csv, catalogue)
            .Select(d => d[..d.IndexOf(':')])
            .Should().BeEquivalentTo(IntendedChanges.GetValueOrDefault(type, []));
    }
}
