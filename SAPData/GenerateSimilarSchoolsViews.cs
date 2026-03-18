using SAPData.Models;
using System.Text;

namespace SAPData;

public sealed class GenerateSimilarSchoolsViews
{
    private readonly IReadOnlyList<DataMapRow> _rows;
    private readonly string _tableMappingPath;
    private readonly string _sqlDir;
    private readonly string _jsonDir;

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

    public GenerateSimilarSchoolsViews(IReadOnlyList<DataMapRow> rows, string tableMappingPath, string sqlDir, string jsonDir)
    {
        _rows = rows;
        _tableMappingPath = tableMappingPath;
        _sqlDir = sqlDir;
        _jsonDir = jsonDir;
    }

    public void Run()
    {
        Directory.CreateDirectory(_sqlDir);
        Directory.CreateDirectory(_jsonDir);

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

            sql = GenerateMaterializedView(view.ViewName, viewRows, tableMap);

            File.WriteAllText(
                Path.Combine(_sqlDir, $"50_{view.ViewName}.sql"),
                sql,
                new UTF8Encoding(false));

            var jsonSql = $@"\copy (select json_array(select row_to_json(r) from(select * from {view.ViewName} where ""URN"" in (select ""URN"" from test_establishments_urns union all select ""NeighbourURN"" from v_similar_schools_secondary_groups where ""URN"" in (select ""URN"" from test_establishments_urns))) r)) to '{_jsonDir}\{view.ModelName}.json' with(format text);";

            File.WriteAllText(
                Path.Combine(_sqlDir, $"70_{view.ViewName}.sql"),
                jsonSql,
                new UTF8Encoding(false));

            Console.WriteLine($"Generated {view.ViewName}");
        }

        var viewName = "v_similar_schools_secondary_values_national_sd";
        var modelName = "SimilarSchoolsSecondaryStandardDeviationsEntry";
        var sdSql = GenerateSecondaryValuesNationalSdView(viewName, tableMap);

        File.WriteAllText(
            Path.Combine(_sqlDir, $"50_{viewName}.sql"),
            sdSql,
            new UTF8Encoding(false));

        var sdJsonSql = $@"\copy (select json_array(select row_to_json(r) from(select * from {viewName}) r)) to '{_jsonDir}\{modelName}.json' with(format text);";

        File.WriteAllText(
            Path.Combine(_sqlDir, $"70_{viewName}.sql"),
            sdJsonSql,
            new UTF8Encoding(false));
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
    // Similar Schools - Secondary Values (National SD)
    // =====================================================

    private string GenerateSecondaryValuesNationalSdView(string viewName, Dictionary<string, string> tableMap)
    {
        var viewRows = _rows
            .Where(r => r.Range == "SimilarSchools")
            .Where(r => r.Type == "SecondaryValues")
            .Where(r => !string.IsNullOrWhiteSpace(r.FileName))
            .ToList();

        if (viewRows.Count == 0)
        {
            Console.WriteLine($"Skipped {viewName}: no DataMap rows found.");
            return string.Empty;
        }

        var fileKeys = viewRows
            .Select(r => (r.FileName ?? "").Trim().TrimStart('\uFEFF'))
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (fileKeys.Count != 1)
            throw new InvalidOperationException(
                $"Expected exactly one source file for {viewName}, found {fileKeys.Count}.");

        if (!TryResolveRawTable(tableMap, fileKeys[0], out var rawTable) || string.IsNullOrWhiteSpace(rawTable))
            throw new InvalidOperationException(
                $"Missing table mapping for '{fileKeys[0]}' when generating {viewName}.");

        var sb = new StringBuilder();

        sb.AppendLine($"-- AUTO-GENERATED MATERIALIZED VIEW: {viewName}");
        sb.AppendLine("-- Computes population standard deviation (stddev_pop) for each metric.");
        sb.AppendLine("-- Includes ks2_avg to match UI metric Ks2AverageScore = (ks2_rp + ks2_mp) / 2.");
        sb.AppendLine();
        sb.AppendLine($"DROP MATERIALIZED VIEW IF EXISTS {viewName};");
        sb.AppendLine();
        sb.AppendLine($"CREATE MATERIALIZED VIEW {viewName}");
        sb.AppendLine("TABLESPACE pg_default");
        sb.AppendLine("AS");
        sb.AppendLine("SELECT");
        sb.AppendLine("    count(*)::int AS \"RowCount\",");
        sb.AppendLine();
        sb.AppendLine("    -- Keep these if useful for debugging / reference");
        sb.AppendLine("    stddev_pop(NULLIF(NULLIF(ks2_rp, 'NA'), '')::numeric) AS \"KS2RP\",");
        sb.AppendLine("    stddev_pop(NULLIF(NULLIF(ks2_mp, 'NA'), '')::numeric) AS \"KS2MP\",");
        sb.AppendLine();
        sb.AppendLine("    -- SD for the same metric used in UI");
        sb.AppendLine("    stddev_pop(");
        sb.AppendLine("        (");
        sb.AppendLine("            NULLIF(NULLIF(ks2_rp, 'NA'), '')::numeric +");
        sb.AppendLine("            NULLIF(NULLIF(ks2_mp, 'NA'), '')::numeric");
        sb.AppendLine("        ) / 2.0");
        sb.AppendLine("    ) AS \"KS2AVG\",");
        sb.AppendLine();
        sb.AppendLine("    stddev_pop(NULLIF(NULLIF(pp_perc, 'NA'), '')::numeric)                  AS \"PPPerc\",");
        sb.AppendLine("    stddev_pop(NULLIF(NULLIF(percent_eal, 'NA'), '')::numeric)              AS \"PercentEAL\",");
        sb.AppendLine("    stddev_pop(NULLIF(NULLIF(polar4quintile_pupils, 'NA'), '')::numeric)    AS \"Polar4QuintilePupils\",");
        sb.AppendLine("    stddev_pop(NULLIF(NULLIF(p_stability, 'NA'), '')::numeric)              AS \"PStability\",");
        sb.AppendLine("    stddev_pop(NULLIF(NULLIF(idaci_pupils, 'NA'), '')::numeric)             AS \"IdaciPupils\",");
        sb.AppendLine("    stddev_pop(NULLIF(NULLIF(percent_sch_support, 'NA'), '')::numeric)      AS \"PercentSchSupport\",");
        sb.AppendLine("    stddev_pop(NULLIF(NULLIF(number_of_pupils, 'NA'), '')::numeric)         AS \"NumberOfPupils\",");
        sb.AppendLine("    stddev_pop(NULLIF(NULLIF(percent_statement_or_ehp, 'NA'), '')::numeric) AS \"PercentageStatementOrEHP\",");
        sb.AppendLine("    stddev_pop(NULLIF(NULLIF(att8scr, 'NA'), '')::numeric)                  AS \"Att8Scr\"");
        sb.AppendLine($"FROM public.{rawTable}");
        sb.AppendLine("WITH DATA;");

        Console.WriteLine($"Generated {viewName}");

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
}