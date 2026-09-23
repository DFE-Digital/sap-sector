namespace SAPSec.Data.Common.Catalogue;

/// <summary>
/// A measure declared once and expanded across the scopes, periods and breakdowns of its <see cref="MeasureSet"/>.
/// </summary>
public sealed class Metric
{
    public const string DefaultNameTemplate = "{metric}_{breakdown}_{scope}_{period}_{unit}";

    private readonly List<Filter> _filters = [];
    private readonly Dictionary<Source, List<Filter>> _sourceFilters = [];
    private readonly Dictionary<(Source Source, string? Breakdown), string> _fields = [];
    private readonly HashSet<(Scope, Period)> _skipped = [];

    public Metric(string name, string field)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new CatalogueException("Metric name must not be empty.");

        Name = name;
        DefaultField = field;
        Description = name;
    }

    public string Name { get; }

    /// <summary>Default source field. Override per source with <see cref="Field(Source, string)"/>.</summary>
    public string DefaultField { get; }

    public string Description { get; private set; }

    public DataType DataType { get; private set; } = DataType.Double;

    public Unit Unit { get; private set; } = Unit.Num;

    public string NameTemplate { get; private set; } = DefaultNameTemplate;

    public IReadOnlyList<Filter> Filters => _filters;

    /// <summary>Breakdowns for this metric; null uses the measure set's breakdowns.</summary>
    public IReadOnlyList<Breakdown>? Breakdowns { get; private set; }

    /// <summary>Scopes for this metric; null uses every scope the measure set has sources for.</summary>
    public IReadOnlyList<Scope>? Scopes { get; private set; }

    /// <summary>Periods for this metric; null uses every period the measure set declares.</summary>
    public IReadOnlyList<Period>? Periods { get; private set; }

    public Metric Described(string description)
    {
        Description = description;
        return this;
    }

    public Metric OfType(DataType dataType)
    {
        DataType = dataType;
        return this;
    }

    public Metric Percentage()
    {
        Unit = Unit.Pct;
        return this;
    }

    /// <summary>A filter applied only to this metric, e.g. subject = "Biology".</summary>
    public Metric Where(string column, params string[] values)
    {
        _filters.Add(new Filter(column, values));
        return this;
    }

    /// <summary>A filter applied to this metric only when reading from <paramref name="source"/>.</summary>
    public Metric Where(Source source, string column, params string[] values)
    {
        if (!_sourceFilters.TryGetValue(source, out var filters))
            _sourceFilters[source] = filters = [];

        filters.Add(new Filter(column, values));
        return this;
    }

    /// <summary>Reads this metric from a different field in <paramref name="source"/>, e.g. "avg_att8" instead of "attainment8_average".</summary>
    public Metric Field(Source source, string field)
    {
        _fields[(source, null)] = field;
        return this;
    }

    /// <summary>
    /// Reads one breakdown of this metric from its own field in <paramref name="source"/>. Use for wide files where each
    /// breakdown is a separate column (e.g. ATT8SCR_BOYS). This also makes the breakdown available from that source
    /// for this metric, even if the source doesn't <see cref="Source.Provides"/> it.
    /// </summary>
    public Metric Field(Source source, Breakdown breakdown, string field)
    {
        _fields[(source, breakdown.Code)] = field;
        return this;
    }

    /// <summary>Excludes one scope and period, e.g. a measure not published in an older source file.</summary>
    public Metric Skip(Scope scope, Period period)
    {
        _skipped.Add((scope, period));
        return this;
    }

    public Metric For(params Breakdown[] breakdowns)
    {
        Breakdowns = breakdowns;
        return this;
    }

    public Metric In(params Scope[] scopes)
    {
        Scopes = scopes;
        return this;
    }

    public Metric During(params Period[] periods)
    {
        Periods = periods;
        return this;
    }

    /// <summary>
    /// Overrides the property name template. Placeholders: {metric}, {breakdown}, {scope}, {period}, {unit}.
    /// </summary>
    public Metric Named(string template)
    {
        NameTemplate = template;
        return this;
    }

    internal bool IsSkipped(Scope scope, Period period) => _skipped.Contains((scope, period));

    internal bool IsAvailableFrom(Source source, Breakdown breakdown) =>
        source.HasBreakdown(breakdown) || _fields.ContainsKey((source, breakdown.Code));

    internal IReadOnlyList<Filter> FiltersFor(Source source) =>
        _sourceFilters.TryGetValue(source, out var filters) ? [.. _filters, .. filters] : _filters;

    internal string FieldFor(Source source, Breakdown breakdown) =>
        _fields.TryGetValue((source, breakdown.Code), out var byBreakdown) ? byBreakdown
        : _fields.TryGetValue((source, null), out var bySource) ? bySource
        : DefaultField;

    internal string PropertyName(Breakdown breakdown, Scope scope, Period period) =>
        NameTemplate
            .Replace("{metric}", Name)
            .Replace("{breakdown}", breakdown.Code)
            .Replace("{scope}", scope.NameCode())
            .Replace("{period}", period.ToString())
            .Replace("{unit}", Unit.ToString());
}
