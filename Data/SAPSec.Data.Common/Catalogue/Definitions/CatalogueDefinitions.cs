using SAPData.Models;

namespace SAPSec.Data.Common.Catalogue.Definitions;

/// <summary>
/// Every dataset in the data map. Add a new dataset here.
/// </summary>
public static class CatalogueDefinitions
{
    public static IReadOnlyList<IDataMapDefinition> Definitions() =>
    [
        .. Ks4Performance.MeasureSets(),
        .. Ks4Destinations.MeasureSets(),
        .. PupilAbsence.MeasureSets(),
        .. Ks2Performance.MeasureSets(),
        .. SchoolEmail.Definitions(),
        .. Workforce.Definitions(),
        .. SimilarSchools.Definitions(),
    ];

    public static IReadOnlyList<DataMapRow> Rows() => DataMapCatalogue.Expand(Definitions());
}
