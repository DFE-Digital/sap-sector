using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SAPData.Models;

namespace SAPSec.Data.Common.Catalogue;

/// <summary>
/// The mapping list (SAPData/DataMap/datamap.generated.json): every property's file, key column, value column and
/// filters as JSON, one mapping per line, so anyone can look up where a figure comes from and pull requests show each
/// mapping change as a changed line. Lines can be edited and imported back with <see cref="ImportCommand"/>.
/// </summary>
public static class DataMapExport
{
    public const string Command = "dotnet run --project SAPData -- export-map";

    public const string ImportCommand = "dotnet run --project SAPData -- import-map";

    /// <summary>The list for the definitions' rows with <paramref name="overrides"/> applied and marked.</summary>
    public static string ToJson(IReadOnlyList<DataMapRow> codeRows, MappingOverrides overrides)
    {
        var mappings = overrides.ApplyTo(codeRows)
            .Where(r => !string.IsNullOrWhiteSpace(r.PropertyName))
            .OrderBy(r => r.Type, StringComparer.Ordinal)
            .ThenBy(r => r.Range, StringComparer.Ordinal)
            .ThenBy(r => r.PropertyName, StringComparer.Ordinal)
            .Select(Mapping.FromRow)
            .Select(m => overrides.Keys.Contains(m.Key) ? m with { Overridden = true } : m)
            .ToList();

        var json = new StringBuilder();
        json.Append("{\n");
        json.Append($"  \"generatedBy\": \"{Command}\",\n");
        json.Append($"  \"howToEdit\": \"change or add lines, then run: {ImportCommand}. Definitions: Data/SAPSec.Data.Common/Catalogue/Definitions\",\n");
        json.Append("  \"filters\": \"a row is read when every filter column matches; a list of values means any of them\",\n");
        json.Append($"  \"codeVersion\": \"{CodeVersion(codeRows)}\",\n");
        json.Append($"  \"count\": {mappings.Count},\n");
        json.Append("  \"mappings\": [\n    ");
        json.Append(string.Join(",\n    ", mappings.Select(m => m.ToLine())));
        json.Append("\n  ]\n}\n");
        return json.ToString();
    }

    /// <summary>
    /// Identifies the definitions a list was generated from, so an import can tell whether the definitions have changed
    /// since (a stale list would otherwise undo those changes).
    /// </summary>
    public static string CodeVersion(IReadOnlyList<DataMapRow> codeRows)
    {
        var lines = codeRows
            .Where(r => !string.IsNullOrWhiteSpace(r.PropertyName))
            .Select(r => Mapping.FromRow(r).ToLine())
            .Order(StringComparer.Ordinal);

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("\n", lines)));
        return Convert.ToHexString(hash)[..16].ToLowerInvariant();
    }

    /// <summary>Reads a mapping list, e.g. one a data engineer has edited.</summary>
    /// <exception cref="CatalogueException">The file isn't a valid mapping list; the message says where.</exception>
    public static (string CodeVersion, IReadOnlyList<Mapping> Mappings) Parse(string json)
    {
        try
        {
            var list = JsonSerializer.Deserialize<ListFile>(json, Mapping.JsonOptions)
                       ?? throw new CatalogueException("The mapping list is empty.");
            return (list.CodeVersion ?? "", list.Mappings ?? []);
        }
        catch (JsonException e)
        {
            throw new CatalogueException($"The mapping list isn't valid{(e.LineNumber is { } line ? $" at line {line + 1}" : "")}: {e.Message}");
        }
    }

    private sealed record ListFile(string? CodeVersion, IReadOnlyList<Mapping>? Mappings);
}
