using SAPData.Models;

namespace SAPSec.Data.Common.Catalogue;

/// <summary>
/// Columns copied as-is from one source file, for datasets that aren't measures by year and breakdown
/// (e.g. contact details, similar schools lists).
/// </summary>
/// <example>
/// <code>
/// new ColumnSet("Email", "Establishment", Source.From("DfE", "school-email-addresses").KeyedBy("URN"))
///     .Column("URN", "URN")
///     .Column("MainEmail", "MainEmail");
/// </code>
/// </example>
public sealed class ColumnSet(string type, string range, Source source) : IDataMapDefinition
{
    private readonly List<(string Property, string Field, DataType DataType)> _columns = [];

    public string Type { get; } = type;

    public string Range { get; } = range;

    public ColumnSet Column(string property, string field, DataType dataType = DataType.String)
    {
        _columns.Add((property, field, dataType));
        return this;
    }

    public IReadOnlyList<DataMapRow> ToDataMapRows()
    {
        if (string.IsNullOrWhiteSpace(source.KeyColumn))
            throw new CatalogueException($"{Type}: source '{source.File}' has no key column. Call .KeyedBy(...).");

        var rows = _columns.Select(c => new DataMapRow
        {
            Range = Range,
            Ref = c.Property,
            PropertyName = c.Property,
            PropertyDescription = c.Property,
            Source = source.Org,
            Type = Type,
            FileName = source.File,
            Field = c.Field,
            DataType = c.DataType.DataMapValue(),
            RecordFilterBy = source.KeyColumn,
        }).ToList();

        DataMapCatalogue.EnsureUniquePropertyNames(rows);
        return rows;
    }
}
