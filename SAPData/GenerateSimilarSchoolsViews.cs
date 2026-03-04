using SAPData.Models;
using System.Text;

namespace SAPData;

public sealed class GenerateSimilarSchoolsViews
{
    private readonly IReadOnlyList<DataMapRow> _rows;
    private readonly string _tableMappingPath;
    private readonly string _sqlDir;

    private sealed record ViewSpec(
        string ViewName,
        string Range,
        string Type
    );

    private static readonly ViewSpec[] Views =
    {
        new("v_similar_schools_secondary_groups", "SimilarSchools", "SecondaryGroups"),
        new("v_similar_schools_secondary_values", "SimilarSchools", "SecondaryValues"),
        new("v_similar_schools_primary_groups", "SimilarSchools", "PrimaryGroups"),
        new("v_similar_schools_primary_values", "SimilarSchools", "PrimaryValues"),
    };

    public GenerateSimilarSchoolsViews(IReadOnlyList<DataMapRow> rows, string tableMappingPath, string sqlDir)
    {
        _rows = rows;
        _tableMappingPath = tableMappingPath;
        _sqlDir = sqlDir;
    }

    public void Run()
    {
        Directory.CreateDirectory(_sqlDir);
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

            Console.WriteLine($"Generated {view.ViewName}");
        }

        GenerateSecondaryValuesNationalSdView(tableMap);
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

                sb.AppendLine($"    src_{i + 1}.\"{prop}\" AS \"{prop}\"{comma}");
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

    private void GenerateSecondaryValuesNationalSdView(Dictionary<string, string> tableMap)
    {
        const string viewName = "v_similar_schools_secondary_values_national_sd";

        var viewRows = _rows
            .Where(r => r.Range == "SimilarSchools")
            .Where(r => r.Type == "SecondaryValues")
            .Where(r => !string.IsNullOrWhiteSpace(r.FileName))
            .ToList();

        if (viewRows.Count == 0)
        {
            Console.WriteLine($"Skipped {viewName}: no DataMap rows found.");
            return;
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
        sb.AppendLine("    count(*)::int AS row_count,");
        sb.AppendLine();
        sb.AppendLine("    -- Keep these if useful for debugging / reference");
        sb.AppendLine("    stddev_pop(NULLIF(NULLIF(ks2_rp, 'NA'), '')::numeric) AS ks2_rp,");
        sb.AppendLine("    stddev_pop(NULLIF(NULLIF(ks2_mp, 'NA'), '')::numeric) AS ks2_mp,");
        sb.AppendLine();
        sb.AppendLine("    -- SD for the same metric used in UI");
        sb.AppendLine("    stddev_pop(");
        sb.AppendLine("        (");
        sb.AppendLine("            NULLIF(NULLIF(ks2_rp, 'NA'), '')::numeric +");
        sb.AppendLine("            NULLIF(NULLIF(ks2_mp, 'NA'), '')::numeric");
        sb.AppendLine("        ) / 2.0");
        sb.AppendLine("    ) AS ks2_avg,");
        sb.AppendLine();
        sb.AppendLine("    stddev_pop(NULLIF(NULLIF(pp_perc, 'NA'), '')::numeric)                  AS pp_perc,");
        sb.AppendLine("    stddev_pop(NULLIF(NULLIF(percent_eal, 'NA'), '')::numeric)              AS percent_eal,");
        sb.AppendLine("    stddev_pop(NULLIF(NULLIF(polar4quintile_pupils, 'NA'), '')::numeric)    AS polar4quintile_pupils,");
        sb.AppendLine("    stddev_pop(NULLIF(NULLIF(p_stability, 'NA'), '')::numeric)              AS p_stability,");
        sb.AppendLine("    stddev_pop(NULLIF(NULLIF(idaci_pupils, 'NA'), '')::numeric)             AS idaci_pupils,");
        sb.AppendLine("    stddev_pop(NULLIF(NULLIF(percent_sch_support, 'NA'), '')::numeric)      AS percent_sch_support,");
        sb.AppendLine("    stddev_pop(NULLIF(NULLIF(number_of_pupils, 'NA'), '')::numeric)         AS number_of_pupils,");
        sb.AppendLine("    stddev_pop(NULLIF(NULLIF(percent_statement_or_ehp, 'NA'), '')::numeric) AS percent_statement_or_ehp,");
        sb.AppendLine("    stddev_pop(NULLIF(NULLIF(att8scr, 'NA'), '')::numeric)                  AS att8scr");
        sb.AppendLine($"FROM public.{rawTable}");
        sb.AppendLine("WITH DATA;");

        File.WriteAllText(
            Path.Combine(_sqlDir, $"50_{viewName}.sql"),
            sb.ToString(),
            new UTF8Encoding(false));

        Console.WriteLine($"Generated {viewName}");
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
}
