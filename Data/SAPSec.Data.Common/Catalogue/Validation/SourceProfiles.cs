using System.Globalization;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;
using SAPData.Models;

namespace SAPSec.Data.Common.Catalogue.Validation;

/// <summary>
/// The shape of one source file: its columns, and the distinct values of the columns the data map filters on.
/// Column names are normalised with <see cref="ColumnNames.Normalise"/>.
/// </summary>
public sealed class SourceProfile
{
    public SortedSet<string> Columns { get; init; } = new(StringComparer.Ordinal);

    public SortedDictionary<string, SortedSet<string>> Values { get; init; } = new(StringComparer.Ordinal);
}

/// <summary>
/// Profiles of the source files a data map reads, keyed by the DataMap file name (without "manual_" or ".csv").
/// A snapshot is committed so validation can run in CI without the source files.
/// </summary>
public sealed class SourceProfiles
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public SortedDictionary<string, SourceProfile> Files { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public static SourceProfiles Load(string path)
    {
        var loaded = JsonSerializer.Deserialize<SourceProfiles>(File.ReadAllText(path), JsonOptions)
            ?? throw new InvalidDataException($"Could not read source profiles from {path}");

        // The serializer creates collections with default comparers; restore the ones lookups rely on.
        var files = new SortedDictionary<string, SourceProfile>(StringComparer.OrdinalIgnoreCase);
        foreach (var (file, profile) in loaded.Files)
        {
            var values = new SortedDictionary<string, SortedSet<string>>(StringComparer.Ordinal);
            foreach (var (column, columnValues) in profile.Values)
                values[column] = new SortedSet<string>(columnValues, StringComparer.Ordinal);

            files[file] = new SourceProfile { Columns = new SortedSet<string>(profile.Columns, StringComparer.Ordinal), Values = values };
        }

        return new SourceProfiles { Files = files };
    }

    public void Save(string path) => File.WriteAllText(path, JsonSerializer.Serialize(this, JsonOptions) + Environment.NewLine);

    /// <summary>
    /// Reads every file the rows refer to from <paramref name="sourceDir"/>, preferring the "manual_" copy as the
    /// pipeline does. Only filter columns have their values collected, so the snapshot stays small.
    /// </summary>
    public static SourceProfiles Build(IEnumerable<DataMapRow> rows, string sourceDir)
    {
        var profiles = new SourceProfiles();

        foreach (var fileRows in rows.GroupBy(r => r.FileName.Trim(), StringComparer.OrdinalIgnoreCase))
        {
            var path = new[] { $"manual_{fileRows.Key}.csv", $"{fileRows.Key}.csv" }
                .Select(name => Path.Combine(sourceDir, name))
                .FirstOrDefault(File.Exists);

            if (path is null)
                continue;

            var filterColumns = fileRows
                .SelectMany(DataMapFilters.Of)
                .Select(f => ColumnNames.Normalise(f.Column))
                .ToHashSet(StringComparer.Ordinal);

            profiles.Files[fileRows.Key] = Read(path, filterColumns);
        }

        return profiles;
    }

    private static SourceProfile Read(string path, IReadOnlySet<string> filterColumns)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture) { BadDataFound = null, MissingFieldFound = null };
        using var reader = new StreamReader(path);
        using var csv = new CsvParser(reader, config);

        if (!csv.Read())
            return new SourceProfile();

        var headers = csv.Record!.Select(ColumnNames.Normalise).ToArray();
        var profile = new SourceProfile { Columns = new SortedSet<string>(headers, StringComparer.Ordinal) };
        var tracked = headers
            .Select((name, index) => (name, index))
            .Where(h => filterColumns.Contains(h.name))
            .ToList();

        foreach (var (name, _) in tracked)
            profile.Values[name] = new SortedSet<string>(StringComparer.Ordinal);

        while (csv.Read())
        {
            var record = csv.Record!;
            foreach (var (name, index) in tracked)
            {
                if (index < record.Length)
                    profile.Values[name].Add(record[index].Trim());
            }
        }

        return profile;
    }
}

internal static class DataMapFilters
{
    public static IEnumerable<(string Column, string[] Values)> Of(DataMapRow r) =>
        new[]
        {
            (r.Filter, r.FilterValue), (r.Filter2, r.Filter2Value), (r.Filter3, r.Filter3Value),
            (r.Filter4, r.Filter4Value), (r.Filter5, r.Filter5Value), (r.Filter6, r.Filter6Value),
            (r.Filter7, r.Filter7Value), (r.Filter8, r.Filter8Value), (r.Filter9, r.Filter9Value)
        }
        .Where(f => !string.IsNullOrWhiteSpace(f.Item1))
        .Select(f => (f.Item1.Trim(), (f.Item2 ?? "").Split('+')));
}
