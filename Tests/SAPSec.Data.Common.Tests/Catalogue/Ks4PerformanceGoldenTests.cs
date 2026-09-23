using SAPSec.Data.Common.Catalogue;
using SAPSec.Data.Common.Catalogue.Definitions;

namespace SAPSec.Data.Common.Tests.Catalogue;

/// <summary>
/// Proves the KS4 Performance catalogue produces the same materialized views as datamap.csv.
/// Remove once datamap.csv is deleted (Story 3).
/// </summary>
public class Ks4PerformanceGoldenTests
{
    [Fact]
    public void Catalogue_has_the_same_effective_mapping_as_datamap_csv()
    {
        var csv = DataMapCsv.ForType(Ks4Performance.Type);
        var catalogue = DataMapCatalogue.Expand(Ks4Performance.MeasureSets());

        DataMapEquivalence.Differences(csv, catalogue).Should().BeEmpty();
    }

    [Fact]
    public void Catalogue_only_contains_ks4_performance_rows()
    {
        DataMapCatalogue.Expand(Ks4Performance.MeasureSets())
            .Should().OnlyContain(r => r.Type == Ks4Performance.Type);
    }
}
