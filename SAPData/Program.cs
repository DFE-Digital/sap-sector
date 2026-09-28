using Microsoft.Extensions.Configuration;
using Sentry;
using SAPSec.Data.Common;
using SAPSec.Data.Common.Catalogue.Definitions;

namespace SAPData;

internal partial class Program
{
    static void Main(string[] args)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets<Program>()
            .Build();

        using var sentry = InitialiseSentry(configuration);

        try
        {
            var runningLocally = bool.TryParse(configuration["RunningLocally"], out var val) && val;

            Console.WriteLine($"RunningLocally: {runningLocally}");

            Console.WriteLine("Generating Raw Data Tables and Scripts...");

            // In CI the working directory is often the repo root.
            // Find SAPData.csproj anywhere under the current directory and use its folder.
            string baseDir = Project.FindProjectDirectoryDownwards("SAPData.csproj");

            string dataMapDir = Path.Combine(baseDir, "DataMap");
            string rawInputDir = Path.Combine(dataMapDir, "SourceFiles");
            string cleanedDir = Path.Combine(dataMapDir, "CleanedFiles");
            string sqlDir = Path.Combine(baseDir, "Sql");
            string rawTablesToRebuildPath = ResolveRawTablesToRebuildPath(baseDir, configuration);
            string runAllSqlFile = Path.Combine(sqlDir, "run_all.sql");
            List<string> sqlFiles = new();

            string infrastructureDir = Path.Combine(Directory.GetParent(baseDir)!.FullName, "SAPSec.Infrastructure");
            string jsonDir = Path.Combine(infrastructureDir, "Data", "Files");
            string generatedJsonDir = Path.Combine(jsonDir, "Generated");
            string primaryJsonDir = Path.Combine(jsonDir, "PrimarySchools");
            string tableMappingPath = Path.Combine(sqlDir, "tablemapping.csv");
            string sourceProfilesPath = Path.Combine(dataMapDir, "source-profiles.json");

            if (RunDeveloperCommand(args, rawInputDir, sourceProfilesPath))
                return;

            Directory.CreateDirectory(cleanedDir);
            Directory.CreateDirectory(sqlDir);
            Directory.CreateDirectory(jsonDir);
            Directory.CreateDirectory(generatedJsonDir);
            Directory.CreateDirectory(primaryJsonDir);

            // -------------------------------------------------
            // 1. Load and check the data map (SAPSec.Data.Common/Catalogue/Definitions)
            // -------------------------------------------------
            var dataMaps = CatalogueDefinitions.Rows().ToList();
            ValidateCatalogue(dataMaps);

            Console.WriteLine($"Loaded {dataMaps.Count} DataMap rows");

            var rebuildAllRawTables = ShouldRebuildAllRawTables(configuration);
            var incremental = IsIncremental(configuration);
            var logicalKeysToRebuild = rebuildAllRawTables
                ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                : LoadLogicalKeysToRebuild(rawTablesToRebuildPath);

            // Incremental: the database reloads and rebuilds only what changed, so the SQL covers everything.
            // Listed raw tables are still forced to reload.
            var generateAllSql = rebuildAllRawTables || incremental;

            WriteCleanupSql(
                Path.Combine(sqlDir, "00_cleanup.sql"),
                incremental ? [] : logicalKeysToRebuild.Select(GenerateRawTables.GenerateShortTableName),
                rebuildAllRawTables,
                incremental);

            // -------------------------------------------------
            // 2. Raw tables (01_, 02_): clean each downloaded file and load it as is, then check the files
            //    against the data map before anything is loaded
            // -------------------------------------------------
            var rawTables = new GenerateRawTables(
                rawInputDir,
                cleanedDir,
                sqlDir,
                tableMappingPath,
                sqlFiles,
                logicalKeysToRebuild,
                rebuildAllRawTables,
                incremental);
            rawTables.Run();

            CheckSourceFiles(dataMaps, rawTables, configuration);

            // -------------------------------------------------
            // 3. Views the website reads (03_, 04_) and their JSON exports (60_, 61_)
            // -------------------------------------------------
            new GenerateViews(
                dataMaps,
                tableMappingPath,
                sqlDir,
                jsonDir,
                generatedJsonDir,
                sqlFiles,
                logicalKeysToRebuild,
                generateAllSql
            ).Run();

            // -------------------------------------------------
            // 4. Indexes (10_)
            // -------------------------------------------------
            new GenerateIndexes(
                sqlDir,
                sqlFiles
            ).Run();

            // -------------------------------------------------
            // 5. Similar schools views (50_) and their JSON exports (70_)
            // -------------------------------------------------
            new GenerateSimilarSchoolsViews(
                dataMaps,
                tableMappingPath,
                sqlDir,
                generatedJsonDir,
                sqlFiles,
                logicalKeysToRebuild,
                generateAllSql
            ).Run();

            // -------------------------------------------------
            // 6. Similar schools indexes (60_)
            // -------------------------------------------------
            new GenerateSimilarSchoolsIndexes(
                sqlDir,
                sqlFiles
            ).Run();

            // -------------------------------------------------
            // 7. run_all.sql: every script, in order
            // -------------------------------------------------
            WriteRunAllSql(runAllSqlFile, sqlDir, sqlFiles, incremental);

            Console.WriteLine("Run Complete.");

            // Optional: avoid blocking in CI
            if (!Console.IsInputRedirected)
            {
                Console.ReadLine();
            }
        }
        catch (Exception ex)
        {
            if (sentry is not null)
            {
                SentrySdk.CaptureException(ex);
                SentrySdk.FlushAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
            }
            throw;
        }
    }

    private static bool IsIncremental(IConfiguration configuration)
    {
        var mode = configuration["RawTableRebuildMode"] ?? Environment.GetEnvironmentVariable("RAW_TABLE_REBUILD_MODE");
        var incremental = !string.Equals(mode?.Trim(), "list", StringComparison.OrdinalIgnoreCase);
        Console.WriteLine(incremental
            ? "Raw table rebuild mode: incremental (reload what changed)."
            : "Raw table rebuild mode: list (reload only the listed tables).");
        return incremental;
    }

    private static IDisposable? InitialiseSentry(IConfiguration configuration)
    {
        var enabledValue = configuration["Sentry:Enabled"] ?? Environment.GetEnvironmentVariable("Sentry__Enabled");
        var enabled = bool.TryParse(enabledValue, out var parsedEnabled) && parsedEnabled;
        var dsn = configuration["Sentry:Dsn"] ?? configuration["SENTRY_DSN"];

        if (!enabled || string.IsNullOrWhiteSpace(dsn))
        {
            return null;
        }

        var environment =
            Environment.GetEnvironmentVariable("DEPLOY_ENV")
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? "production";

        return SentrySdk.Init(options =>
        {
            options.Dsn = dsn;
            options.Environment = environment.Trim().ToLowerInvariant();
            options.SendDefaultPii = false;
            options.AttachStacktrace = true;
        });
    }

    private static string ResolveRawTablesToRebuildPath(string baseDir, IConfiguration configuration)
    {
        var configuredPath =
            configuration["RawTablesToRebuildPath"]
            ?? Environment.GetEnvironmentVariable("RAW_TABLES_TO_REBUILD_PATH");

        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            return Path.Combine(baseDir, "raw_tables_to_rebuild.txt");
        }

        var resolvedPath = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.GetFullPath(Path.Combine(baseDir, configuredPath));

        Console.WriteLine($"Using raw table rebuild list from: {resolvedPath}");
        return resolvedPath;
    }

    private static bool ShouldRebuildAllRawTables(IConfiguration configuration)
    {
        var configuredValue =
            configuration["RebuildAllRawTables"]
            ?? Environment.GetEnvironmentVariable("REBUILD_ALL_RAW_TABLES");

        var rebuildAll = bool.TryParse(configuredValue, out var parsed) && parsed;

        if (rebuildAll)
        {
            Console.WriteLine("Full raw-table rebuild enabled. The rebuild list will be ignored.");
        }

        return rebuildAll;
    }

    private static HashSet<string> LoadLogicalKeysToRebuild(string path)
    {
        if (!File.Exists(path))
        {
            Console.WriteLine($"No raw table rebuild list found at {path}. No raw tables will be dropped or recreated.");
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        var keys = File.ReadAllLines(path)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith('#'))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Console.WriteLine($"Loaded {keys.Count} raw table key(s) to rebuild.");
        return keys;
    }

}
