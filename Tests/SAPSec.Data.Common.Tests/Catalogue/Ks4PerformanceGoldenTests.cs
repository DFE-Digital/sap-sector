using SAPSec.Data.Common.Catalogue;
using SAPSec.Data.Common.Catalogue.Definitions;

namespace SAPSec.Data.Common.Tests.Catalogue;

/// <summary>
/// Proves the KS4 Performance catalogue produces the same materialized views as datamap.csv,
/// apart from deliberate data fixes. Remove once datamap.csv is deleted (Story 3).
/// </summary>
public class Ks4PerformanceGoldenTests
{
    private static readonly string[] IntendedChanges =
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
    ];

    [Fact]
    public void Catalogue_matches_datamap_csv_apart_from_intended_fixes()
    {
        var csv = DataMapCsv.ForType(Ks4Performance.Type);
        var catalogue = DataMapCatalogue.Expand(Ks4Performance.MeasureSets());

        DataMapEquivalence.Differences(csv, catalogue)
            .Select(d => d[..d.IndexOf(':')])
            .Should().BeEquivalentTo(IntendedChanges);
    }

    [Fact]
    public void Catalogue_only_contains_ks4_performance_rows()
    {
        DataMapCatalogue.Expand(Ks4Performance.MeasureSets())
            .Should().OnlyContain(r => r.Type == Ks4Performance.Type);
    }
}
