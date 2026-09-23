namespace SAPSec.Data.Common.Catalogue;

/// <summary>
/// A source file for one scope and period: which file, how rows are keyed, which rows apply,
/// and how each breakdown is selected within it.
/// </summary>
/// <example>
/// <code>
/// Source.Ees("202425_performance_tables_schools_final")
///     .KeyedBy("school_urn")
///     .Where("time_period", "202425")
///     .Provides(Breakdowns.Total, ("breakdown", "Total"))
///     .Provides(Breakdowns.Boys, ("breakdown", "Boys"));
/// </code>
/// </example>
public sealed class Source
{
    private readonly List<Filter> _filters = [];
    private readonly Dictionary<string, IReadOnlyList<Filter>> _breakdowns = new(StringComparer.Ordinal);
    private readonly Dictionary<(string Metric, string? Breakdown), string> _fields = [];

    private Source(string org, string file)
    {
        if (string.IsNullOrWhiteSpace(file))
            throw new CatalogueException("Source file must not be empty.");

        Org = org;
        File = file;
    }

    public string Org { get; }

    public string File { get; }

    public string KeyColumn { get; private set; } = "";

    public IReadOnlyList<Filter> Filters => _filters;

    public static Source Ees(string file) => new("EES", file);

    public static Source Cscp(string file) => new("CSCP", file);

    public static Source Gias(string file) => new("GIAS", file);

    public static Source From(string org, string file) => new(org, file);

    /// <summary>The column that identifies the school, LA or England row (DataMap "RecordFilterBy").</summary>
    public Source KeyedBy(string column)
    {
        KeyColumn = column;
        return this;
    }

    /// <summary>A filter applied to every measure read from this source.</summary>
    public Source Where(string column, params string[] values)
    {
        _filters.Add(new Filter(column, values));
        return this;
    }

    /// <summary>Declares that this source provides a breakdown, selected by the given filters (none for a whole-file total).</summary>
    public Source Provides(Breakdown breakdown, params (string Column, string Value)[] filters)
    {
        _breakdowns[breakdown.Code] = filters.Select(f => new Filter(f.Column, f.Value)).ToList();
        return this;
    }

    /// <summary>Overrides the field a metric is read from in this source, e.g. "avg_att8" instead of "attainment8_average".</summary>
    public Source Field(string metric, string field)
    {
        _fields[(metric, null)] = field;
        return this;
    }

    /// <summary>
    /// Overrides the field for one metric and breakdown. Use for wide files where each breakdown is its own column,
    /// e.g. ATT8SCR_BOYS. This also marks the breakdown as provided for that metric.
    /// </summary>
    public Source Field(string metric, Breakdown breakdown, string field)
    {
        _fields[(metric, breakdown.Code)] = field;
        return this;
    }

    internal bool Provides(string metric, Breakdown breakdown) =>
        _breakdowns.ContainsKey(breakdown.Code) || _fields.ContainsKey((metric, breakdown.Code));

    internal IReadOnlyList<Filter> BreakdownFilters(Breakdown breakdown) =>
        _breakdowns.TryGetValue(breakdown.Code, out var filters) ? filters : [];

    internal string ResolveField(string metric, Breakdown breakdown, string defaultField) =>
        _fields.TryGetValue((metric, breakdown.Code), out var byBreakdown) ? byBreakdown
        : _fields.TryGetValue((metric, null), out var byMetric) ? byMetric
        : defaultField;
}
