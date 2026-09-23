using SAPSec.Data.Common.Catalogue;

namespace SAPSec.DataMapConverter;

/// <summary>
/// Infers measure sets, sources and metrics from flat datamap rows.
/// Metrics that don't fit the model are reported as failures so the caller can keep them as raw rows.
/// </summary>
internal static class Inference
{
    public sealed record Result(List<SetModel> Sets, Dictionary<MetricKey, string> Failures);

    public static Result Infer(IReadOnlyList<ParsedRow> rows)
    {
        var failures = new Dictionary<MetricKey, string>();
        var byMetric = rows.GroupBy(r => r.Metric).ToList();
        var families = new List<(SetModel Set, Dictionary<(Scope, Period), string> Files, List<IGrouping<MetricKey, ParsedRow>> Metrics)>();

        foreach (var metric in byMetric)
        {
            var files = metric.GroupBy(r => (r.Scope, r.Period)).ToDictionary(g => g.Key, g => g.Select(r => r.Row.FileName.Trim()).Distinct().ToList());
            if (files.Values.Any(f => f.Count > 1))
            {
                failures[metric.Key] = "reads one scope and period from more than one file";
                continue;
            }

            var years = metric.GroupBy(r => r.Period).ToDictionary(g => g.Key, g => g.Select(r => r.YearStart).Distinct().ToList());
            if (years.Values.Any(y => y.Count > 1))
            {
                failures[metric.Key] = "has more than one Year for a period";
                continue;
            }

            var family = families.FirstOrDefault(f =>
                f.Set.Subtype == metric.Key.Subtype &&
                files.All(x => !f.Files.TryGetValue(x.Key, out var file) || file == x.Value[0]) &&
                years.All(y => !f.Set.Years.TryGetValue(y.Key, out var year) || year == y.Value[0]));

            if (family.Set is null)
            {
                family = (new SetModel { Subtype = metric.Key.Subtype }, [], []);
                families.Add(family);
            }

            foreach (var (key, file) in files)
                family.Files[key] = file[0];
            foreach (var (period, year) in years)
                family.Set.Years[period] = year[0];
            family.Metrics.Add(metric);
        }

        foreach (var (set, files, metrics) in families)
            BuildSet(set, files, metrics, failures);

        return new Result(families.Select(f => f.Set).Where(s => s.Metrics.Count > 0).ToList(), failures);
    }

    private static void BuildSet(
        SetModel set,
        Dictionary<(Scope, Period), string> files,
        List<IGrouping<MetricKey, ParsedRow>> metrics,
        Dictionary<MetricKey, string> failures)
    {
        var scopeOrder = metrics.SelectMany(m => m).Select(r => r.Scope).Distinct().ToList();
        var metricModels = metrics.ToDictionary(m => m.Key, m => new MetricModel { Key = m.Key });

        foreach (var (scope, period) in files.Keys.OrderBy(k => scopeOrder.IndexOf(k.Item1)).ThenBy(k => k.Item2))
        {
            var sourceRows = metrics.SelectMany(m => m).Where(r => r.Scope == scope && r.Period == period).ToList();
            var source = new SourceModel
            {
                Scope = scope,
                Period = period,
                File = files[(scope, period)],
                Org = MostCommon(sourceRows.Select(r => r.Row.Source)),
                Key = MostCommon(sourceRows.Select(r => r.Row.RecordFilterBy).Where(k => !string.IsNullOrWhiteSpace(k))),
            };
            set.Sources.Add(source);

            // Filters every row shares belong to the source.
            source.Filters.AddRange(sourceRows[0].Filters.Where(f => sourceRows.All(r => r.Filters.Contains(f))));
            var remaining = sourceRows.ToDictionary(r => r, r => r.Filters.Where(f => !source.Filters.Contains(f)).ToList());
            var columns = remaining.Values.SelectMany(f => f).Select(f => f.Column).Distinct().ToList();

            // A column whose value depends only on the breakdown selects the breakdown; one that depends only on the
            // metric is a metric filter. Anything else can't be modelled.
            var breakdownColumns = columns.Where(c => IsFunctionOf(remaining, c, r => r.Breakdown.Code)).ToList();
            var metricColumns = columns.Except(breakdownColumns).Where(c => IsFunctionOf(remaining, c, r => r.Metric)).ToList();
            foreach (var column in columns.Except(breakdownColumns).Except(metricColumns))
                foreach (var r in remaining.Where(x => x.Value.Any(f => f.Column == column)))
                    failures.TryAdd(r.Key.Metric, $"filter '{column}' depends on both metric and breakdown in {source.File}");

            source.FieldBased = breakdownColumns.Count == 0 &&
                sourceRows.GroupBy(r => r.Metric).Any(m => m.Select(r => r.Row.Field).Distinct().Count() > 1);

            if (!source.FieldBased)
            {
                foreach (var breakdownRows in sourceRows.GroupBy(r => r.Breakdown.Code).OrderBy(g => ParsedRow.Order(g.First().Breakdown)))
                {
                    var filters = remaining[breakdownRows.First()].Where(f => breakdownColumns.Contains(f.Column)).ToList();
                    if (filters.Any(f => f.Values.Length > 1))
                        foreach (var r in breakdownRows)
                            failures.TryAdd(r.Metric, $"breakdown filter with OR values in {source.File}");

                    source.Provides.Add((breakdownRows.First().Breakdown, filters));
                }
            }

            foreach (var metricRows in sourceRows.GroupBy(r => r.Metric))
            {
                var model = metricModels[metricRows.Key];
                foreach (var filter in remaining[metricRows.First()].Where(f => metricColumns.Contains(f.Column)))
                    model.SourceWhere.Add((source, filter));
            }
        }

        var setScopes = set.Sources.Select(s => s.Scope).Distinct().ToList();
        var setPeriods = set.Years.Keys.ToList();

        foreach (var metricRows in metrics)
        {
            var model = metricModels[metricRows.Key];

            var dataTypes = metricRows.Select(r => r.Row.DataType.Trim().ToLowerInvariant()).Distinct().ToList();
            if (dataTypes.Count > 1)
            {
                failures.TryAdd(model.Key, "has more than one DataType");
                continue;
            }

            ParsedRow.TryDataType(dataTypes[0], out var dataType);
            model.DataType = dataType;
            model.DefaultField = MostCommon(metricRows.Select(r => r.Row.Field.Trim()));
            model.For.AddRange(metricRows.Select(r => r.Breakdown).DistinctBy(b => b.Code).OrderBy(ParsedRow.Order));

            foreach (var sourceRows in metricRows.GroupBy(r => (r.Scope, r.Period)))
            {
                var source = set.Sources.Single(s => s.Scope == sourceRows.Key.Scope && s.Period == sourceRows.Key.Period);
                var fields = sourceRows.Select(r => r.Row.Field.Trim()).Distinct().ToList();

                if (source.FieldBased)
                    model.BreakdownFields.AddRange(sourceRows.Select(r => (source, r.Breakdown, r.Row.Field.Trim())));
                else if (fields.Count > 1)
                    model.BreakdownFields.AddRange(sourceRows.Where(r => r.Row.Field.Trim() != model.DefaultField).Select(r => (source, r.Breakdown, r.Row.Field.Trim())));
                else if (fields[0] != model.DefaultField)
                    model.SourceFields.Add((source, fields[0]));
            }

            // Promote metric filters that are the same in every source the metric reads from.
            var perSource = model.SourceWhere.GroupBy(w => w.Source).ToList();
            var sourcesUsed = metricRows.Select(r => (r.Scope, r.Period)).Distinct().Count();
            if (perSource.Count == sourcesUsed && perSource.Count > 0)
            {
                var first = perSource[0].Select(w => w.Filter).ToList();
                if (perSource.All(g => g.Select(w => w.Filter).SequenceEqual(first)))
                {
                    model.Where.AddRange(first);
                    model.SourceWhere.Clear();
                }
            }

            var covered = metricRows.Select(r => (r.Scope, r.Period)).ToHashSet();
            var scopes = setScopes.Where(s => covered.Any(c => c.Scope == s)).ToList();
            var periods = setPeriods.Where(p => covered.Any(c => c.Period == p)).ToList();
            if (!scopes.SequenceEqual(setScopes))
                model.In = scopes;
            if (!periods.SequenceEqual(setPeriods))
                model.During = periods;
            model.Skips.AddRange(scopes.SelectMany(s => periods.Select(p => (s, p))).Where(x => !covered.Contains(x)));

            set.Metrics.Add(model);
        }

        set.Metrics.RemoveAll(m => failures.ContainsKey(m.Key));

        // The most common breakdown list becomes the set default.
        var common = set.Metrics
            .GroupBy(m => string.Join(",", m.For.Select(b => b.Code)))
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        if (common is not null)
        {
            set.Breakdowns.AddRange(common.First().For);
            foreach (var m in common)
                m.UsesSetBreakdowns = true;
        }
    }

    private static bool IsFunctionOf<T>(Dictionary<ParsedRow, List<ColumnFilter>> rows, string column, Func<ParsedRow, T> key) =>
        rows.GroupBy(r => key(r.Key))
            .All(g => g.Select(r => r.Value.FirstOrDefault(f => f.Column == column)?.Value).Distinct().Count() == 1);

    private static string MostCommon(IEnumerable<string> values) =>
        values.GroupBy(v => v.Trim()).OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault() ?? "";
}
