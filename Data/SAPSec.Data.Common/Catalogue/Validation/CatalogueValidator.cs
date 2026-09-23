using System.Text.RegularExpressions;
using SAPData.Models;

namespace SAPSec.Data.Common.Catalogue.Validation;

public sealed record ValidationIssue(string Rule, string View, string Property, string Message)
{
    public override string ToString() => $"[{Rule}] {View}.{Property}: {Message}";
}

/// <summary>
/// Checks expanded data map rows for mistakes the builder can't rule out on its own.
/// Each rule is based on a real error found in datamap.csv.
/// </summary>
public static partial class CatalogueValidator
{
    public const string PeriodInName = "period-in-name";
    public const string TimePeriodFilter = "time-period-filter";
    public const string DuplicateMapping = "duplicate-mapping";
    public const string KeyColumn = "key-column";
    public const string UnknownFile = "unknown-file";
    public const string UnknownColumn = "unknown-column";
    public const string UnknownValue = "unknown-value";

    [GeneratedRegex(@"_(?<scope>Est|LA|Eng)_(?<period>Current|Previous2|Previous)_(?:Num|Pct)$")]
    private static partial Regex NameSuffix();

    [GeneratedRegex(@"^(?<start>\d{4})-(?<end>\d{4})$")]
    private static partial Regex AcademicYearLabel();

    /// <summary>
    /// Validates rows on their own and, when <paramref name="profiles"/> is given, against the source files' columns and values.
    /// </summary>
    public static IReadOnlyList<ValidationIssue> Validate(IEnumerable<DataMapRow> rows, SourceProfiles? profiles = null)
    {
        var mapped = rows
            .Where(r => !string.IsNullOrWhiteSpace(r.PropertyName))
            .Where(r => !string.Equals(r.IgnoreMapping?.Trim(), "Y", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var issues = new List<ValidationIssue>();
        issues.AddRange(mapped.SelectMany(CheckPeriod));
        issues.AddRange(CheckDuplicateMappings(mapped));
        issues.AddRange(CheckKeyColumns(mapped));

        if (profiles is not null)
            issues.AddRange(mapped.SelectMany(r => CheckAgainstSource(r, profiles)));

        return issues;
    }

    private static string View(DataMapRow r) => $"{r.Type}/{r.Range}";

    private static string File(DataMapRow r) => r.FileName.Trim().TrimStart('﻿');

    // A property named "…_Previous2_…" must be a Previous2 row, and its time_period filter must match its year.
    // (datamap.csv mapped Prog8_Avg_LA_Previous2_Num to the current year.)
    private static IEnumerable<ValidationIssue> CheckPeriod(DataMapRow r)
    {
        var suffix = NameSuffix().Match(r.PropertyName);
        if (suffix.Success && suffix.Groups["period"].Value != r.YearDesc)
            yield return new(PeriodInName, View(r), r.PropertyName, $"name says {suffix.Groups["period"].Value} but the row is YearDesc '{r.YearDesc}'");

        var year = AcademicYearLabel().Match(r.Year ?? "");
        if (!year.Success)
            yield break;

        var code = new AcademicYear(int.Parse(year.Groups["start"].Value)).Code;
        foreach (var (column, values) in DataMapFilters.Of(r))
        {
            if (ColumnNames.Normalise(column) == "time_period" && values.Any(v => v.Trim() != code))
                yield return new(TimePeriodFilter, View(r), r.PropertyName, $"time_period filter '{string.Join('+', values)}' doesn't match the row's year {r.Year} ({code})");
        }
    }

    // Two properties reading the same file, field and filters show the same number under different names.
    // (datamap.csv read the grade 4+ column for EngMaths59_Mob_Eng_*, duplicating EngMaths49_Mob_Eng_*.)
    private static IEnumerable<ValidationIssue> CheckDuplicateMappings(IEnumerable<DataMapRow> rows) =>
        rows.GroupBy(r => (View(r), Mapping: $"{File(r)}:{ColumnNames.Normalise(r.Field)}:{FilterSignature(r)}"))
            .Where(g => g.Select(r => r.PropertyName).Distinct().Count() > 1)
            .SelectMany(g => g.Select(r => new ValidationIssue(
                DuplicateMapping, g.Key.Item1, r.PropertyName,
                $"reads the same data as {string.Join(", ", g.Select(x => x.PropertyName).Where(p => p != r.PropertyName).Distinct())}")));

    // The generator keys each file in a view by its first row only, so every row must agree.
    private static IEnumerable<ValidationIssue> CheckKeyColumns(IEnumerable<DataMapRow> rows) =>
        rows.GroupBy(r => (View: View(r), File: File(r)))
            .Where(g => g.Select(r => ColumnNames.Normalise(r.RecordFilterBy)).Distinct().Count() > 1)
            .Select(g => new ValidationIssue(
                KeyColumn, g.Key.View, g.Key.File,
                $"rows use different key columns ({string.Join(", ", g.Select(r => $"'{r.RecordFilterBy}'").Distinct())}); only the first is used"));

    // Fields, key columns and filter columns must exist, and filter values must occur in the file.
    // (datamap.csv filtered grade = 7/8/9 in a file that only has grade bands, so every value was blank.)
    private static IEnumerable<ValidationIssue> CheckAgainstSource(DataMapRow r, SourceProfiles profiles)
    {
        if (!profiles.Files.TryGetValue(File(r), out var profile))
        {
            yield return new(UnknownFile, View(r), r.PropertyName, $"no profile for source file '{File(r)}'; regenerate the source profiles");
            yield break;
        }

        foreach (var (role, column) in new[] { ("field", r.Field), ("key column", r.RecordFilterBy) })
        {
            if (!profile.Columns.Contains(ColumnNames.Normalise(column)))
                yield return new(UnknownColumn, View(r), r.PropertyName, $"{role} '{column}' is not a column in {File(r)}");
        }

        foreach (var (column, values) in DataMapFilters.Of(r))
        {
            var name = ColumnNames.Normalise(column);
            if (!profile.Columns.Contains(name))
            {
                yield return new(UnknownColumn, View(r), r.PropertyName, $"filter column '{column}' is not a column in {File(r)}");
                continue;
            }

            if (!profile.Values.TryGetValue(name, out var known))
                continue;

            var missing = values.Select(v => v.Trim()).Where(v => !known.Contains(v)).ToList();
            if (missing.Count > 0)
                yield return new(UnknownValue, View(r), r.PropertyName, $"filter {column} = '{string.Join("', '", missing)}' matches no rows in {File(r)}");
        }
    }

    private static string FilterSignature(DataMapRow r) =>
        string.Join(";", DataMapFilters.Of(r)
            .Select(f => $"{ColumnNames.Normalise(f.Column)}=({string.Join('|', f.Values.Select(v => v.Trim()).Order(StringComparer.Ordinal))})")
            .Order(StringComparer.Ordinal));
}
