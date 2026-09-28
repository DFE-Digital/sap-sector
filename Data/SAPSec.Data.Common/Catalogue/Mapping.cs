using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using SAPData.Models;

namespace SAPSec.Data.Common.Catalogue;

/// <summary>
/// One property's mapping, as written in the mapping list (datamap.generated.json) and the overrides file:
/// which file, which row (key column and filters) and which column holds the value.
/// </summary>
public sealed record Mapping
{
    public required string Property { get; init; }
    public required string Dataset { get; init; }
    public string? Subtype { get; init; }
    public required string Scope { get; init; }
    public string? Period { get; init; }
    public string? Year { get; init; }
    public string? Publisher { get; init; }
    public required string File { get; init; }
    public required string KeyColumn { get; init; }
    public required string ValueColumn { get; init; }
    public required string DataType { get; init; }

    /// <summary>Column → values. A row is read when every column matches one of its values.</summary>
    public Dictionary<string, string[]> Filters { get; init; } = new(StringComparer.Ordinal);

    /// <summary>Set in the mapping list when this mapping comes from the overrides file rather than the definitions.</summary>
    public bool? Overridden { get; init; }

    /// <summary>Identifies a property: property names are unique within a dataset and scope.</summary>
    [JsonIgnore]
    public string Key => KeyOf(Dataset, Scope, Property);

    internal static string KeyOf(string dataset, string scope, string property) => $"{dataset}/{scope}/{property}";

    internal static string KeyOf(DataMapRow r) => KeyOf(r.Type, r.Range, r.PropertyName);

    internal static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>The mapping on one line of JSON.</summary>
    public string ToLine() => JsonSerializer.Serialize(this, JsonOptions);

    public static Mapping FromRow(DataMapRow r)
    {
        var filters = new Dictionary<string, string[]>(StringComparer.Ordinal);
        foreach (var (column, values) in DataMapFilters.Of(r))
        {
            if (!filters.TryAdd(column, values))
                throw new CatalogueException($"{r.PropertyName} filters on '{column}' twice; combine the values into one filter.");
        }

        return new Mapping
        {
            Property = r.PropertyName,
            Dataset = r.Type,
            Subtype = NullIfBlank(r.Subtype),
            Scope = r.Range,
            Period = NullIfBlank(r.YearDesc),
            Year = NullIfBlank(r.Year),
            Publisher = NullIfBlank(r.Source),
            File = r.FileName.Trim(),
            KeyColumn = r.RecordFilterBy,
            ValueColumn = r.Field,
            DataType = r.DataType,
            Filters = filters,
        };
    }

    /// <summary>
    /// The data map row for this mapping. <paramref name="replacing"/> is the row it overrides, if any, whose
    /// description is kept.
    /// </summary>
    public DataMapRow ToRow(DataMapRow? replacing = null)
    {
        foreach (var (name, value) in new[] { ("property", Property), ("dataset", Dataset), ("scope", Scope), ("file", File),
                     ("keyColumn", KeyColumn), ("valueColumn", ValueColumn), ("dataType", DataType) })
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new CatalogueException($"{Key}: '{name}' must not be empty.");
        }

        if (Filters.Count > MeasureSet.MaxFilters)
            throw new CatalogueException($"{Key}: has {Filters.Count} filters; the data map supports at most {MeasureSet.MaxFilters}.");

        var row = new DataMapRow
        {
            Range = Scope,
            Ref = Property,
            PropertyName = Property,
            PropertyDescription = replacing?.PropertyDescription ?? Property,
            Source = Publisher ?? "",
            Type = Dataset,
            Subtype = Subtype ?? "",
            Year = Year ?? "",
            YearDesc = Period ?? "",
            FileName = File,
            Field = ValueColumn,
            DataType = DataType,
            RecordFilterBy = KeyColumn,
        };

        var index = 1;
        foreach (var (column, values) in Filters)
        {
            try
            {
                MeasureSet.SetFilter(row, index++, new Filter(column, values ?? []));
            }
            catch (CatalogueException e)
            {
                throw new CatalogueException($"{Key}: {e.Message}");
            }
        }

        return row;
    }

    private static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
}
