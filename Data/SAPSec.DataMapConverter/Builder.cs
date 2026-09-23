using SAPData.Models;
using SAPSec.Data.Common.Catalogue;

namespace SAPSec.DataMapConverter;

/// <summary>
/// Builds real <see cref="MeasureSet"/>s from the inferred model, mirroring what <see cref="Emitter"/> writes,
/// so the conversion can be verified before any code is generated.
/// </summary>
internal static class Builder
{
    public static MeasureSet Build(string type, SetModel set)
    {
        var sources = set.Sources.ToDictionary(s => s, BuildSource);
        var measureSet = new MeasureSet(type, set.Subtype);

        foreach (var (period, year) in set.Years)
            measureSet.Year(period, new AcademicYear(year));

        foreach (var source in set.Sources)
            measureSet.Source(source.Scope, source.Period, sources[source]);

        measureSet.Breakdowns(set.Breakdowns);

        foreach (var m in set.Metrics)
        {
            var metric = new Metric(m.Key.Name, m.DefaultField).OfType(m.DataType);

            if (m.Key.Unit == Unit.Pct)
                metric.Percentage();
            if (m.Key.Template != Metric.DefaultNameTemplate)
                metric.Named(m.Key.Template);
            if (!m.UsesSetBreakdowns)
                metric.For([.. m.For]);
            if (m.In is not null)
                metric.In([.. m.In]);
            if (m.During is not null)
                metric.During([.. m.During]);
            foreach (var (scope, period) in m.Skips)
                metric.Skip(scope, period);
            foreach (var f in m.Where)
                metric.Where(f.Column, f.Values);
            foreach (var (source, f) in m.SourceWhere)
                metric.Where(sources[source], f.Column, f.Values);
            foreach (var (source, field) in m.SourceFields)
                metric.Field(sources[source], field);
            foreach (var (source, breakdown, field) in m.BreakdownFields)
                metric.Field(sources[source], breakdown, field);

            measureSet.Metric(metric);
        }

        return measureSet;
    }

    public static MeasureSet BuildRaw(string type, IEnumerable<DataMapRow> rows)
    {
        var set = new MeasureSet(type, "Unmodelled");
        foreach (var row in rows)
            set.Row(row);
        return set;
    }

    private static Source BuildSource(SourceModel s)
    {
        var source = Source.From(s.Org, s.File).KeyedBy(s.Key);

        foreach (var f in s.Filters)
            source.Where(f.Column, f.Values);

        foreach (var (breakdown, filters) in s.Provides)
            source.Provides(breakdown, [.. filters.Select(f => (f.Column, f.Value))]);

        return source;
    }
}
