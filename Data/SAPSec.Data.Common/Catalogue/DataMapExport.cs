using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using SAPData.Models;

namespace SAPSec.Data.Common.Catalogue;

/// <summary>
/// Writes every property's mapping (file, key column, value column and filters) as JSON, one mapping per line, so
/// anyone can look up where a figure comes from, and pull requests show each mapping change as a changed line.
/// The file is generated from the catalogue and never edited by hand.
/// </summary>
public static class DataMapExport
{
    public const string Command = "dotnet run --project SAPData -- export-map";

    private static readonly JsonWriterOptions Options = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public static string ToJson(IEnumerable<DataMapRow> rows)
    {
        var mappings = rows
            .Where(r => !string.IsNullOrWhiteSpace(r.PropertyName))
            .OrderBy(r => r.Type, StringComparer.Ordinal)
            .ThenBy(r => r.Range, StringComparer.Ordinal)
            .ThenBy(r => r.PropertyName, StringComparer.Ordinal)
            .Select(Mapping)
            .ToList();

        var json = new StringBuilder();
        json.Append("{\n");
        json.Append($"  \"generatedBy\": \"{Command}\",\n");
        json.Append("  \"editIn\": \"Data/SAPSec.Data.Common/Catalogue/Definitions (this file is generated; changes here are lost)\",\n");
        json.Append("  \"filters\": \"a row is read when every filter column matches; a list of values means any of them\",\n");
        json.Append($"  \"count\": {mappings.Count},\n");
        json.Append("  \"mappings\": [\n    ");
        json.Append(string.Join(",\n    ", mappings));
        json.Append("\n  ]\n}\n");
        return json.ToString();
    }

    private static string Mapping(DataMapRow r)
    {
        using var stream = new MemoryStream();
        using (var json = new Utf8JsonWriter(stream, Options))
        {
            json.WriteStartObject();
            json.WriteString("property", r.PropertyName);
            json.WriteString("dataset", r.Type);
            WriteIfPresent(json, "subtype", r.Subtype);
            json.WriteString("scope", r.Range);
            WriteIfPresent(json, "period", r.YearDesc);
            WriteIfPresent(json, "year", r.Year);
            WriteIfPresent(json, "publisher", r.Source);
            json.WriteString("file", r.FileName.Trim());
            json.WriteString("keyColumn", r.RecordFilterBy);
            json.WriteString("valueColumn", r.Field);
            json.WriteString("dataType", r.DataType);

            json.WriteStartObject("filters");
            var columns = new HashSet<string>(StringComparer.Ordinal);
            foreach (var (column, values) in DataMapFilters.Of(r))
            {
                if (!columns.Add(column))
                    throw new CatalogueException($"{r.PropertyName} filters on '{column}' twice; combine the values into one filter.");

                json.WriteStartArray(column);
                foreach (var value in values)
                    json.WriteStringValue(value);
                json.WriteEndArray();
            }
            json.WriteEndObject();

            json.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static void WriteIfPresent(Utf8JsonWriter json, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            json.WriteString(name, value);
    }
}
