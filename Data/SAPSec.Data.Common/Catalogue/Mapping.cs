using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using SAPData.Models;

namespace SAPSec.Data.Common.Catalogue;

/// <summary>
/// One property's mapping, as written in the mapping list (datamap.generated.json): which file, which row (key column
/// and filters) and which column holds the value.
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

    private static readonly JsonSerializerOptions JsonOptions = new()
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

    private static string? NullIfBlank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
}
