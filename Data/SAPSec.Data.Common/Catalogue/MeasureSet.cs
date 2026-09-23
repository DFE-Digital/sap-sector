using SAPData.Models;

namespace SAPSec.Data.Common.Catalogue;

/// <summary>
/// A group of metrics sharing a DataMap Type/Subtype, reporting years and sources.
/// Expands to the <see cref="DataMapRow"/>s the SQL generators consume.
/// </summary>
/// <example>
/// <code>
/// new MeasureSet("KS4_Performance", "Performance")
///     .Year(Period.Current, new AcademicYear(2024))
///     .Source(Scope.Establishment, Period.Current, schoolsCurrent)
///     .Breakdowns(Breakdowns.Standard)
///     .Metric(new Metric("Attainment8", "attainment8_average"));
/// </code>
/// </example>
public sealed class MeasureSet
{
    // DataMapRow has Filter..Filter9.
    public const int MaxFilters = 9;

    private readonly SortedDictionary<Period, AcademicYear> _years = [];
    private readonly Dictionary<(Scope Scope, Period Period), Source> _sources = [];
    private readonly List<Scope> _scopes = [];
    private readonly List<Metric> _metrics = [];
    private readonly List<DataMapRow> _rawRows = [];
    private IReadOnlyList<Breakdown> _breakdowns = [Catalogue.Breakdowns.Total];

    public MeasureSet(string type, string subtype)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new CatalogueException("Measure set type must not be empty.");

        Type = type;
        Subtype = subtype;
    }

    public string Type { get; }

    public string Subtype { get; }

    public IReadOnlyList<Metric> Metrics => _metrics;

    public MeasureSet Year(Period period, AcademicYear year)
    {
        _years[period] = year;
        return this;
    }

    /// <summary>Sets Current, Previous and Previous2 to consecutive years ending at <paramref name="currentStartYear"/>.</summary>
    public MeasureSet Years(int currentStartYear) =>
        Year(Period.Current, new AcademicYear(currentStartYear))
            .Year(Period.Previous, new AcademicYear(currentStartYear - 1))
            .Year(Period.Previous2, new AcademicYear(currentStartYear - 2));

    public MeasureSet Source(Scope scope, Period period, Source source)
    {
        _sources[(scope, period)] = source;
        if (!_scopes.Contains(scope))
            _scopes.Add(scope);
        return this;
    }

    /// <summary>Default breakdowns for metrics that don't declare their own.</summary>
    public MeasureSet Breakdowns(IReadOnlyList<Breakdown> breakdowns)
    {
        _breakdowns = breakdowns;
        return this;
    }

    public MeasureSet Breakdowns(params Breakdown[] breakdowns) => Breakdowns((IReadOnlyList<Breakdown>)breakdowns);

    public MeasureSet Metric(Metric metric)
    {
        _metrics.Add(metric);
        return this;
    }

    public MeasureSet Metric(string name, string field, Func<Metric, Metric>? configure = null) =>
        Metric(configure?.Invoke(new Metric(name, field)) ?? new Metric(name, field));

    /// <summary>
    /// Escape hatch for mappings the builder doesn't model (e.g. GIAS attributes with normalised lookups).
    /// The row is emitted as-is after the expanded metrics.
    /// </summary>
    public MeasureSet Row(DataMapRow row)
    {
        _rawRows.Add(row);
        return this;
    }

    public IReadOnlyList<DataMapRow> ToDataMapRows()
    {
        var rows = new List<DataMapRow>();

        foreach (var metric in _metrics)
        {
            foreach (var scope in metric.Scopes ?? _scopes)
            {
                foreach (var period in metric.Periods ?? [.. _years.Keys])
                {
                    if (!_years.TryGetValue(period, out var year))
                        throw new CatalogueException($"{Type}: metric '{metric.Name}' uses period {period}, but no year is declared for it.");

                    if (!_sources.TryGetValue((scope, period), out var source))
                        throw new CatalogueException(
                            $"{Type}: metric '{metric.Name}' has no source for {scope}/{period}. " +
                            "Add a source, or restrict the metric with .In(...) or .During(...).");

                    foreach (var breakdown in metric.Breakdowns ?? _breakdowns)
                    {
                        if (!source.Provides(metric.Name, breakdown))
                            continue;

                        rows.Add(BuildRow(metric, scope, period, year, breakdown, source));
                    }
                }
            }
        }

        rows.AddRange(_rawRows);
        DataMapCatalogue.EnsureUniquePropertyNames(rows);
        return rows;
    }

    private DataMapRow BuildRow(Metric metric, Scope scope, Period period, AcademicYear year, Breakdown breakdown, Source source)
    {
        var propertyName = metric.PropertyName(breakdown, scope, period);

        if (string.IsNullOrWhiteSpace(source.KeyColumn))
            throw new CatalogueException($"{Type}: source '{source.File}' used by '{propertyName}' has no key column. Call .KeyedBy(...).");

        var filters = source.BreakdownFilters(breakdown)
            .Concat(metric.Filters)
            .Concat(source.Filters)
            .ToList();

        if (filters.Count > MaxFilters)
            throw new CatalogueException($"{Type}: '{propertyName}' has {filters.Count} filters; the DataMap supports at most {MaxFilters}.");

        var row = new DataMapRow
        {
            Range = scope.ToString(),
            Ref = propertyName,
            PropertyName = propertyName,
            PropertyDescription = $"{metric.Description} ({breakdown.Description})",
            Source = source.Org,
            Type = Type,
            Subtype = Subtype,
            Year = year.Label,
            YearDesc = period.ToString(),
            FileName = source.File,
            Field = source.ResolveField(metric.Name, breakdown, metric.Field),
            DataType = metric.DataType.DataMapValue(),
            RecordFilterBy = source.KeyColumn,
        };

        for (var i = 0; i < filters.Count; i++)
            SetFilter(row, i + 1, filters[i]);

        return row;
    }

    private static void SetFilter(DataMapRow row, int index, Filter filter)
    {
        var (column, value) = (filter.Column, filter.EncodedValue);

        switch (index)
        {
            case 1: row.Filter = column; row.FilterValue = value; break;
            case 2: row.Filter2 = column; row.Filter2Value = value; break;
            case 3: row.Filter3 = column; row.Filter3Value = value; break;
            case 4: row.Filter4 = column; row.Filter4Value = value; break;
            case 5: row.Filter5 = column; row.Filter5Value = value; break;
            case 6: row.Filter6 = column; row.Filter6Value = value; break;
            case 7: row.Filter7 = column; row.Filter7Value = value; break;
            case 8: row.Filter8 = column; row.Filter8Value = value; break;
            case 9: row.Filter9 = column; row.Filter9Value = value; break;
            default: throw new ArgumentOutOfRangeException(nameof(index), index, null);
        }
    }
}
