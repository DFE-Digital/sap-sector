using SAPData.Models;

namespace SAPSec.Data.Common.Catalogue;

/// <summary>
/// Expands catalogue definitions into the flat DataMap rows consumed by the SQL and JSON generators.
/// </summary>
public static class DataMapCatalogue
{
    public static IReadOnlyList<DataMapRow> Expand(IEnumerable<IDataMapDefinition> definitions)
    {
        var rows = definitions.SelectMany(d => d.ToDataMapRows()).ToList();

        // Definitions with different subtypes share a view (Range + Type), so check across all of them.
        EnsureUniquePropertyNames(rows);
        return rows;
    }

    internal static void EnsureUniquePropertyNames(IEnumerable<DataMapRow> rows)
    {
        var duplicates = rows
            .Where(r => !string.IsNullOrWhiteSpace(r.PropertyName))
            .GroupBy(r => (r.Range, r.Type, r.PropertyName))
            .Where(g => g.Count() > 1)
            .Select(g => $"{g.Key.Type}/{g.Key.Range}/{g.Key.PropertyName} ({g.Count()}x)")
            .ToList();

        if (duplicates.Count > 0)
            throw new CatalogueException($"Duplicate DataMap property names: {string.Join(", ", duplicates)}");
    }
}
