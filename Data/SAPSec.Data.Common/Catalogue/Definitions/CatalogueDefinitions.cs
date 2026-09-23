using SAPData.Models;

namespace SAPSec.Data.Common.Catalogue.Definitions;

/// <summary>
/// Every dataset defined in code. The pipeline uses these instead of their datamap.csv rows.
/// </summary>
public static class CatalogueDefinitions
{
    public static IReadOnlyList<string> Types { get; } =
    [
        Ks4Performance.Type, Ks4Destinations.Type, PupilAbsence.Type, Ks2Performance.Type,
        SchoolEmail.Type, Workforce.Type, .. SimilarSchools.Types,
    ];

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
