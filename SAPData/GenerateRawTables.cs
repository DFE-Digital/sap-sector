using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SAPData;

public class GenerateRawTables
{
    private readonly string _inputDir;
    private readonly string _cleanDir;
    private readonly string _sqlDir;

    private readonly Dictionary<string, string> _tableMappings = new();

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

        WriteTableMappings();

        Console.WriteLine("RAW table creation scripts generated.");
    }

    private void ProcessCsv(
        string csvPath,
        StringBuilder createSql,
        StringBuilder copySql,
        StringBuilder copyLocalSql)
    {
        string datasetKey = Path.GetFileNameWithoutExtension(csvPath);
        string tableName = GenerateShortTableName(datasetKey);

        _tableMappings[datasetKey] = tableName;

        string cleanCsvPath = Path.Combine(_cleanDir, datasetKey + ".clean.csv");

        Console.WriteLine($"Processing: {datasetKey}");

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
                // Pad missing trailing columns
                while (row.Count < columnCount)
                    row.Add("");
            }
            else if (row.Count > columnCount)
            {
                // Truncate overflow safely
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
    // TABLE MAPPING
    // =====================================================
    private void WriteTableMappings()
    {
        string mappingPath = Path.Combine(_sqlDir, "tablemapping.csv");

        using var writer = new StreamWriter(mappingPath, false, Encoding.UTF8);
        foreach (var kvp in _tableMappings.OrderBy(k => k.Key))
        {
            writer.WriteLine($"{kvp.Key},{kvp.Value}");
        }

        Console.WriteLine($"Generated: {mappingPath}");
    }

    // =====================================================
    // HELPERS
    // =====================================================
    private static string GenerateShortTableName(string datasetKey)
    {
        using var sha1 = SHA1.Create();
        var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(datasetKey));
        string shortHash = BitConverter.ToString(hash).Replace("-", "").Substring(0, 8).ToLowerInvariant();

        string prefix = Sanitise(datasetKey);
        if (prefix.Length > 18)
            prefix = prefix.Substring(0, 18);

        return $"raw_{prefix}_{shortHash}";
    }


    private static string Sanitise(string input)
    {
        return input
            .Trim()
            .Replace("-", "_");
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
