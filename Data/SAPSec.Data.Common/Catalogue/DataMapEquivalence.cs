using System.Text;
using SAPData.Models;

namespace SAPSec.Data.Common.Catalogue;

/// <summary>
/// Compares two sets of DataMap rows by what the view generator actually reads from them, so a catalogue can be
/// proven to produce the same materialized views as datamap.csv.
/// </summary>
/// <remarks>
/// Mirrors SAPData.GenerateViews.GenerateMaterializedView: rows are grouped into views by Range + Type, then by file.
/// Each file uses the first row's key column, and each property uses its first row's field, data type and filters.
/// Filters are ANDed, so their order is ignored. Values within a filter are ORed, so their order is ignored too.
/// Column names are compared after the same normalisation the generator applies (lower case, non-alphanumeric → '_').
/// </remarks>
public static class DataMapEquivalence
{
    public static IReadOnlyList<string> Differences(IEnumerable<DataMapRow> expected, IEnumerable<DataMapRow> actual)
    {
        var expectedViews = Effective(expected);
        var actualViews = Effective(actual);
        var differences = new List<string>();

        foreach (var view in expectedViews.Keys.Union(actualViews.Keys).Order())
        {
            if (!expectedViews.TryGetValue(view, out var e))
            {
                differences.Add($"{view}: unexpected view");
                continue;
            }

            if (!actualViews.TryGetValue(view, out var a))
            {
                differences.Add($"{view}: missing view");
                continue;
            }

            foreach (var file in e.Keys.Keys.Union(a.Keys.Keys).Order())
            {
                e.Keys.TryGetValue(file, out var expectedKey);
                a.Keys.TryGetValue(file, out var actualKey);
                if (expectedKey != actualKey)
                    differences.Add($"{view} [{file}]: key '{expectedKey}' != '{actualKey}'");
            }

            foreach (var property in e.Properties.Keys.Union(a.Properties.Keys).Order())
            {
                if (!e.Properties.TryGetValue(property, out var ep))
                    differences.Add($"{view}.{property}: unexpected property");
                else if (!a.Properties.TryGetValue(property, out var ap))
                    differences.Add($"{view}.{property}: missing property");
                else if (ep != ap)
                    differences.Add($"{view}.{property}: expected {ep} but was {ap}");
            }
        }

        return differences;
    }

    private sealed record View(Dictionary<string, string> Keys, Dictionary<string, string> Properties);

    private static Dictionary<string, View> Effective(IEnumerable<DataMapRow> rows)
    {
        var views = new Dictionary<string, View>(StringComparer.Ordinal);

        var mapped = rows
            .Where(r => !string.IsNullOrWhiteSpace(r.PropertyName))
            .Where(r => !string.Equals(r.IgnoreMapping?.Trim(), "Y", StringComparison.OrdinalIgnoreCase));

        foreach (var viewRows in mapped.GroupBy(r => $"{r.Type}/{r.Range}"))
        {
            var keys = new Dictionary<string, string>(StringComparer.Ordinal);
            var sources = new Dictionary<string, List<(string File, string Mapping)>>(StringComparer.Ordinal);

            foreach (var fileRows in viewRows.GroupBy(r => (r.FileName ?? "").Trim().TrimStart('﻿')))
            {
                keys[fileRows.Key] = DbCol(fileRows.First().RecordFilterBy);

                foreach (var propertyRows in fileRows.GroupBy(r => r.PropertyName))
                {
                    if (!sources.TryGetValue(propertyRows.Key, out var list))
                        sources[propertyRows.Key] = list = [];

                    list.Add((fileRows.Key, Describe(propertyRows.First())));
                }
            }

            // A property fed by several files is COALESCEd in file order, so keep that order in the description.
            var properties = sources.ToDictionary(
                p => p.Key,
                p => string.Join(" ?? ", p.Value.Select(s => $"{s.File}:{s.Mapping}")),
                StringComparer.Ordinal);

            views[viewRows.Key] = new View(keys, properties);
        }

        return views;
    }

    private static string Describe(DataMapRow r)
    {
        var filters = new[]
            {
                (r.Filter, r.FilterValue), (r.Filter2, r.Filter2Value), (r.Filter3, r.Filter3Value),
                (r.Filter4, r.Filter4Value), (r.Filter5, r.Filter5Value), (r.Filter6, r.Filter6Value),
                (r.Filter7, r.Filter7Value), (r.Filter8, r.Filter8Value), (r.Filter9, r.Filter9Value)
            }
            .Where(f => !string.IsNullOrWhiteSpace(f.Item1))
            .Select(f => $"{DbCol(f.Item1)}=({string.Join('|', (f.Item2 ?? "").Split('+').Order(StringComparer.Ordinal))})")
            .Order(StringComparer.Ordinal);

        return $"{DbCol(r.Field)}:{(r.DataType ?? "").ToLowerInvariant()} where [{string.Join(", ", filters)}]";
    }

    // Same normalisation as SAPData.GenerateViews.DbCol.
    private static string DbCol(string? header)
    {
        if (string.IsNullOrWhiteSpace(header))
            return header ?? "";

        var s = header.Trim().ToLowerInvariant();
        var sb = new StringBuilder(s.Length);
        foreach (var ch in s)
            sb.Append(char.IsLetterOrDigit(ch) ? ch : '_');

        return sb.ToString();
    }
}
