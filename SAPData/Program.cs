using Microsoft.Extensions.Configuration;
using Sentry;
using SAPData.Models;
using SAPSec.Data.Common;
using SAPSec.Data.Common.Catalogue;
using SAPSec.Data.Common.Catalogue.Definitions;
using SAPSec.Data.Common.Catalogue.Validation;
using System.Text;
using System.Text.RegularExpressions;

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

            if (args.Contains("profile-sources"))
            {
                WriteSourceProfiles(rawInputDir, sourceProfilesPath);
                return;
            }

            if (args.Contains("catalogue-summary"))
            {
                WriteCatalogueSummary(rawInputDir);
                return;
            }

            Directory.CreateDirectory(cleanedDir);
            Directory.CreateDirectory(sqlDir);
            Directory.CreateDirectory(jsonDir);
            Directory.CreateDirectory(generatedJsonDir);
            Directory.CreateDirectory(primaryJsonDir);

            // -------------------------------------------------
            // 1. Load the data map (defined in code: SAPSec.Data.Common/Catalogue/Definitions)
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
            // 2. Generate raw tables + cleaned files + mapping
            // -------------------------------------------------
            new GenerateRawTables(
                rawInputDir,
                cleanedDir,
                sqlDir,
                tableMappingPath,
                sqlFiles,
                logicalKeysToRebuild,
                rebuildAllRawTables,
                incremental
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
                sqlFiles,
                logicalKeysToRebuild,
                generateAllSql
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
                sqlFiles,
                logicalKeysToRebuild,
                generateAllSql
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
                var view = ViewFile().Match(line);
                if (incremental && view.Success)
                {
                    // A view is rebuilt when it's missing (reloading a raw table drops its views) or its SQL changed.
                    var fingerprint = IncrementalLoad.Fingerprint(File.ReadAllText(Path.Combine(sqlDir, line)) + HelperFunctionsSql());
                    runAllSql.Append(IncrementalLoad.View(view.Groups["view"].Value, line, fingerprint));
                }
                else
                {
                    runAllSql.AppendLine(@$"\ir {line}");
                }
            }

            File.WriteAllText(runAllSqlFile, runAllSql.ToString());

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

    // Helper functions the generated views call. Part of each view's fingerprint, so changing one rebuilds the views.
    private static string HelperFunctionsSql()
    {
        var helpers = new StringBuilder();
        helpers.AppendLine("-- =========================");
        helpers.AppendLine("-- Cleaning helpers");
        helpers.AppendLine("-- =========================");
        helpers.AppendLine();
        helpers.AppendLine("CREATE OR REPLACE FUNCTION clean_int(value TEXT)");
        helpers.AppendLine("RETURNS INT");
        helpers.AppendLine("LANGUAGE plpgsql");
        helpers.AppendLine("IMMUTABLE");
        helpers.AppendLine("AS $$");
        helpers.AppendLine("BEGIN");
        helpers.AppendLine("    IF value IS NULL OR trim(value) IN ('', 'NE', 'N', 'na', 'n/a', 'N/A', 'SUPP', '.', '-', '--', 'z') THEN");
        helpers.AppendLine("        RETURN NULL;");
        helpers.AppendLine("    END IF;");
        helpers.AppendLine();
        helpers.AppendLine("    RETURN value::INT;");
        helpers.AppendLine();
        helpers.AppendLine("EXCEPTION WHEN others THEN");
        helpers.AppendLine("    RETURN NULL;");
        helpers.AppendLine("END;");
        helpers.AppendLine("$$;");
        helpers.AppendLine();
        helpers.AppendLine("CREATE OR REPLACE FUNCTION clean_numeric(value TEXT)");
        helpers.AppendLine("RETURNS NUMERIC");
        helpers.AppendLine("LANGUAGE plpgsql");
        helpers.AppendLine("IMMUTABLE");
        helpers.AppendLine("AS $$");
        helpers.AppendLine("DECLARE");
        helpers.AppendLine("    result NUMERIC;");
        helpers.AppendLine("BEGIN");
        helpers.AppendLine("    IF value IS NULL OR trim(value) IN ('', 'NE', 'N', 'na', 'n/a', 'N/A', 'SUPP', '.', '-', '--', 'z') THEN");
        helpers.AppendLine("        RETURN NULL;");
        helpers.AppendLine("    END IF;");
        helpers.AppendLine();
        helpers.AppendLine("    -- Same rules as the website's parser (SAPSec.Core MeasureHelper.ParseNullableDecimal):");
        helpers.AppendLine("    -- a trailing '%' is ignored, and anything that isn't a finite number is NULL.");
        helpers.AppendLine("    result := regexp_replace(trim(value), '%$', '')::NUMERIC;");
        helpers.AppendLine();
        helpers.AppendLine("    IF result IN ('NaN'::NUMERIC, 'Infinity'::NUMERIC, '-Infinity'::NUMERIC) THEN");
        helpers.AppendLine("        RETURN NULL;");
        helpers.AppendLine("    END IF;");
        helpers.AppendLine();
        helpers.AppendLine("    RETURN result;");
        helpers.AppendLine();
        helpers.AppendLine("EXCEPTION WHEN others THEN");
        helpers.AppendLine("    RETURN NULL;");
        helpers.AppendLine("END;");
        helpers.AppendLine("$$;");
        helpers.AppendLine();
        helpers.AppendLine("CREATE OR REPLACE FUNCTION clean_date(value TEXT)");
        helpers.AppendLine("RETURNS DATE");
        helpers.AppendLine("LANGUAGE plpgsql");
        helpers.AppendLine("IMMUTABLE");
        helpers.AppendLine("AS $$");
        helpers.AppendLine("BEGIN");
        helpers.AppendLine("    IF value IS NULL OR trim(value) IN ('', 'na', 'n/a', 'N/A', '.', '-', '--') THEN");
        helpers.AppendLine("        RETURN NULL;");
        helpers.AppendLine("    END IF;");
        helpers.AppendLine();
        helpers.AppendLine("    RETURN value::DATE;");
        helpers.AppendLine();
        helpers.AppendLine("EXCEPTION WHEN others THEN");
        helpers.AppendLine("    RETURN NULL;");
        helpers.AppendLine("END;");
        helpers.AppendLine("$$;");
        helpers.AppendLine();
        return helpers.ToString();
    }

    [GeneratedRegex(@"^(03|04|50)_(?<view>v_\w+)\.sql$")]
    private static partial Regex ViewFile();

    private static bool IsIncremental(IConfiguration configuration)
    {
        var mode = configuration["RawTableRebuildMode"] ?? Environment.GetEnvironmentVariable("RAW_TABLE_REBUILD_MODE");
        var incremental = !string.Equals(mode?.Trim(), "list", StringComparison.OrdinalIgnoreCase);
        Console.WriteLine(incremental
            ? "Raw table rebuild mode: incremental (reload what changed)."
            : "Raw table rebuild mode: list (reload only the listed tables).");
        return incremental;
    }

    // Fails before any SQL is generated, so a data map mistake never reaches the ETL step.
    private static void ValidateCatalogue(IReadOnlyList<DataMapRow> rows)
    {
        var issues = CatalogueValidator.Validate(rows);
        if (issues.Count == 0)
        {
            Console.WriteLine($"Catalogue validated: {rows.Count} rows, no issues.");
            return;
        }

        foreach (var issue in issues)
            Console.Error.WriteLine(issue);

        throw new InvalidOperationException($"Data map catalogue has {issues.Count} validation issue(s); see the log above.");
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

    private static void WriteCleanupSql(string path, IEnumerable<string> tableNamesToRebuild, bool rebuildAllRawTables, bool incremental)
    {
        var tablesToRebuild = tableNamesToRebuild
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var sql = new StringBuilder();
        sql.AppendLine("-- ================================================================");
        sql.AppendLine("-- 00_cleanup.sql");
        sql.AppendLine("-- Auto-generated by SAPData/Program.cs");
        sql.AppendLine("-- Drops only explicitly listed raw tables before regeneration.");
        sql.AppendLine("-- Recreates helper functions used by generated views.");
        sql.AppendLine("-- ================================================================");
        sql.AppendLine();
        sql.AppendLine(@"\echo 'Cleaning up listed raw tables and regenerating helper functions...'");
        sql.AppendLine();
        sql.AppendLine("DO $$");
        sql.AppendLine("DECLARE");
        sql.AppendLine("  v_schema text := current_schema();");
        sql.AppendLine("  r record;");
        if (tablesToRebuild.Count > 0)
        {
            sql.AppendLine($"  tables_to_rebuild text[] := ARRAY[{string.Join(", ", tablesToRebuild.Select(name => $"'{name}'"))}];");
        }
        else
        {
            sql.AppendLine("  tables_to_rebuild text[] := ARRAY[]::text[];");
        }
        sql.AppendLine("BEGIN");
        sql.AppendLine("  EXECUTE format('SET search_path TO %I', v_schema);");
        sql.AppendLine();
        sql.AppendLine("  -- Drop only listed raw tables");
        sql.AppendLine("  FOR r IN");
        sql.AppendLine("    SELECT schemaname, tablename");
        sql.AppendLine("    FROM pg_tables");
        sql.AppendLine("    WHERE schemaname = v_schema");
        if (rebuildAllRawTables)
        {
            sql.AppendLine("      AND tablename LIKE 't\\_%' ESCAPE '\\'");
        }
        else
        {
            sql.AppendLine("      AND tablename = ANY(tables_to_rebuild)");
        }
        sql.AppendLine("  LOOP");
        sql.AppendLine("    EXECUTE format('DROP TABLE IF EXISTS %I.%I CASCADE', r.schemaname, r.tablename);");
        sql.AppendLine("  END LOOP;");
        sql.AppendLine("END $$;");
        sql.AppendLine();
        if (incremental)
        {
            sql.AppendLine("-- Fingerprints of what each raw table and view was last built from (incremental loads).");
            sql.Append(IncrementalLoad.LogTablesSql(rebuildAllRawTables));
            sql.AppendLine();
        }

        sql.Append(HelperFunctionsSql());
        sql.AppendLine(@"\echo 'Cleanup complete.'");

        File.WriteAllText(path, sql.ToString(), new UTF8Encoding(false));
    }
}
