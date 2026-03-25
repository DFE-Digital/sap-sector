using CsvHelper;
using Microsoft.Extensions.Configuration;
using SAPData.Models;
using System.Globalization;
using System.Text;

namespace SAPData;

internal class Program
{
    static void Main(string[] args)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        var runningLocally = bool.TryParse(configuration["RunningLocally"], out var val) && val;

        Console.WriteLine($"RunningLocally: {runningLocally}");

        Console.WriteLine("Generating Raw Data Tables and Scripts...");

        // In CI the working directory is often the repo root.
        // Find SAPData.csproj anywhere under the current directory and use its folder.
        string baseDir = FindProjectDirectoryDownwards("SAPData.csproj");

        string dataMapDir = Path.Combine(baseDir, "DataMap");
        string rawInputDir = Path.Combine(dataMapDir, "SourceFiles");
        string cleanedDir = Path.Combine(dataMapDir, "CleanedFiles");
        string dataMapCsv = Path.Combine(dataMapDir, "datamap.csv");
        string sqlDir = Path.Combine(baseDir, "Sql");
        string runAllSqlFile = Path.Combine(sqlDir, "run_all.sql");
        List<string> sqlFiles = new();

        string infrastructureDir = Path.Combine(Directory.GetParent(baseDir)!.FullName, "SAPSec.Data");
        string jsonDir = Path.Combine(infrastructureDir, "Files");
        string generatedJsonDir = Path.Combine(jsonDir, "Generated");
        string tableMappingPath = Path.Combine(sqlDir, "tablemapping.csv");

        Directory.CreateDirectory(cleanedDir);
        Directory.CreateDirectory(sqlDir);
        Directory.CreateDirectory(jsonDir);
        Directory.CreateDirectory(generatedJsonDir);

        // -------------------------------------------------
        // 1. Load DataMap
        // -------------------------------------------------
        List<DataMapRow> dataMaps;
        using (var reader = new StreamReader(dataMapCsv))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            csv.Context.RegisterClassMap<DataMapMapping>();
            dataMaps = csv.GetRecords<DataMapRow>().ToList();
        }

        Console.WriteLine($"Loaded {dataMaps.Count} DataMap rows");

        // -------------------------------------------------
        // 2. Generate raw tables + cleaned files + mapping
        // -------------------------------------------------
        new GenerateRawTables(
            rawInputDir,
            cleanedDir,
            sqlDir,
            tableMappingPath,
            sqlFiles
        ).Run();

        // -------------------------------------------------
        // 3. Generate views
        // -------------------------------------------------
        new GenerateViews(
            dataMaps,
            tableMappingPath,
            sqlDir,
            jsonDir,
            generatedJsonDir,
            sqlFiles
        ).Run();

        // -------------------------------------------------
        // 10. Generate indexes
        // -------------------------------------------------
        new GenerateIndexes(
            sqlDir,
            sqlFiles
        ).Run();

        // -------------------------------------------------
        // 50. Generate similar schools views
        // -------------------------------------------------
        new GenerateSimilarSchoolsViews(
            dataMaps,
            tableMappingPath,
            sqlDir,
            generatedJsonDir,
            sqlFiles
        ).Run();

        // -------------------------------------------------
        // 60. Generate similar schools indexes
        // -------------------------------------------------
        new GenerateSimilarSchoolsIndexes(
            sqlDir,
            sqlFiles
        ).Run();

        var runAllSql = new StringBuilder();
        runAllSql.AppendLine(@"-- ================================================================");
        runAllSql.AppendLine(@"-- run_all.sql");
        runAllSql.AppendLine(@"-- ================================================================");
        runAllSql.AppendLine(@"");
        runAllSql.AppendLine(@"\set ON_ERROR_STOP on");
        runAllSql.AppendLine(@"");
        runAllSql.AppendLine(@"\ir 00_cleanup.sql");

        foreach (var line in sqlFiles.Order())
        {
            runAllSql.AppendLine(@$"\ir {line}");
        }

        File.WriteAllText(runAllSqlFile, runAllSql.ToString());

        Console.WriteLine("Run Complete.");

        // Optional: avoid blocking in CI
        if (!Console.IsInputRedirected)
        {
            Console.ReadLine();
        }
    }

    private static string FindProjectDirectoryDownwards(string projectFileName)
    {
        var startDir = Directory.GetCurrentDirectory();

        // Fast path: if we're already in the project directory.
        var direct = Path.Combine(startDir, projectFileName);
        if (File.Exists(direct))
            return startDir;

        // Primary search (works in GitHub Actions)
        var matches = Directory.GetFiles(startDir, projectFileName, SearchOption.AllDirectories);

        if (matches.Length == 0)
        {
            // ---------- LOCAL FALLBACK ----------
            // If running from bin/Debug/... or bin/Release/..., walk up until we exit /bin
            var dir = new DirectoryInfo(startDir);
            while (dir != null && dir.Name.Equals("bin", StringComparison.OrdinalIgnoreCase) == false)
            {
                dir = dir.Parent;
            }

            // If we found /bin, move one level up (project root candidate)
            if (dir?.Parent != null)
            {
                var projectRootCandidate = dir.Parent.FullName;

                var fallbackMatches = Directory.GetFiles(
                    projectRootCandidate,
                    projectFileName,
                    SearchOption.AllDirectories);

                if (fallbackMatches.Length > 0)
                {
                    var preferredFallback = fallbackMatches
                        .FirstOrDefault(p =>
                            string.Equals(
                                new DirectoryInfo(Path.GetDirectoryName(p)!).Name,
                                "SAPData",
                                StringComparison.OrdinalIgnoreCase))
                        ?? fallbackMatches[0];

                    return Path.GetDirectoryName(preferredFallback)!;
                }
            }

            throw new DirectoryNotFoundException(
                $"Could not find {projectFileName} under {startDir} (including bin fallback)"
            );
        }

        // Prefer SAPData project if multiple found
        var preferred = matches
            .FirstOrDefault(p =>
                string.Equals(
                    new DirectoryInfo(Path.GetDirectoryName(p)!).Name,
                    "SAPData",
                    StringComparison.OrdinalIgnoreCase))
            ?? matches[0];

        return Path.GetDirectoryName(preferred)!;
    }
}
