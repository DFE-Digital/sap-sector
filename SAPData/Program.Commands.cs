using SAPData.Models;
using SAPSec.Data.Common.Catalogue;
using SAPSec.Data.Common.Catalogue.Definitions;
using SAPSec.Data.Common.Catalogue.Validation;

namespace SAPData;

// Developer commands: dotnet run --project SAPData -- <command>. They run instead of the pipeline and load nothing.
internal partial class Program
{
    // Returns true when args name a command, which has then run instead of the pipeline.
    private static bool RunDeveloperCommand(string[] args, string dataMapDir, string rawInputDir, string sourceProfilesPath)
    {
        var command = Array.FindIndex(args, a => a is "profile-sources" or "catalogue-summary" or "explain" or "export-map" or "import-map");
        if (command < 0)
            return false;

        switch (args[command])
        {
            case "profile-sources":
                WriteSourceProfiles(rawInputDir, sourceProfilesPath);
                return true;
            case "catalogue-summary":
                WriteCatalogueSummary(rawInputDir);
                return true;
            case "explain":
                ExplainProperty(args.ElementAtOrDefault(command + 1) ?? "");
                return true;
            case "export-map":
                WriteMappingList(MappingListPath(dataMapDir), force: args.Contains("--force"));
                return true;
            case "import-map":
                ImportMappingList(MappingListPath(dataMapDir), OverridesPath(dataMapDir), sourceProfilesPath);
                return true;
            default:
                return false;
        }
    }

    private static string MappingListPath(string dataMapDir) => Path.Combine(dataMapDir, "datamap.generated.json");

    private static string OverridesPath(string dataMapDir) =>
        Path.Combine(Directory.GetParent(dataMapDir)!.Parent!.FullName, "Data", "SAPSec.Data.Common", "Catalogue", MappingOverrides.FileName);

    // The mapping list: every property's file, key column, value column and filters, one per line. A test fails if the
    // committed list is out of date. Won't overwrite edits that haven't been imported yet, unless --force.
    private static void WriteMappingList(string path, bool force)
    {
        var codeRows = CatalogueDefinitions.CodeRows();
        var json = DataMapExport.ToJson(codeRows, MappingOverrides.Embedded());

        if (!force && File.Exists(path) && HasUnimportedEdits(File.ReadAllText(path), json, codeRows))
        {
            Console.Error.WriteLine($"{path} has edits that haven't been imported. Run: {DataMapExport.ImportCommand}");
            Console.Error.WriteLine("(or add --force to discard them).");
            Environment.ExitCode = 1;
            return;
        }

        File.WriteAllText(path, json);
        Console.WriteLine($"Wrote the mapping list to {path}");
    }

    // A list generated from the current definitions that no longer matches what they generate has been edited by hand.
    private static bool HasUnimportedEdits(string current, string expected, IReadOnlyList<DataMapRow> codeRows)
    {
        if (current.ReplaceLineEndings("\n") == expected)
            return false;

        try
        {
            return DataMapExport.Parse(current).CodeVersion == DataMapExport.CodeVersion(codeRows);
        }
        catch (CatalogueException)
        {
            return true;
        }
    }

    // Turns edits to the mapping list into overrides, after checking them with the validation rules and the source
    // file snapshot. Nothing is written if anything fails.
    private static void ImportMappingList(string listPath, string overridesPath, string sourceProfilesPath)
    {
        var codeRows = CatalogueDefinitions.CodeRows();
        MappingOverrides overrides;
        try
        {
            var (version, mappings) = DataMapExport.Parse(File.ReadAllText(listPath));
            if (version != DataMapExport.CodeVersion(codeRows))
            {
                throw new CatalogueException(
                    "The definitions have changed since this list was generated, so importing it could undo those changes. " +
                    $"Keep a copy of your edits, run {DataMapExport.Command} --force, and make them again.");
            }

            overrides = MappingOverrides.FromEditedList(codeRows, mappings);
        }
        catch (CatalogueException e)
        {
            Console.Error.WriteLine(e.Message);
            Environment.ExitCode = 1;
            return;
        }

        var profiles = File.Exists(sourceProfilesPath) ? SourceProfiles.Load(sourceProfilesPath) : null;
        var issues = CatalogueValidator.Validate(overrides.ApplyTo(codeRows), profiles);
        if (issues.Count > 0)
        {
            foreach (var issue in issues)
                Console.Error.WriteLine(issue);

            Console.Error.WriteLine($"Nothing imported: {issues.Count} issue(s). If you added a source file, profile it first: " +
                                    "dotnet run --project SAPData -- profile-sources");
            Environment.ExitCode = 1;
            return;
        }

        var codeKeys = codeRows.Where(r => !string.IsNullOrWhiteSpace(r.PropertyName))
            .Select(r => Mapping.FromRow(r).Key)
            .ToHashSet(StringComparer.Ordinal);
        var changed = overrides.Mappings.Count(m => codeKeys.Contains(m.Key));

        File.WriteAllText(overridesPath, overrides.ToJson());
        File.WriteAllText(listPath, DataMapExport.ToJson(codeRows, overrides));

        Console.WriteLine($"Imported {overrides.Mappings.Count} override(s): {changed} changed, {overrides.Mappings.Count - changed} added.");
        Console.WriteLine($"Written to {overridesPath}; the mapping list marks them \"overridden\": true.");
        Console.WriteLine("Next: run the tests and raise a pull request with both files.");
    }

    // Snapshot of the source files' columns and filter values, committed so catalogue tests can check fields and
    // filter values in CI. Regenerate after adding or changing a source file: dotnet run --project SAPData -- profile-sources
    private static void WriteSourceProfiles(string sourceDir, string path)
    {
        var rows = CatalogueDefinitions.Rows();
        var profiles = SourceProfiles.Build(rows, sourceDir);
        profiles.Save(path);

        var missing = rows.Select(r => r.FileName.Trim()).Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(f => !profiles.Files.ContainsKey(f))
            .ToList();

        Console.WriteLine($"Wrote {profiles.Files.Count} source profile(s) to {path}");
        foreach (var file in missing)
            Console.Error.WriteLine($"Not found in {sourceDir}: {file}.csv (or manual_{file}.csv)");
    }

    // Lists each dataset's years and the source files it reads, marking any missing from the source folder.
    // Use when preparing a new data year: dotnet run --project SAPData -- catalogue-summary
    private static void WriteCatalogueSummary(string sourceDir)
    {
        bool Present(string file) =>
            File.Exists(Path.Combine(sourceDir, $"{file}.csv")) || File.Exists(Path.Combine(sourceDir, $"manual_{file}.csv"));

        foreach (var type in CatalogueDefinitions.Rows().GroupBy(r => r.Type))
        {
            Console.WriteLine(type.Key);

            foreach (var period in type.GroupBy(r => r.YearDesc).OrderBy(g => g.Key))
            {
                var label = string.IsNullOrEmpty(period.Key) ? "(no year)" : $"{period.Key} {period.First().Year}";
                Console.WriteLine($"  {label}");

                foreach (var file in period.Select(r => r.FileName.Trim()).Distinct().Order())
                    Console.WriteLine($"    {(Present(file) ? "  " : "! ")}{file}");
            }
        }

        Console.WriteLine();
        Console.WriteLine($"! = not found in {sourceDir}");
    }

    // Shows where one property's value comes from: file, key column, field and the filters that pick its rows.
    // dotnet run --project SAPData -- explain Attainment8_EAL_Est_Current_Num (or part of a name to search)
    private static void ExplainProperty(string name)
    {
        var rows = CatalogueDefinitions.Rows().ToList();
        var matches = rows.Where(r => string.Equals(r.PropertyName, name, StringComparison.OrdinalIgnoreCase)).ToList();

        if (matches.Count == 0)
        {
            var similar = rows.Select(r => r.PropertyName)
                .Where(p => name.Length > 0 && p.Contains(name, StringComparison.OrdinalIgnoreCase))
                .Distinct().Order().ToList();

            Console.WriteLine(similar.Count == 0
                ? $"No property named '{name}'. Usage: dotnet run --project SAPData -- explain <PropertyName>"
                : $"No property named '{name}'. Did you mean one of these?");
            foreach (var property in similar.Take(30))
                Console.WriteLine($"  {property}");
            if (similar.Count > 30)
                Console.WriteLine($"  … and {similar.Count - 30} more");
            return;
        }

        foreach (var r in matches)
        {
            Console.WriteLine(r.PropertyName);
            Console.WriteLine($"  Dataset:     {r.Type} ({r.Subtype}), {r.Range}");
            Console.WriteLine($"  Year:        {(string.IsNullOrEmpty(r.YearDesc) ? "(none)" : $"{r.YearDesc} {r.Year}")}");
            Console.WriteLine($"  File:        {r.FileName} ({r.Source})");
            Console.WriteLine($"  Key column:  {r.RecordFilterBy}");
            Console.WriteLine($"  Value from:  {r.Field}");
            if (MappingOverrides.Embedded().Keys.Contains(Mapping.FromRow(r).Key))
                Console.WriteLine($"  Overridden:  yes, in {MappingOverrides.FileName} (imported from the mapping list)");

            var filters = DataMapFilters.Of(r)
                .Select(f => f.Values.Length == 1
                    ? $"{f.Column} = '{f.Values[0]}'"
                    : $"{f.Column} in ('{string.Join("', '", f.Values)}')")
                .ToList();
            Console.WriteLine($"  Rows where:  {(filters.Count == 0 ? "(every row)" : string.Join(Environment.NewLine + "          and  ", filters))}");
            Console.WriteLine();
        }
    }
}
