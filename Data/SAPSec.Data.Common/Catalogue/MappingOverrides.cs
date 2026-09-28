using System.Text;
using System.Text.Json;
using SAPData.Models;

namespace SAPSec.Data.Common.Catalogue;

/// <summary>
/// Mapping changes made in the mapping list (SAPData/DataMap/datamap.generated.json) rather than in the definitions.
/// Each override is a whole mapping line: it replaces the definitions' mapping with the same dataset, scope and
/// property, or adds a new one. <c>import-map</c> writes them to <see cref="FileName"/>, next to this class, and the
/// file is built into this assembly so the pipeline, tests and commands all see the same data map.
/// </summary>
public sealed class MappingOverrides
{
    public const string FileName = "datamap.overrides.json";

    private static readonly Lazy<MappingOverrides> EmbeddedOverrides = new(LoadEmbedded);

    private MappingOverrides(IReadOnlyList<Mapping> mappings)
    {
        Mappings = mappings.OrderBy(m => m.Key, StringComparer.Ordinal).ToList();
        Keys = Mappings.Select(m => m.Key).ToHashSet(StringComparer.Ordinal);
    }

    public static MappingOverrides None { get; } = new([]);

    public IReadOnlyList<Mapping> Mappings { get; }

    public IReadOnlySet<string> Keys { get; }

    /// <summary>The overrides built into this assembly from <see cref="FileName"/>.</summary>
    public static MappingOverrides Embedded() => EmbeddedOverrides.Value;

    /// <summary>The definitions' rows with these overrides applied: replaced in place, added at the end.</summary>
    public IReadOnlyList<DataMapRow> ApplyTo(IReadOnlyList<DataMapRow> codeRows)
    {
        if (Mappings.Count == 0)
            return codeRows;

        var rows = codeRows.ToList();
        var indexByKey = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var i = 0; i < rows.Count; i++)
            indexByKey.TryAdd(Mapping.KeyOf(rows[i]), i);

        foreach (var mapping in Mappings)
        {
            if (indexByKey.TryGetValue(mapping.Key, out var index))
                rows[index] = mapping.ToRow(rows[index]);
            else
                rows.Add(mapping.ToRow());
        }

        DataMapCatalogue.EnsureUniquePropertyNames(rows);
        return rows;
    }

    /// <summary>
    /// The overrides that turn the definitions' mappings into <paramref name="edited"/>: every changed or added line.
    /// A line changed back to the definitions' mapping is no longer an override. Mappings can't be removed here.
    /// </summary>
    /// <exception cref="CatalogueException">The edited list has duplicate, removed or invalid mappings.</exception>
    public static MappingOverrides FromEditedList(IReadOnlyList<DataMapRow> codeRows, IReadOnlyList<Mapping> edited)
    {
        var problems = new List<string>();
        var code = codeRows
            .Where(r => !string.IsNullOrWhiteSpace(r.PropertyName))
            .ToDictionary(Mapping.KeyOf, Mapping.FromRow, StringComparer.Ordinal);

        problems.AddRange(edited
            .GroupBy(m => m.Key, StringComparer.Ordinal)
            .Where(g => g.Count() > 1)
            .Select(g => $"{g.Key} appears {g.Count()} times"));

        var editedKeys = edited.Select(m => m.Key).ToHashSet(StringComparer.Ordinal);
        var removed = code.Keys.Where(k => !editedKeys.Contains(k)).Order(StringComparer.Ordinal).ToList();
        if (removed.Count > 0)
        {
            problems.Add($"{removed.Count} mapping(s) were removed or renamed, which the list can't do (remove or rename them in " +
                         $"the definitions): {string.Join(", ", removed.Take(10))}{(removed.Count > 10 ? ", …" : "")}");
        }

        var overrides = new List<Mapping>();
        foreach (var mapping in edited.DistinctBy(m => m.Key))
        {
            var candidate = mapping with { Overridden = null };
            if (code.TryGetValue(candidate.Key, out var original) && original.ToLine() == candidate.ToLine())
                continue;

            try
            {
                candidate.ToRow();
                overrides.Add(candidate);
            }
            catch (CatalogueException e)
            {
                problems.Add(e.Message);
            }
        }

        if (problems.Count > 0)
            throw new CatalogueException("The mapping list can't be imported:" + Environment.NewLine + "  " + string.Join(Environment.NewLine + "  ", problems));

        return new MappingOverrides(overrides);
    }

    public static MappingOverrides Parse(string json)
    {
        var file = JsonSerializer.Deserialize<OverridesFile>(json, Mapping.JsonOptions)
                   ?? throw new CatalogueException($"{FileName} is empty.");
        return new MappingOverrides(file.Overrides);
    }

    public string ToJson()
    {
        var json = new StringBuilder();
        json.Append("{\n");
        json.Append("  \"about\": \"Mapping changes imported from SAPData/DataMap/datamap.generated.json. Each line replaces the definitions' mapping with the same dataset, scope and property, or adds one. Written by import-map; move them into the definitions when convenient.\",\n");
        json.Append("  \"overrides\": [");
        if (Mappings.Count > 0)
            json.Append("\n    ").Append(string.Join(",\n    ", Mappings.Select(m => m.ToLine()))).Append("\n  ");
        json.Append("]\n}\n");
        return json.ToString();
    }

    private static MappingOverrides LoadEmbedded()
    {
        using var stream = typeof(MappingOverrides).Assembly.GetManifestResourceStream(FileName);
        if (stream is null)
            return None;

        using var reader = new StreamReader(stream);
        return Parse(reader.ReadToEnd());
    }

    private sealed record OverridesFile(IReadOnlyList<Mapping> Overrides);
}
