using System.Text.RegularExpressions;
using SAPData.Models;
using SAPSec.Data.Common.Catalogue;
using SAPSec.Data.Common.Catalogue.Validation;

namespace SAPData;

/// <summary>
/// Checks the files about to be loaded, before any SQL runs: every column and filter value the data map uses, and
/// every column the establishment view reads, must exist in them. A schema change in a newly downloaded file (a
/// renamed column, relabelled breakdown or missing year) then stops the pipeline before the maintenance page or
/// any database change, so the live views keep their previous data.
/// </summary>
internal static partial class SourceFileCheck
{
    // Rules that compare the data map with the files. The others don't depend on the files and run earlier.
    private static readonly HashSet<string> FileRules =
    [
        CatalogueValidator.UnknownFile,
        CatalogueValidator.UnknownColumn,
        CatalogueValidator.UnknownValue,
    ];

    [GeneratedRegex(@"\bt\.""(?<column>[^""]+)""")]
    private static partial Regex RawColumnReference();

    public static IReadOnlyList<string> Run(
        IReadOnlyList<DataMapRow> rows,
        IReadOnlyDictionary<string, string> tableMappings,
        IReadOnlyDictionary<string, string> sourceFilesByTable)
    {
        var tableMap = new Dictionary<string, string>(tableMappings, StringComparer.OrdinalIgnoreCase);

        string? FileFor(string datasetKey) =>
            GenerateViews.TryResolveRawTable(tableMap, datasetKey, out var table)
            && table is not null
            && sourceFilesByTable.TryGetValue(table, out var path)
                ? path
                : null;

        var issues = new List<string>();

        var profiles = SourceProfiles.Build(rows, FileFor);
        issues.AddRange(CatalogueValidator.Validate(rows, profiles)
            .Where(i => FileRules.Contains(i.Rule))
            .Select(i => i.ToString()));

        issues.AddRange(CheckEstablishment(tableMap, FileFor));

        return issues;
    }

    // v_establishment is hand-written SQL rather than catalogue rows, so check the GIAS columns it reads directly.
    private static IEnumerable<string> CheckEstablishment(Dictionary<string, string> tableMap, Func<string, string?> fileFor)
    {
        if (!GenerateViews.TryResolveManagedDatasetKey(
                GenerateViews.LoadRawSources(), tableMap, "GIAS", "All establishment", "Metadata", "Current", out var datasetKey))
        {
            yield return "[establishment] v_establishment: no GIAS establishment file found among the downloaded files";
            yield break;
        }

        var path = fileFor(datasetKey);
        if (path is null)
        {
            yield return $"[establishment] v_establishment: could not find the file for '{datasetKey}'";
            yield break;
        }

        var header = File.ReadLines(path).FirstOrDefault() ?? "";
        var columns = header.Split(',').Select(h => ColumnNames.Normalise(h.Trim('"'))).ToHashSet(StringComparer.Ordinal);

        var used = RawColumnReference()
            .Matches(GenerateViews.GenerateEstablishmentDimensionView("t"))
            .Select(m => m.Groups["column"].Value)
            .Distinct();

        foreach (var column in used.Where(c => !columns.Contains(c)))
            yield return $"[establishment] v_establishment: reads column '{column}', which is not in {Path.GetFileName(path)}";
    }
}
