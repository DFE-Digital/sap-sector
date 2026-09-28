using System.Text;
using SAPData.Models;

namespace SAPSec.Data.Common.Catalogue;

/// <summary>
/// The mapping list (SAPData/DataMap/datamap.generated.json): every property's file, key column, value column and
/// filters, one mapping per line, generated from the definitions. It shows the effect of a definition change property
/// by property, so reviewers can see exactly which figures a pull request changes.
/// </summary>
public static class DataMapExport
{
    public const string Command = "dotnet run --project SAPData -- export-map";

    public static string ToJson(IEnumerable<DataMapRow> rows)
    {
        var mappings = rows
            .Where(r => !string.IsNullOrWhiteSpace(r.PropertyName))
            .OrderBy(r => r.Type, StringComparer.Ordinal)
            .ThenBy(r => r.Range, StringComparer.Ordinal)
            .ThenBy(r => r.PropertyName, StringComparer.Ordinal)
            .Select(r => Mapping.FromRow(r).ToLine())
            .ToList();

        var json = new StringBuilder();
        json.Append("{\n");
        json.Append($"  \"generatedBy\": \"{Command}\",\n");
        json.Append("  \"editIn\": \"SAPData/DataMap/Definitions (this file is generated; changes here are lost)\",\n");
        json.Append("  \"filters\": \"a row is read when every filter column matches; a list of values means any of them\",\n");
        json.Append($"  \"count\": {mappings.Count},\n");
        json.Append("  \"mappings\": [\n    ");
        json.Append(string.Join(",\n    ", mappings));
        json.Append("\n  ]\n}\n");
        return json.ToString();
    }
}
