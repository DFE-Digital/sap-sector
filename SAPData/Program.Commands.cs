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
        var command = Array.FindIndex(args, a => a is "profile-sources" or "catalogue-summary" or "explain" or "export-map");
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
                WriteDataMapExport(Path.Combine(dataMapDir, "datamap.generated.json"));
                return true;
            default:
                return false;
        }
    }

    // Every property's file, key column, value column and filters, one per line, for anyone who needs to look up a
    // mapping without reading the definitions. A test fails if the committed file is out of date.
    private static void WriteDataMapExport(string path)
    {
        var rows = CatalogueDefinitions.Rows();
        File.WriteAllText(path, DataMapExport.ToJson(rows));
        Console.WriteLine($"Wrote {rows.Count} mappings to {path}");
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
