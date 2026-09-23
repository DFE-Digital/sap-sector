using System.Text.RegularExpressions;
using SAPData.Models;
using SAPSec.Data.Common.Catalogue;

namespace SAPSec.DataMapConverter;

internal sealed record ColumnFilter(string Column, string Value)
{
    /// <summary>DataMap encodes OR values with '+'.</summary>
    public string[] Values => Value.Split('+');

    public static IReadOnlyList<ColumnFilter> From(DataMapRow r) =>
        new[]
        {
            (r.Filter, r.FilterValue), (r.Filter2, r.Filter2Value), (r.Filter3, r.Filter3Value),
            (r.Filter4, r.Filter4Value), (r.Filter5, r.Filter5Value), (r.Filter6, r.Filter6Value),
            (r.Filter7, r.Filter7Value), (r.Filter8, r.Filter8Value), (r.Filter9, r.Filter9Value)
        }
        .Where(f => !string.IsNullOrWhiteSpace(f.Item1))
        .Select(f => new ColumnFilter(f.Item1, f.Item2 ?? ""))
        .ToList();
}

internal sealed record MetricKey(string Subtype, string Name, Unit Unit, string Template)
{
    public override string ToString() => $"{Subtype}/{Name}_{Unit}";
}

/// <summary>A datamap row whose property name follows {metric}_{breakdown}_{scope}_{period}_{unit}.</summary>
internal sealed partial record ParsedRow(
    DataMapRow Row,
    MetricKey Metric,
    Breakdown Breakdown,
    Scope Scope,
    Period Period,
    int YearStart,
    IReadOnlyList<ColumnFilter> Filters)
{
    private static readonly IReadOnlyDictionary<string, Breakdown> KnownBreakdowns = typeof(Breakdowns)
        .GetFields()
        .Where(f => f.FieldType == typeof(Breakdown))
        .Select(f => (Breakdown)f.GetValue(null)!)
        .ToDictionary(b => b.Code, StringComparer.Ordinal);

    [GeneratedRegex(@"^(?<metric>[^_]+)_(?<label>.+?)_(?<scope>Est|LA|Eng)_(?<period>Current|Previous2|Previous)_(?<unit>Num|Pct)$")]
    private static partial Regex NamePattern();

    [GeneratedRegex(@"^(?<start>\d{4})-(?<end>\d{4})$")]
    private static partial Regex YearPattern();

    public static bool TryParse(DataMapRow row, out ParsedRow? parsed, out string reason)
    {
        parsed = null;
        var match = NamePattern().Match(row.PropertyName.Trim());

        if (!match.Success)
            return Fail("property name doesn't follow {metric}_{breakdown}_{scope}_{period}_{unit}", out reason);

        var scope = match.Groups["scope"].Value switch
        {
            "Est" => Scope.Establishment,
            "LA" => Scope.LA,
            _ => Scope.England
        };

        if (scope.ToString() != row.Range)
            return Fail($"name scope {scope} doesn't match Range '{row.Range}'", out reason);

        var period = Enum.Parse<Period>(match.Groups["period"].Value);
        if (period.ToString() != row.YearDesc)
            return Fail($"name period {period} doesn't match YearDesc '{row.YearDesc}'", out reason);

        var year = YearPattern().Match(row.Year ?? "");
        if (!year.Success || int.Parse(year.Groups["end"].Value) != int.Parse(year.Groups["start"].Value) + 1)
            return Fail($"Year '{row.Year}' isn't an academic year", out reason);

        if (!TryDataType(row.DataType, out _))
            return Fail($"unsupported DataType '{row.DataType}'", out reason);

        var filters = ColumnFilter.From(row);
        if (filters.Any(f => f.Values.Any(string.IsNullOrWhiteSpace)))
            return Fail("a filter has an empty value", out reason);

        if (string.IsNullOrWhiteSpace(row.Field))
            return Fail("no Field", out reason);

        if (!TryBreakdown(match.Groups["label"].Value, out var breakdown, out var template))
            return Fail($"breakdown label '{match.Groups["label"].Value}' has no known breakdown code", out reason);

        var metric = new MetricKey(row.Subtype, match.Groups["metric"].Value, Enum.Parse<Unit>(match.Groups["unit"].Value), template);
        parsed = new ParsedRow(row, metric, breakdown, scope, period, int.Parse(year.Groups["start"].Value), filters);
        reason = "";
        return true;
    }

    public static bool TryDataType(string? value, out DataType dataType)
    {
        dataType = default;
        var match = Enum.GetValues<DataType>().Where(d => string.Equals(d.DataMapValue(), value?.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
        if (match.Count != 1)
            return false;

        dataType = match[0];
        return true;
    }

    /// <summary>Sort key following the declaration order in <see cref="Breakdowns"/>; custom breakdowns last.</summary>
    public static int Order(Breakdown breakdown)
    {
        var index = KnownBreakdowns.Keys.ToList().IndexOf(breakdown.Code);
        return index < 0 ? int.MaxValue : index;
    }

    public static bool IsKnown(Breakdown breakdown) => KnownBreakdowns.ContainsKey(breakdown.Code);

    public static string KnownName(Breakdown breakdown) => typeof(Breakdowns)
        .GetFields()
        .Single(f => f.FieldType == typeof(Breakdown) && ((Breakdown)f.GetValue(null)!).Code == breakdown.Code)
        .Name;

    // "Boy" → Boys with the default template; "Reading_Boy_Cohort" → Boys with "{metric}_Reading_{breakdown}_Cohort_…";
    // a single unknown token such as "Avg" becomes a custom breakdown.
    private static bool TryBreakdown(string label, out Breakdown breakdown, out string template)
    {
        const string suffix = "_{scope}_{period}_{unit}";

        if (KnownBreakdowns.TryGetValue(label, out breakdown!))
        {
            template = Data.Common.Catalogue.Metric.DefaultNameTemplate;
            return true;
        }

        var parts = label.Split('_');
        var known = parts.Select((p, i) => (p, i)).Where(x => KnownBreakdowns.ContainsKey(x.p)).ToList();

        if (known.Count == 1)
        {
            breakdown = KnownBreakdowns[known[0].p];
            parts[known[0].i] = "{breakdown}";
            template = "{metric}_" + string.Join('_', parts) + suffix;
            return true;
        }

        if (parts.Length == 1)
        {
            breakdown = new Breakdown(label, label);
            template = Data.Common.Catalogue.Metric.DefaultNameTemplate;
            return true;
        }

        template = "";
        return false;
    }

    private static bool Fail(string message, out string reason)
    {
        reason = message;
        return false;
    }
}

internal sealed class SourceModel
{
    public required Scope Scope { get; init; }
    public required Period Period { get; init; }
    public required string File { get; init; }
    public string Org { get; set; } = "";
    public string Key { get; set; } = "";
    public List<ColumnFilter> Filters { get; } = [];

    /// <summary>Wide file where each breakdown is its own column (no breakdown filters).</summary>
    public bool FieldBased { get; set; }

    public List<(Breakdown Breakdown, List<ColumnFilter> Filters)> Provides { get; } = [];

    public string VariableName => Scope switch
    {
        Scope.Establishment => "establishment",
        Scope.LA => "la",
        _ => "england"
    } + Period;
}

internal sealed class MetricModel
{
    public required MetricKey Key { get; init; }
    public DataType DataType { get; set; }
    public string DefaultField { get; set; } = "";
    public List<Breakdown> For { get; } = [];
    public List<Scope>? In { get; set; }
    public List<Period>? During { get; set; }
    public List<(Scope Scope, Period Period)> Skips { get; } = [];
    public List<ColumnFilter> Where { get; } = [];
    public List<(SourceModel Source, ColumnFilter Filter)> SourceWhere { get; } = [];
    public List<(SourceModel Source, string Field)> SourceFields { get; } = [];
    public List<(SourceModel Source, Breakdown Breakdown, string Field)> BreakdownFields { get; } = [];
    public bool UsesSetBreakdowns { get; set; }
}

internal sealed class SetModel
{
    public required string Subtype { get; init; }
    public SortedDictionary<Period, int> Years { get; } = [];
    public List<SourceModel> Sources { get; } = [];
    public List<Breakdown> Breakdowns { get; } = [];
    public List<MetricModel> Metrics { get; } = [];
}
