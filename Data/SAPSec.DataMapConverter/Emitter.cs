using System.Text;
using SAPData.Models;
using SAPSec.Data.Common.Catalogue;

namespace SAPSec.DataMapConverter;

/// <summary>Writes the inferred model as a C# catalogue definition.</summary>
internal static class Emitter
{
    public static string Emit(
        string type,
        string className,
        IReadOnlyList<SetModel> sets,
        IReadOnlyList<(DataMapRow Row, string Reason)> raw,
        IReadOnlyList<string> droppedDuplicates)
    {
        var methodNames = MethodNames(sets);
        var sb = new StringBuilder();

        sb.AppendLine("using SAPData.Models;");
        sb.AppendLine();
        sb.AppendLine("namespace SAPSec.Data.Common.Catalogue.Definitions;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// {type} measures.");
        sb.AppendLine("/// Converted from datamap.csv by SAPSec.DataMapConverter; maintained by hand from now on.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public static class {className}");
        sb.AppendLine("{");
        sb.AppendLine($"    public const string Type = {Lit(type)};");
        sb.AppendLine();

        var all = methodNames.Values.ToList();
        if (raw.Count > 0 || droppedDuplicates.Count > 0)
            all.Add("Unmodelled");
        sb.AppendLine($"    public static IReadOnlyList<MeasureSet> MeasureSets() => [{string.Join(", ", all.Select(n => n + "()"))}];");

        foreach (var set in sets)
            EmitSet(sb, set, methodNames[set]);

        if (raw.Count > 0 || droppedDuplicates.Count > 0)
            EmitRaw(sb, raw, droppedDuplicates);

        sb.AppendLine("}");
        return sb.ToString();
    }

    private static Dictionary<SetModel, string> MethodNames(IReadOnlyList<SetModel> sets)
    {
        var names = new Dictionary<SetModel, string>();
        foreach (var group in sets.GroupBy(s => Identifier(s.Subtype)))
        {
            var i = 0;
            foreach (var set in group)
                names[set] = i++ == 0 ? group.Key : $"{group.Key}{i}";
        }
        return names;
    }

    private static void EmitSet(StringBuilder sb, SetModel set, string methodName)
    {
        sb.AppendLine();
        sb.AppendLine($"    public static MeasureSet {methodName}()");
        sb.AppendLine("    {");

        foreach (var (period, year) in set.Years)
            sb.AppendLine($"        var {YearVar(period)} = new AcademicYear({year});");
        sb.AppendLine();

        foreach (var source in set.Sources)
        {
            sb.Append($"        var {source.VariableName} = {SourceFactory(source.Org, source.File)}");
            sb.Append($"\n            .KeyedBy({Lit(source.Key)})");

            foreach (var f in source.Filters)
                sb.Append($"\n            .Where({Lit(f.Column)}, {Values(f, set, source.Period)})");

            foreach (var (breakdown, filters) in source.Provides)
            {
                var args = filters.Select(f => $"({Lit(f.Column)}, {Lit(f.Value)})");
                sb.Append($"\n            .Provides({string.Join(", ", new[] { BreakdownExpr(breakdown) }.Concat(args))})");
            }

            sb.AppendLine(";");
            sb.AppendLine();
        }

        sb.Append($"        return new MeasureSet(Type, {Lit(set.Subtype)})");
        foreach (var period in set.Years.Keys)
            sb.Append($"\n            .Year(Period.{period}, {YearVar(period)})");
        foreach (var source in set.Sources)
            sb.Append($"\n            .Source(Scope.{source.Scope}, Period.{source.Period}, {source.VariableName})");
        sb.Append($"\n            .Breakdowns({string.Join(", ", set.Breakdowns.Select(BreakdownExpr))})");

        foreach (var m in set.Metrics)
        {
            sb.Append($"\n            .Metric(new Metric({Lit(m.Key.Name)}, {Lit(m.DefaultField)})");
            var calls = new List<string>();

            if (m.Key.Unit == Unit.Pct)
                calls.Add(".Percentage()");
            if (m.DataType != DataType.Double)
                calls.Add($".OfType(DataType.{m.DataType})");
            if (m.Key.Template != Metric.DefaultNameTemplate)
                calls.Add($".Named({Lit(m.Key.Template)})");
            if (!m.UsesSetBreakdowns)
                calls.Add($".For({string.Join(", ", m.For.Select(BreakdownExpr))})");
            if (m.In is not null)
                calls.Add($".In({string.Join(", ", m.In.Select(s => $"Scope.{s}"))})");
            if (m.During is not null)
                calls.Add($".During({string.Join(", ", m.During.Select(p => $"Period.{p}"))})");
            calls.AddRange(m.Skips.Select(s => $".Skip(Scope.{s.Scope}, Period.{s.Period})"));
            calls.AddRange(m.Where.Select(f => $".Where({Lit(f.Column)}, {string.Join(", ", f.Values.Select(Lit))})"));
            calls.AddRange(m.SourceWhere.Select(w => $".Where({w.Source.VariableName}, {Lit(w.Filter.Column)}, {string.Join(", ", w.Filter.Values.Select(Lit))})"));
            calls.AddRange(m.SourceFields.Select(f => $".Field({f.Source.VariableName}, {Lit(f.Field)})"));
            calls.AddRange(m.BreakdownFields.Select(f => $".Field({f.Source.VariableName}, {BreakdownExpr(f.Breakdown)}, {Lit(f.Field)})"));

            foreach (var call in calls)
                sb.Append($"\n                {call}");
            sb.Append(')');
        }

        sb.AppendLine(";");
        sb.AppendLine("    }");
    }

    private static void EmitRaw(StringBuilder sb, IReadOnlyList<(DataMapRow Row, string Reason)> raw, IReadOnlyList<string> dropped)
    {
        sb.AppendLine();
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Rows copied from datamap.csv as-is because they don't fit the catalogue model.");
        sb.AppendLine("    /// Each needs reviewing: most are data map mistakes preserved so the generated views don't change.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public static MeasureSet Unmodelled()");
        sb.AppendLine("    {");

        foreach (var d in dropped)
            sb.AppendLine($"        // DROPPED: {d}");
        if (dropped.Count > 0 && raw.Count > 0)
            sb.AppendLine();

        sb.Append("        return new MeasureSet(Type, \"Unmodelled\")");
        foreach (var (row, reason) in raw)
        {
            sb.Append($"\n            // {reason}");
            sb.Append("\n            .Row(new DataMapRow");
            sb.Append("\n            {");
            foreach (var (name, value) in RowProperties(row))
                sb.Append($"\n                {name} = {Lit(value)},");
            sb.Append("\n            })");
        }
        sb.AppendLine(";");
        sb.AppendLine("    }");
    }

    private static IEnumerable<(string Name, string Value)> RowProperties(DataMapRow row) =>
        typeof(DataMapRow).GetProperties()
            .Where(p => p.PropertyType == typeof(string))
            .Select(p => (p.Name, Value: ((string?)p.GetValue(row) ?? "").Trim()))
            .Where(p => p.Value.Length > 0);

    // time_period values that match the source's year are written as year.Code so rolling a year only changes the year.
    private static string Values(ColumnFilter filter, SetModel set, Period period) =>
        filter.Values.Length == 1 && set.Years.TryGetValue(period, out var year) && filter.Value == new AcademicYear(year).Code
            ? $"{YearVar(period)}.Code"
            : string.Join(", ", filter.Values.Select(Lit));

    private static string SourceFactory(string org, string file) => org switch
    {
        "EES" => $"Source.Ees({Lit(file)})",
        "CSCP" => $"Source.Cscp({Lit(file)})",
        "GIAS" => $"Source.Gias({Lit(file)})",
        _ => $"Source.From({Lit(org)}, {Lit(file)})"
    };

    private static string BreakdownExpr(Breakdown b) =>
        ParsedRow.IsKnown(b) ? $"Breakdowns.{ParsedRow.KnownName(b)}" : $"new Breakdown({Lit(b.Code)}, {Lit(b.Description)})";

    private static string YearVar(Period period) => char.ToLowerInvariant(period.ToString()[0]) + period.ToString()[1..];

    private static string Identifier(string value)
    {
        var id = new string(value.Where(char.IsLetterOrDigit).ToArray());
        return id.Length == 0 || char.IsDigit(id[0]) ? "Set" + id : id;
    }

    private static string Lit(string value) =>
        "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
}
