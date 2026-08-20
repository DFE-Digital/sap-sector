using SAPData.Models;
using System.Text;

namespace SAPData;

public sealed class GenerateSimilarSchoolsViews
{
    private readonly IReadOnlyList<DataMapRow> _rows;
    private readonly string _tableMappingPath;
    private readonly string _sqlDir;
    private readonly string _generatedJsonDir;
    private readonly List<string> _sqlFiles;
    private readonly HashSet<string> _rawTableNamesToRebuild;
    private readonly bool _rebuildAllRawTables;

    private sealed record ViewSpec(
        string ViewName,
        string Range,
        string Type,
        string ModelName
    );

    private static readonly ViewSpec[] Views =
    {
        new("v_similar_schools_secondary_groups", "SimilarSchools", "SecondaryGroups", "SimilarSchoolsSecondaryGroupsEntry"),
        new("v_similar_schools_secondary_values", "SimilarSchools", "SecondaryValues", "SimilarSchoolsSecondaryValuesEntry"),
        new("v_similar_schools_primary_groups", "SimilarSchools", "PrimaryGroups", "SimilarSchoolsPrimaryGroupsEntry"),
        new("v_similar_schools_primary_values", "SimilarSchools", "PrimaryValues", "SimilarSchoolsPrimaryValuesEntry"),
    };

    public GenerateSimilarSchoolsViews(
        IReadOnlyList<DataMapRow> rows,
        string tableMappingPath,
        string sqlDir,
        string generatedJsonDir,
        List<string> sqlFiles,
        IEnumerable<string>? logicalKeysToRebuild = null,
        bool rebuildAllRawTables = false)
    {
        _rows = rows;
        _tableMappingPath = tableMappingPath;
        _sqlDir = sqlDir;
        _generatedJsonDir = generatedJsonDir;
        _sqlFiles = sqlFiles;
        _rebuildAllRawTables = rebuildAllRawTables;
        _rawTableNamesToRebuild = new HashSet<string>(
            (logicalKeysToRebuild ?? Array.Empty<string>())
                .Select(GenerateRawTables.GenerateShortTableName),
            StringComparer.OrdinalIgnoreCase);
    }

    public void Run()
    {
        var tableMap = LoadTableMappings();

        foreach (var view in Views)
        {
            string sql;

            var viewRows = _rows
                .Where(r => r.Range == view.Range)
                .Where(r => r.Type == view.Type)
                .Where(r => !string.IsNullOrWhiteSpace(r.PropertyName))
                .ToList();

            if (viewRows.Count == 0)
                continue;

            if (!ShouldRebuildView(viewRows, tableMap))
            {
                sql = BuildSkippedSql(view.ViewName, "No rebuilt raw tables affect this view.");
                WriteSql("50", view.ViewName, sql);

                var skippedModelFile = Path.Combine(_generatedJsonDir, $"{view.ModelName}.json");
                var skippedJsonSql = $@"\copy (select json_array(select row_to_json(r) from(select * from {view.ViewName} where ""URN"" in (select ""URN"" from test_establishments_urns union all select ""NeighbourURN"" from v_similar_schools_secondary_groups where ""URN"" in (select ""URN"" from test_establishments_urns))) r)) to '{skippedModelFile}' with(format text);";
                WriteSql("70", view.ViewName, skippedJsonSql);
                continue;
            }

            sql = GenerateMaterializedView(view.ViewName, viewRows, tableMap);
            WriteSql("50", view.ViewName, sql);

            var modelFile = Path.Combine(_generatedJsonDir, $"{view.ModelName}.json");
            var jsonSql = $@"\copy (select json_array(select row_to_json(r) from(select * from {view.ViewName} where ""URN"" in (select ""URN"" from test_establishments_urns union all select ""NeighbourURN"" from v_similar_schools_secondary_groups where ""URN"" in (select ""URN"" from test_establishments_urns))) r)) to '{modelFile}' with(format text);";
            WriteSql("70", view.ViewName, jsonSql);

            Console.WriteLine($"Generated {view.ViewName}");
        }

    }

    private bool ShouldRebuildView(List<DataMapRow> rows, Dictionary<string, string> tableMap)
    {
        if (_rebuildAllRawTables)
            return true;

        if (_rawTableNamesToRebuild.Count == 0)
            return false;

        var datasetKeys = rows
            .Select(r => (r.FileName ?? "").Trim().TrimStart('\uFEFF'))
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .Distinct(StringComparer.OrdinalIgnoreCase);

        foreach (var datasetKey in datasetKeys)
        {
            if (TryResolveRawTable(tableMap, datasetKey, out var rawTable) &&
                !string.IsNullOrWhiteSpace(rawTable) &&
                _rawTableNamesToRebuild.Contains(rawTable))
            {
                return true;
            }
        }

        return false;
    }

    private static string BuildSkippedSql(string viewName, string reason)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"-- AUTO-GENERATED MATERIALIZED VIEW: {viewName}");
        sb.AppendLine("-- NOTE: This file was generated but the view SQL was skipped.");
        sb.AppendLine("-- In selective rebuild mode, the existing materialized view must already exist.");
        sb.AppendLine($"-- REASON: {reason}");
        sb.AppendLine();
        sb.AppendLine("DO $$");
        sb.AppendLine("DECLARE");
        sb.AppendLine("  v_schema text := current_schema();");
        sb.AppendLine("BEGIN");
        sb.AppendLine($"  IF to_regclass(format('%I.%I', v_schema, '{viewName}')) IS NULL THEN");
        sb.AppendLine($"    RAISE EXCEPTION 'Skipped view {viewName} does not exist in schema %, but is required for selective rebuild. {reason}', v_schema;");
        sb.AppendLine("  END IF;");
        sb.AppendLine("END $$;");
        return sb.ToString();
    }

    // =====================================================
    // GENERIC MATERIALIZED VIEW (FACT VIEWS)
    // =====================================================

    private string GenerateMaterializedView(string viewName, List<DataMapRow> rows, Dictionary<string, string> tableMap)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"-- AUTO-GENERATED MATERIALIZED VIEW: {viewName}");
        sb.AppendLine();
        sb.AppendLine($"DROP MATERIALIZED VIEW IF EXISTS {viewName};");
        sb.AppendLine();
        sb.AppendLine($"CREATE MATERIALIZED VIEW {viewName} AS");

        var groups = rows.GroupBy(r => r.FileName).ToList();

        sb.AppendLine("SELECT");

        for (int i = 0; i < groups.Count; i++)
        {
            var g = groups[i];
            var props = g.ToList();
            for (int j = 0; j < props.Count; j++)
            {
                //var file = groups[i][j].FileName;
                var prop = props[j].PropertyName;
                var comma = i == groups.Count - 1 && j == props.Count - 1 ? "" : ",";

                sb.AppendLine($"    src_{i + 1}.\"{DbCol(props[j].Field)}\" AS \"{prop}\"{comma}");
            }
        }

        for (int i = 0; i < groups.Count; i++)
        {
            var g = groups[i];
            if (!tableMap.TryGetValue(g.Key, out var rawTable))
                throw new InvalidOperationException($"Missing table mapping for {g.Key}");

            if (i == 0)
            {
                sb.AppendLine($"FROM {rawTable} src_{i + 1}");
            }
            else
            {
                sb.AppendLine(
                    $"LEFT JOIN {rawTable} src_{i + 1} ON src_{i + 1}.\"Id\" = src_1.\"Id\""
                );
            }
        }

        sb.AppendLine(";");

        return sb.ToString();
    }

    // =====================================================
    // HELPERS
    // =====================================================

    private Dictionary<string, string> LoadTableMappings() =>
        File.ReadAllLines(_tableMappingPath)
            .Select(l => l.Split(','))
            .Where(p => p.Length == 2)
            .ToDictionary(p => p[0], p => p[1]);

    private static bool TryResolveRawTable(
        Dictionary<string, string> tableMap,
        string? datasetKey,
        out string? rawTable)
    {
        rawTable = "";

        if (string.IsNullOrWhiteSpace(datasetKey))
            return false;

        var key = datasetKey.Trim().TrimStart('\uFEFF');

        if (tableMap.TryGetValue(key, out rawTable))
            return true;

        var manualShortKey = "m_" + key;
        if (tableMap.TryGetValue(manualShortKey, out rawTable))
            return true;

        var manualLongKey = "manual_" + key;
        if (tableMap.TryGetValue(manualLongKey, out rawTable))
            return true;

        return false;
    }

    // =====================================================
    // COLUMN NORMALISATION (DataMap header -> raw table column)
    // =====================================================
    private static string DbCol(string? header)
    {
        if (string.IsNullOrWhiteSpace(header))
            return header ?? "";

        // Match GenerateRawTables.Sanitise behaviour (lower + non-alnum -> '_')
        var s = header.Trim().ToLowerInvariant();
        var sb = new StringBuilder(s.Length);
        foreach (var ch in s)
            sb.Append(char.IsLetterOrDigit(ch) ? ch : '_');

        return sb.ToString();
    }

    private void WriteSql(string prefix, string viewName, string sql)
    {
        var fileName = $"{prefix}_{viewName}.sql";

        File.WriteAllText(
            Path.Combine(_sqlDir, fileName),
            sql,
            new UTF8Encoding(false));
        _sqlFiles.Add($"{prefix}_{viewName}.sql");

        Console.WriteLine($"Generated view script: {fileName}");
    }
}
