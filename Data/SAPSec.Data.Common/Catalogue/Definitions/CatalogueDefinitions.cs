using SAPData.Models;

namespace SAPSec.Data.Common.Catalogue.Definitions;

/// <summary>
/// Every dataset defined in code. The pipeline uses these instead of their datamap.csv rows.
/// </summary>
public static class CatalogueDefinitions
{
    public static IReadOnlyList<string> Types { get; } = [Ks4Performance.Type, Ks4Destinations.Type];

    public static IReadOnlyList<MeasureSet> MeasureSets() => [.. Ks4Performance.MeasureSets(), .. Ks4Destinations.MeasureSets()];

    public static IReadOnlyList<DataMapRow> Rows() => DataMapCatalogue.Expand(MeasureSets());
}
