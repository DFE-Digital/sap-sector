using System.Globalization;
using CsvHelper;
using SAPData.Models;
using SAPSec.Data.Common.Catalogue;
using SAPSec.DataMapConverter;

// One-off tool: converts the datamap.csv rows for one Type into a C# catalogue definition, and verifies the
// catalogue produces the same effective mapping as the CSV before writing it.
//
// Usage: dotnet run --project Data/SAPSec.DataMapConverter -- <datamap.csv> <Type> <ClassName> <output.cs>

if (args.Length != 4)
{
    Console.Error.WriteLine("Usage: <datamap.csv> <Type> <ClassName> <output.cs>");
    return 1;
}

var (csvPath, type, className, outputPath) = (args[0], args[1], args[2], args[3]);

List<DataMapRow> csvRows;
using (var reader = new StreamReader(csvPath))
using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
{
    csv.Context.RegisterClassMap<DataMapMapping>();
    csvRows = csv.GetRecords<DataMapRow>()
        .Where(r => r.Type == type)
        .Where(r => !string.IsNullOrWhiteSpace(r.PropertyName))
        .Where(r => !string.Equals(r.IgnoreMapping?.Trim(), "Y", StringComparison.OrdinalIgnoreCase))
        .ToList();
}

Console.WriteLine($"{csvRows.Count} mapped rows for {type}");

// The generator only uses the first row for a property in a view; later duplicates from the same file are dead.
var kept = new List<DataMapRow>();
var dropped = new List<string>();
foreach (var group in csvRows.GroupBy(r => (r.Range, r.PropertyName)))
{
    kept.Add(group.First());

    foreach (var duplicate in group.Skip(1))
    {
        if (duplicate.FileName.Trim() != group.First().FileName.Trim())
        {
            Console.Error.WriteLine($"{group.Key.PropertyName} is mapped from two files; the generator COALESCEs them. Resolve this in datamap.csv first.");
            return 1;
        }

        dropped.Add($"duplicate {duplicate.Range} {duplicate.PropertyName} (YearDesc={duplicate.YearDesc}, " +
                    $"filters: {string.Join(", ", ColumnFilter.From(duplicate).Select(f => $"{f.Column}={f.Value}"))}); the generator only used the first.");
    }
}

var parsed = new List<ParsedRow>();
var unparsed = new List<(DataMapRow Row, string Reason)>();
foreach (var row in kept)
{
    if (ParsedRow.TryParse(row, out var p, out var reason))
        parsed.Add(p!);
    else
        unparsed.Add((row, $"{row.PropertyName}: {reason}"));
}

var forcedRaw = new Dictionary<MetricKey, string>();
var propertyToMetric = parsed.ToDictionary(p => (p.Row.Range, p.Row.PropertyName), p => p.Metric);

while (true)
{
    var result = Inference.Infer(parsed.Where(p => !forcedRaw.ContainsKey(p.Metric)).ToList());
    foreach (var (metric, reason) in result.Failures)
        forcedRaw.TryAdd(metric, reason);

    if (result.Failures.Count > 0)
        continue;

    var raw = unparsed
        .Concat(parsed.Where(p => forcedRaw.ContainsKey(p.Metric)).Select(p => (p.Row, $"{p.Row.PropertyName}: {forcedRaw[p.Metric]}")))
        .ToList();

    var measureSets = result.Sets.Select(s => Builder.Build(type, s)).Append(Builder.BuildRaw(type, raw.Select(r => r.Row))).ToList();
    var differences = DataMapEquivalence.Differences(csvRows, DataMapCatalogue.Expand(measureSets));

    if (differences.Count == 0)
    {
        File.WriteAllText(outputPath, Emitter.Emit(type, className, result.Sets, raw, dropped));

        Console.WriteLine($"Verified and wrote {outputPath}");
        Console.WriteLine($"  measure sets: {result.Sets.Count}, metrics: {result.Sets.Sum(s => s.Metrics.Count)}, " +
                          $"sources: {result.Sets.Sum(s => s.Sources.Count)}, unmodelled rows: {raw.Count}, dropped duplicates: {dropped.Count}");
        foreach (var (_, reason) in raw)
            Console.WriteLine($"  unmodelled: {reason}");
        return 0;
    }

    // Keep any metric that doesn't round-trip as raw rows, then try again.
    var failing = differences
        .Select(d => d.Split(':')[0])
        .Select(d => d.Split('.'))
        .Where(p => p.Length == 2)
        .Select(p => (Range: p[0].Split('/')[1], Property: p[1]))
        .Where(propertyToMetric.ContainsKey)
        .Select(k => propertyToMetric[k])
        .Where(m => !forcedRaw.ContainsKey(m))
        .Distinct()
        .ToList();

    if (failing.Count == 0)
    {
        Console.Error.WriteLine("Catalogue doesn't match datamap.csv and no metric could be isolated:");
        foreach (var d in differences.Take(50))
            Console.Error.WriteLine($"  {d}");
        return 1;
    }

    foreach (var metric in failing)
        forcedRaw[metric] = "didn't round-trip through the catalogue model";
}
