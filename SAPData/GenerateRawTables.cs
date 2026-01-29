using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SAPData;

public class GenerateRawTables
{
    private readonly string _inputDir;
    private readonly string _cleanDir;
    private readonly string _sqlDir;

    private readonly Dictionary<string, string> _tableMappings = new(StringComparer.OrdinalIgnoreCase);

    public GenerateRawTables(string inputDir, string cleanDir, string sqlDir)
    {
        _inputDir = inputDir;
        _cleanDir = cleanDir;
        _sqlDir = sqlDir;
    }

    public void Run()
    {
        Directory.CreateDirectory(_cleanDir);
        Directory.CreateDirectory(_sqlDir);

        var createSql = new StringBuilder();
        var copySql = new StringBuilder();
        var copyLocalSql = new StringBuilder();

        createSql.AppendLine("-- AUTO-GENERATED RAW TABLE DEFINITIONS");
        copySql.AppendLine("-- AUTO-GENERATED COPY INTO RAW (PIPELINE)");
        copyLocalSql.AppendLine("-- AUTO-GENERATED LOCAL COPY INTO RAW");

        foreach (var csvPath in Directory.GetFiles(_inputDir, "*.csv"))
        {
            ProcessCsv(csvPath, createSql, copySql, copyLocalSql);
        }

        var utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        File.WriteAllText(Path.Combine(_sqlDir, "01_create_raw_tables.sql"), createSql.ToString(), utf8NoBom);
        File.WriteAllText(Path.Combine(_sqlDir, "02_copy_into_raw.sql"), copySql.ToString(), utf8NoBom);
        File.WriteAllText(Path.Combine(_sqlDir, "02_copy_into_raw_local.sql"), copyLocalSql.ToString(), utf8NoBom);

        // Add alias rows BEFORE writing tablemapping.csv
        AddLegacyAliasesFromRawSources();

        WriteTableMappings();

        Console.WriteLine("RAW table creation scripts generated.");
    }

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
        bool isManual = fileKey.StartsWith("manual_", StringComparison.OrdinalIgnoreCase);
        string logicalKey = isManual ? fileKey["manual_".Length..] : fileKey;

        // Physical table name: prefix-free, based on logical identity (stable)
        string tableName = GenerateShortTableName(logicalKey);

        // Map BOTH keys to the same physical table
        // - DataMap will use logicalKey
        // - Any direct lookups / legacy scripts may still use fileKey
        if (_tableMappings.ContainsKey(logicalKey))
            Console.WriteLine($"WARNING: Duplicate logical dataset key detected: {logicalKey} (existing table '{_tableMappings[logicalKey]}', new '{tableName}')");

        _tableMappings[logicalKey] = tableName;
        _tableMappings[fileKey] = tableName;

        string cleanCsvPath = Path.Combine(_cleanDir, fileKey + ".clean.csv");

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
        // CREATE TABLE
        // -----------------------------
        createSql.AppendLine($"DROP TABLE IF EXISTS {tableName};");
        createSql.AppendLine($"CREATE TABLE {tableName} (");

        for (int i = 0; i < headers.Count; i++)
        {
            string col = Sanitise(headers[i]);
            string comma = i == headers.Count - 1 ? "" : ",";
            createSql.AppendLine($"    \"{col}\" TEXT{comma}");
        }

        createSql.AppendLine(");");
        createSql.AppendLine();

        // -----------------------------
        // COPY (pipeline)
        // -----------------------------
        copySql.AppendLine(
            $"COPY {tableName} FROM '{cleanCsvPath.Replace("\\", "/")}' " +
            "WITH (FORMAT csv, HEADER true, NULL '', DELIMITER ',');");
        copySql.AppendLine();

        // -----------------------------
        // COPY (local)
        // -----------------------------
        copyLocalSql.AppendLine(
            $"\\copy {tableName} FROM '{cleanCsvPath.Replace("\\", "/")}' CSV HEADER;");
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
        string mappingPath = Path.Combine(_sqlDir, "tablemapping.csv");

        using var writer = new StreamWriter(mappingPath, false, Encoding.UTF8);
        foreach (var kvp in _tableMappings.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
        {
            writer.WriteLine($"{kvp.Key},{kvp.Value}");
        }

        Console.WriteLine($"Generated: {mappingPath}");
    }

    // =====================================================
    // HELPERS
    // =====================================================
    private static string GenerateShortTableName(string logicalKey)
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
}
