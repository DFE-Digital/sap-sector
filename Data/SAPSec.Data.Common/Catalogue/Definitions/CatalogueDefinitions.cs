using SAPData.Models;

namespace SAPSec.Data.Common.Catalogue.Definitions;

/// <summary>
/// The data map: every dataset defined in SAPData/DataMap/Definitions (listed in catalogue.json). Developers and data
/// engineers edit those JSON files; this turns them into the rows the pipeline reads.
/// </summary>
public static class CatalogueDefinitions
{
    public static IReadOnlyList<IDataMapDefinition> Definitions() => JsonDefinitions.Load();

    public static IReadOnlyList<DataMapRow> Rows() => DataMapCatalogue.Expand(Definitions());
}
