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
        string preservedTablesPath = Path.Combine(baseDir, "preserved_raw_tables.txt");
        string runAllSqlFile = Path.Combine(sqlDir, "run_all.sql");
        List<string> sqlFiles = new();

        string infrastructureDir = Path.Combine(Directory.GetParent(baseDir)!.FullName, "SAPSec.Infrastructure");
        string jsonDir = Path.Combine(infrastructureDir, "Data", "Files");
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

        var preservedLogicalKeys = LoadPreservedLogicalKeys(preservedTablesPath);
        WriteCleanupSql(
            Path.Combine(sqlDir, "00_cleanup.sql"),
            preservedLogicalKeys.Select(GenerateRawTables.GenerateShortTableName));

        // -------------------------------------------------
        // 2. Generate raw tables + cleaned files + mapping
        // -------------------------------------------------
        new GenerateRawTables(
            rawInputDir,
            cleanedDir,
            sqlDir,
            tableMappingPath,
            sqlFiles,
            preservedLogicalKeys
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

    private static HashSet<string> LoadPreservedLogicalKeys(string path)
    {
        if (!File.Exists(path))
        {
            Console.WriteLine($"No preserve list found at {path}. Continuing without preserved raw tables.");
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        var keys = File.ReadAllLines(path)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith('#'))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Console.WriteLine($"Loaded {keys.Count} preserved raw table key(s).");
        return keys;
    }

    private static void WriteCleanupSql(string path, IEnumerable<string> preservedTableNames)
    {
        var preserved = preservedTableNames
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var sql = new StringBuilder();
        sql.AppendLine("-- ================================================================");
        sql.AppendLine("-- 00_cleanup.sql");
        sql.AppendLine("-- Auto-generated by SAPData/Program.cs");
        sql.AppendLine("-- Drops generated objects so the pipeline can re-run idempotently.");
        sql.AppendLine("-- Preserved raw tables are excluded from deletion.");
        sql.AppendLine("-- ================================================================");
        sql.AppendLine();
        sql.AppendLine(@"\echo 'Cleaning up generated objects and regenerating reusable functions...'");
        sql.AppendLine();
        sql.AppendLine("DO $$");
        sql.AppendLine("DECLARE");
        sql.AppendLine("  v_schema text := current_schema();");
        sql.AppendLine("  r record;");
        if (preserved.Count > 0)
        {
            sql.AppendLine($"  preserved_tables text[] := ARRAY[{string.Join(", ", preserved.Select(name => $"'{name}'"))}];");
        }
        else
        {
            sql.AppendLine("  preserved_tables text[] := ARRAY[]::text[];");
        }
        sql.AppendLine("BEGIN");
        sql.AppendLine("  EXECUTE format('SET search_path TO %I', v_schema);");
        sql.AppendLine();
        sql.AppendLine("  -- 1) Drop generated materialized views (v_*)");
        sql.AppendLine("  FOR r IN");
        sql.AppendLine("    SELECT schemaname, matviewname");
        sql.AppendLine("    FROM pg_matviews");
        sql.AppendLine("    WHERE schemaname = v_schema");
        sql.AppendLine(@"      AND matviewname LIKE 'v\_%' ESCAPE '\' ");
        sql.AppendLine("  LOOP");
        sql.AppendLine("    EXECUTE format('DROP MATERIALIZED VIEW IF EXISTS %I.%I CASCADE', r.schemaname, r.matviewname);");
        sql.AppendLine("  END LOOP;");
        sql.AppendLine();
        sql.AppendLine("  -- 2) Drop generated tables (t_*, raw_*), excluding preserved raw tables");
        sql.AppendLine("  FOR r IN");
        sql.AppendLine("    SELECT schemaname, tablename");
        sql.AppendLine("    FROM pg_tables");
        sql.AppendLine("    WHERE schemaname = v_schema");
        sql.AppendLine(@"      AND (tablename LIKE 't\_%' ESCAPE '\' OR tablename LIKE 'raw\_%' ESCAPE '\')");
        sql.AppendLine("      AND NOT (tablename = ANY(preserved_tables))");
        sql.AppendLine("  LOOP");
        sql.AppendLine("    EXECUTE format('DROP TABLE IF EXISTS %I.%I CASCADE', r.schemaname, r.tablename);");
        sql.AppendLine("  END LOOP;");
        sql.AppendLine();
        sql.AppendLine("  EXECUTE 'DROP FUNCTION IF EXISTS clean_int(TEXT)';");
        sql.AppendLine("  EXECUTE 'DROP FUNCTION IF EXISTS clean_numeric(TEXT)';");
        sql.AppendLine("  EXECUTE 'DROP FUNCTION IF EXISTS clean_date(TEXT)';");
        sql.AppendLine("END $$;");
        sql.AppendLine();
        sql.AppendLine("-- =========================");
        sql.AppendLine("-- Cleaning helpers");
        sql.AppendLine("-- =========================");
        sql.AppendLine();
        sql.AppendLine("CREATE OR REPLACE FUNCTION clean_int(value TEXT)");
        sql.AppendLine("RETURNS INT");
        sql.AppendLine("LANGUAGE plpgsql");
        sql.AppendLine("IMMUTABLE");
        sql.AppendLine("AS $$");
        sql.AppendLine("BEGIN");
        sql.AppendLine("    IF value IS NULL OR trim(value) IN ('', 'NE', 'N', 'na', 'n/a', 'N/A', 'SUPP', '.', '-', '--', 'z') THEN");
        sql.AppendLine("        RETURN NULL;");
        sql.AppendLine("    END IF;");
        sql.AppendLine();
        sql.AppendLine("    RETURN value::INT;");
        sql.AppendLine();
        sql.AppendLine("EXCEPTION WHEN others THEN");
        sql.AppendLine("    RETURN NULL;");
        sql.AppendLine("END;");
        sql.AppendLine("$$;");
        sql.AppendLine();
        sql.AppendLine("CREATE OR REPLACE FUNCTION clean_numeric(value TEXT)");
        sql.AppendLine("RETURNS NUMERIC");
        sql.AppendLine("LANGUAGE plpgsql");
        sql.AppendLine("IMMUTABLE");
        sql.AppendLine("AS $$");
        sql.AppendLine("BEGIN");
        sql.AppendLine("    IF value IS NULL OR trim(value) IN ('', 'NE', 'N', 'na', 'n/a', 'N/A', 'SUPP', '.', '-', '--', 'z') THEN");
        sql.AppendLine("        RETURN NULL;");
        sql.AppendLine("    END IF;");
        sql.AppendLine();
        sql.AppendLine("    RETURN value::NUMERIC;");
        sql.AppendLine();
        sql.AppendLine("EXCEPTION WHEN others THEN");
        sql.AppendLine("    RETURN NULL;");
        sql.AppendLine("END;");
        sql.AppendLine("$$;");
        sql.AppendLine();
        sql.AppendLine("CREATE OR REPLACE FUNCTION clean_date(value TEXT)");
        sql.AppendLine("RETURNS DATE");
        sql.AppendLine("LANGUAGE plpgsql");
        sql.AppendLine("IMMUTABLE");
        sql.AppendLine("AS $$");
        sql.AppendLine("BEGIN");
        sql.AppendLine("    IF value IS NULL OR trim(value) IN ('', 'na', 'n/a', 'N/A', '.', '-', '--') THEN");
        sql.AppendLine("        RETURN NULL;");
        sql.AppendLine("    END IF;");
        sql.AppendLine();
        sql.AppendLine("    RETURN value::DATE;");
        sql.AppendLine();
        sql.AppendLine("EXCEPTION WHEN others THEN");
        sql.AppendLine("    RETURN NULL;");
        sql.AppendLine("END;");
        sql.AppendLine("$$;");
        sql.AppendLine();
        sql.AppendLine(@"\echo 'Cleanup complete.'");

        File.WriteAllText(path, sql.ToString(), new UTF8Encoding(false));
    }
}
