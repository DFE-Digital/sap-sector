namespace SAPSec.Data.Common.Catalogue;

/// <summary>
/// A measure declared once and expanded across the scopes, periods and breakdowns of its <see cref="MeasureSet"/>.
/// </summary>
public sealed class Metric
{
    public const string DefaultNameTemplate = "{metric}_{breakdown}_{scope}_{period}_{unit}";

    private readonly List<Filter> _filters = [];

    public Metric(string name, string field)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new CatalogueException("Metric name must not be empty.");

        Name = name;
        Field = field;
        Description = name;
    }

    public string Name { get; }

    /// <summary>Default source field. Sources can override it with <see cref="Source.Field(string, string)"/>.</summary>
    public string Field { get; }

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

    internal string PropertyName(Breakdown breakdown, Scope scope, Period period) =>
        NameTemplate
            .Replace("{metric}", Name)
            .Replace("{breakdown}", breakdown.Code)
            .Replace("{scope}", scope.NameCode())
            .Replace("{period}", period.ToString())
            .Replace("{unit}", Unit.ToString());
}
