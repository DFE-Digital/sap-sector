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

    /// <summary>The data map the pipeline uses: the definitions plus any overrides imported from the mapping list.</summary>
    public static IReadOnlyList<DataMapRow> Rows() => MappingOverrides.Embedded().ApplyTo(CodeRows());

    /// <summary>The definitions' rows only, without overrides.</summary>
    public static IReadOnlyList<DataMapRow> CodeRows() => DataMapCatalogue.Expand(Definitions());
}
