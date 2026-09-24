using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SAPData;

public class GenerateRawTables
{
    private readonly string _inputDir;
    private readonly string _cleanDir;
    private readonly string _sqlDir;
    private readonly string _tableMappingPath;
    private readonly List<string> _sqlFiles;
    private readonly HashSet<string> _logicalKeysToRebuild;
    private readonly bool _rebuildAllRawTables;
    private readonly bool _incremental;

    private readonly Dictionary<string, string> _tableMappings = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _loadedTables = [];
    private readonly Dictionary<string, string> _sourceFilesByTable = new(StringComparer.OrdinalIgnoreCase);

    public GenerateRawTables(
        string inputDir,
        string cleanDir,
        string sqlDir,
        string tableMappingPath,
        List<string> sqlFiles,
        IEnumerable<string>? logicalKeysToRebuild = null,
        bool rebuildAllRawTables = false,
        bool incremental = false)
    {
        _inputDir = inputDir;
        _cleanDir = cleanDir;
        _sqlDir = sqlDir;
        _tableMappingPath = tableMappingPath;
        _sqlFiles = sqlFiles;
        _logicalKeysToRebuild = new HashSet<string>(
            logicalKeysToRebuild ?? Array.Empty<string>(),
            StringComparer.OrdinalIgnoreCase);
        _rebuildAllRawTables = rebuildAllRawTables;
        _incremental = incremental;
    }

    /// <summary>Dataset key (file name, alias or version) to raw table, as written to tablemapping.csv.</summary>
    public IReadOnlyDictionary<string, string> TableMappings => _tableMappings;

    /// <summary>Raw table to the cleaned file it is loaded from (available after <see cref="Run"/>).</summary>
    public IReadOnlyDictionary<string, string> SourceFilesByTable => _sourceFilesByTable;

    public void Run()
    {
        var createSql = new StringBuilder();
        var copySql = new StringBuilder();
        var copyLocalSql = new StringBuilder();

        createSql.AppendLine("-- AUTO-GENERATED RAW TABLE DEFINITIONS");
        copySql.AppendLine("-- AUTO-GENERATED COPY INTO RAW (PIPELINE)");
        copyLocalSql.AppendLine("-- AUTO-GENERATED LOCAL COPY INTO RAW");

        foreach (var csvPath in SourceFiles())
        {
            ProcessCsv(csvPath, createSql, copySql, copyLocalSql);
        }

        if (_incremental)
        {
            copySql.Append(IncrementalLoad.DropSupersededTablesSql(_loadedTables));
            copyLocalSql.Append(IncrementalLoad.DropSupersededTablesSql(_loadedTables));
        }

        var utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        WriteSql("01", "create_raw_tables", createSql.ToString());
        WriteSql("02", "copy_into_raw", copySql.ToString(), false);
        WriteSql("02", "copy_into_raw_local", copyLocalSql.ToString());

        // Add alias rows BEFORE writing tablemapping.csv
        AddLegacyAliasesFromRawSources();

        WriteTableMappings();

        Console.WriteLine("RAW table creation scripts generated.");
    }

    // Incremental loads read one file per dataset (the manual_ copy if there is one, as its table mapping does);
    // loading both into one table would duplicate rows and reload it on every run.
    private IEnumerable<string> SourceFiles()
    {
        var files = Directory.GetFiles(_inputDir, "*.csv");
        if (!_incremental)
            return files;

        return files
            .GroupBy(f => LogicalKey(Path.GetFileNameWithoutExtension(f)), StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                var chosen = g.OrderBy(f => IsManual(Path.GetFileNameWithoutExtension(f))).Last();
                foreach (var other in g.Where(f => f != chosen))
                {
                    Console.WriteLine($"Using {Path.GetFileName(chosen)} for '{g.Key}'; ignoring {Path.GetFileName(other)}.");
                    _tableMappings[Path.GetFileNameWithoutExtension(other)] = GenerateShortTableName(g.Key);
                }
                return chosen;
            })
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsManual(string fileKey) => fileKey.StartsWith("manual_", StringComparison.OrdinalIgnoreCase);

    private static string LogicalKey(string fileKey) => IsManual(fileKey) ? fileKey["manual_".Length..] : fileKey;

    private void ProcessCsv(
        string csvPath,
        StringBuilder createSql,
        StringBuilder copySql,
        StringBuilder copyLocalSql)
    {
        // fileKey is the actual CSV name (without extension) in your input dir.
        // This MAY include manual_ prefix (because the blob is stored that way).
        string fileKey = Path.GetFileNameWithoutExtension(csvPath);

        // logicalKey is the dataset identity used by DataMap / GenerateViews (no manual_ prefix)
        string logicalKey = LogicalKey(fileKey);

        // Physical table name: prefix-free, based on logical identity (stable)
        string tableName = GenerateShortTableName(logicalKey);
        bool rebuildTable = _rebuildAllRawTables || _logicalKeysToRebuild.Contains(logicalKey);

        // Map BOTH keys to the same physical table
        // - DataMap will use logicalKey
        // - Any direct lookups / legacy scripts may still use fileKey
        if (_tableMappings.ContainsKey(logicalKey))
            Console.WriteLine($"WARNING: Duplicate logical dataset key detected: {logicalKey} (existing table '{_tableMappings[logicalKey]}', new '{tableName}')");

        _tableMappings[logicalKey] = tableName;
        _tableMappings[fileKey] = tableName;

        string cleanCsvPath = Path.Combine(_cleanDir, fileKey + ".clean.csv");
        _sourceFilesByTable[tableName] = cleanCsvPath;

        Console.WriteLine($"Processing: {fileKey}");

        using var reader = new StreamReader(csvPath, Encoding.UTF8, true);
        using var writer = new StreamWriter(cleanCsvPath, false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        // -----------------------------
        // Header
        // -----------------------------
        string? headerLine = reader.ReadLine();
        if (headerLine == null)
            throw new InvalidOperationException($"Missing header: {csvPath}");

        var headers = ParseCsvLine(headerLine);
        int columnCount = headers.Count;

        writer.WriteLine(string.Join(",", headers.Select(EscapeCsv)));

        // -----------------------------
        // Rows (defensive normalisation)
        // -----------------------------
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var row = ParseCsvLine(line);

            if (row.Count < columnCount)
            {
                while (row.Count < columnCount)
                    row.Add("");
            }
            else if (row.Count > columnCount)
            {
                row = row.Take(columnCount).ToList();
            }

            writer.WriteLine(string.Join(",", row.Select(EscapeCsv)));
        }

        // -----------------------------
        // CREATE TABLE + COPY
        // -----------------------------
        var createTable = new StringBuilder();
        createTable.AppendLine($"CREATE TABLE {tableName} (");
        for (int i = 0; i < headers.Count; i++)
        {
            string col = Sanitise(headers[i]);
            string comma = i == headers.Count - 1 ? "" : ",";
            createTable.AppendLine($"    \"{col}\" TEXT{comma}");
        }
        createTable.Append(");");

        string copy = $"COPY {tableName} FROM '{cleanCsvPath.Replace("\\", "/")}' " +
            "WITH (FORMAT csv, HEADER true, NULL '', DELIMITER ',');";
        string copyLocal = $"\\copy {tableName} FROM '{cleanCsvPath.Replace("\\", "/")}' CSV HEADER;";

        if (_incremental)
        {
            var fingerprint = IncrementalLoad.Fingerprint(csvPath, createTable.ToString());
            var forced = _logicalKeysToRebuild.Contains(logicalKey);
            _loadedTables.Add(tableName);
            createSql.Append(IncrementalLoad.RawTableCreate(tableName, fileKey, fingerprint, createTable.ToString(), forced));
            copySql.Append(IncrementalLoad.RawTableCopy(tableName, fileKey, fingerprint, copy));
            copyLocalSql.Append(IncrementalLoad.RawTableCopy(tableName, fileKey, fingerprint, copyLocal));
            return;
        }

        if (!rebuildTable)
        {
            Console.WriteLine($"Leaving table for logical key '{logicalKey}' ({tableName}) unchanged. Skipping DROP/CREATE/COPY.");
            return;
        }

        createSql.AppendLine($"DROP TABLE IF EXISTS {tableName};");
        createSql.AppendLine(createTable.ToString());
        createSql.AppendLine();

        // -----------------------------
        // COPY (pipeline)
        // -----------------------------
        copySql.AppendLine(copy);
        copySql.AppendLine();

        // -----------------------------
        // COPY (local)
        // -----------------------------
        copyLocalSql.AppendLine(copyLocal);
        copyLocalSql.AppendLine();
    }

    // =====================================================
    // ALIASING: raw_sources.json FileName -> raw table
    // =====================================================
    private void AddLegacyAliasesFromRawSources()
    {
        var rawSourcesPath = Path.Combine("SAPData", "raw_sources.json");
        var manifestPath = Path.Combine(_inputDir, "versions.json");

        if (!File.Exists(rawSourcesPath))
        {
            Console.WriteLine($"Alias mapping skipped: raw_sources.json not found at {rawSourcesPath}");
            return;
        }

        if (!File.Exists(manifestPath))
        {
            Console.WriteLine($"Alias mapping skipped: versions.json not found in input dir ({manifestPath})");
            return;
        }

        Dictionary<string, ManifestEntry> manifest;
        try
        {
            manifest = LoadManifest(manifestPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Alias mapping skipped: failed to parse versions.json. {ex.Message}");
            return;
        }

        RawSourceEntry[] sources;
        try
        {
            sources = JsonSerializer.Deserialize<RawSourceEntry[]>(File.ReadAllText(rawSourcesPath))
                      ?? Array.Empty<RawSourceEntry>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Alias mapping skipped: failed to parse raw_sources.json. {ex.Message}");
            return;
        }

        foreach (var s in sources)
        {
            if (string.IsNullOrWhiteSpace(s.Type) ||
                string.IsNullOrWhiteSpace(s.Subtype) ||
                string.IsNullOrWhiteSpace(s.Year) ||
                string.IsNullOrWhiteSpace(s.SourceOrg) ||
                string.IsNullOrWhiteSpace(s.FileName))
            {
                continue;
            }

            var baseKey = MakeKey($"{s.Type}_{s.Subtype}_{s.Year}");

            // Resolve which datasetKey exists in _tableMappings
            // - GIAS: baseKey
            // - EES: baseKey_v{eesLatestVersion} (from manifest)
            string? actualDatasetKey = null;

            if (s.SourceOrg.Equals("GIAS", StringComparison.OrdinalIgnoreCase))
            {
                actualDatasetKey = baseKey;
            }
            else if (s.SourceOrg.Equals("EES", StringComparison.OrdinalIgnoreCase))
            {
                if (manifest.TryGetValue(baseKey, out var me) && !string.IsNullOrWhiteSpace(me.EesLatestVersion))
                {
                    actualDatasetKey = $"{baseKey}_v{me.EesLatestVersion}";
                }
                else
                {
                    // Fallback: pick any mapping that matches baseKey_v*
                    actualDatasetKey = _tableMappings.Keys
                        .FirstOrDefault(k => k.StartsWith(baseKey + "_v", StringComparison.OrdinalIgnoreCase));
                }
            }

            if (string.IsNullOrWhiteSpace(actualDatasetKey))
                continue;

            if (!_tableMappings.TryGetValue(actualDatasetKey, out var rawTable))
                continue;

            // Convert FileName pattern to exact legacy key for mapping
            var legacyKey = s.FileName.Trim();

            if (legacyKey.Contains("YYYYmmDD", StringComparison.OrdinalIgnoreCase))
            {
                if (!manifest.TryGetValue(baseKey, out var me) || string.IsNullOrWhiteSpace(me.LastSuccessDate))
                    continue;

                legacyKey = legacyKey.Replace("YYYYmmDD", me.LastSuccessDate, StringComparison.OrdinalIgnoreCase);
            }

            // Alias row: legacyKey -> rawTable
            _tableMappings[legacyKey] = rawTable;
        }
    }

    private static Dictionary<string, ManifestEntry> LoadManifest(string path)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var root = doc.RootElement;

        var result = new Dictionary<string, ManifestEntry>(StringComparer.OrdinalIgnoreCase);

        foreach (var prop in root.EnumerateObject())
        {
            var key = prop.Name;
            var obj = prop.Value;

            string? lastSuccessDate = null;
            string? eesLatestVersion = null;

            if (obj.ValueKind == JsonValueKind.Object)
            {
                if (obj.TryGetProperty("lastSuccessDate", out var d) && d.ValueKind == JsonValueKind.String)
                    lastSuccessDate = d.GetString();

                if (obj.TryGetProperty("eesLatestVersion", out var v) && v.ValueKind == JsonValueKind.String)
                    eesLatestVersion = v.GetString();
            }

            result[key] = new ManifestEntry(lastSuccessDate, eesLatestVersion);
        }

        return result;
    }

    private sealed record ManifestEntry(string? LastSuccessDate, string? EesLatestVersion);

    private sealed record RawSourceEntry(
        string? Type,
        string? Subtype,
        string? Year,
        string? SourceOrg,
        string? FileName
    );

    private static string MakeKey(string composite)
    {
        return new string(composite
            .Select(c => char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '.' ? c : '_')
            .ToArray())
            .ToLowerInvariant();
    }

    // =====================================================
    // TABLE MAPPING
    // =====================================================
    private void WriteTableMappings()
    {
        Console.WriteLine($"Generating: {_tableMappingPath}");

        using var writer = new StreamWriter(_tableMappingPath, false, Encoding.UTF8);
        foreach (var kvp in _tableMappings.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
        {
            writer.WriteLine($"{kvp.Key},{kvp.Value}");
            Console.WriteLine($"{kvp.Key},{kvp.Value}");
        }

        Console.WriteLine($"Generated: {_tableMappingPath}");
    }

    // =====================================================
    // HELPERS
    // =====================================================
    public static string GenerateShortTableName(string logicalKey)
    {
        using var sha1 = SHA1.Create();

        var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(logicalKey));
        string shortHash = BitConverter
            .ToString(hash)
            .Replace("-", "")
            .Substring(0, 10)
            .ToLowerInvariant();

        string baseName = Sanitise(logicalKey);

        if (baseName.Length > 20)
            baseName = baseName.Substring(0, 20);

        // Always prefix raw tables with t_
        // (so cleanup can safely target t_% and names never start with a digit)
        return $"t_{baseName}_{shortHash}";
    }

    private static string Sanitise(string input)
    {
        var sb = new StringBuilder();

        foreach (var c in input.Trim())
        {
            if (char.IsLetterOrDigit(c) || c == '_')
                sb.Append(c);
            else
                sb.Append('_');
        }

        return sb.ToString().ToLowerInvariant();
    }


    // =====================================================
    // CSV PARSER (RFC4180-safe)
    // =====================================================
    private static List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    sb.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(sb.ToString());
                sb.Clear();
            }
            else
            {
                sb.Append(c);
            }
        }

        result.Add(sb.ToString());
        return result;
    }

    private static string EscapeCsv(string value)
    {
        if (value == null)
            return "";

        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            return "\"" + value.Replace("\"", "\"\"") + "\"";

        return value;
    }

    private void WriteSql(string prefix, string viewName, string sql, bool addToRunAll = true)
    {
        var fileName = $"{prefix}_{viewName}.sql";

        File.WriteAllText(
            Path.Combine(_sqlDir, fileName),
            sql,
            new UTF8Encoding(false));

        if (addToRunAll)
        {
            _sqlFiles.Add($"{prefix}_{viewName}.sql");
        }

        Console.WriteLine($"Generated view index script: {fileName}");
    }
}
