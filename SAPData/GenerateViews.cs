using SAPData.Models;
using System.Text;

namespace SAPData;

public sealed class GenerateViews
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
        new("v_establishment", "Establishment", "Establishment"),
        new("v_establishment_absence", "Establishment", "PupilAbsence"),
        new("v_establishment_destinations", "Establishment", "KS4_Destinations"),
        new("v_establishment_performance", "Establishment", "KS4_Performance"),
        new("v_establishment_workforce", "Establishment", "Workforce"),
        new("v_england_destinations", "England", "KS4_Destinations"),
        new("v_england_performance", "England", "KS4_Performance"),
        new("v_la_destinations", "LA", "KS4_Destinations"),
        new("v_la_performance", "LA", "KS4_Performance")
    };

    public GenerateViews(IReadOnlyList<DataMapRow> rows, string tableMappingPath, string sqlDir)
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

            if (view.ViewName == "v_establishment")
            {
                if (!tableMap.Keys.Any(k =>
                    k.TrimStart('\uFEFF')
                     .StartsWith("edubase", StringComparison.OrdinalIgnoreCase)))
                {
                    continue; // no establishment source available
                }

                sql = GenerateEstablishmentDimensionView(tableMap);
            }
            else
            {
                var viewRows = _rows
                    .Where(r => r.Range == view.Range)
                    .Where(r => r.Type == view.Type)
                    .Where(r => !string.IsNullOrWhiteSpace(r.PropertyName))
                    .ToList();

                if (viewRows.Count == 0)
                    continue;

                sql = GenerateMaterializedView(view.ViewName, viewRows, tableMap);
            }

            File.WriteAllText(
                Path.Combine(_sqlDir, $"03_{view.ViewName}.sql"),
                sql,
                new UTF8Encoding(false));

            Console.WriteLine($"Generated {view.ViewName}");
        }
    }

    // =====================================================
    // ESTABLISHMENT DIMENSION (EDUBASE)
    // =====================================================

    private string GenerateEstablishmentDimensionView(
        Dictionary<string, string> tableMap)
    {
        // Single source of truth for establishment
        var rawTable = tableMap
            .First(kvp => kvp.Key.StartsWith("edubase", StringComparison.OrdinalIgnoreCase))
            .Value;

        var sb = new StringBuilder();

        sb.AppendLine("-- AUTO-GENERATED MATERIALIZED VIEW: v_establishment");
        sb.AppendLine();
        sb.AppendLine("DROP MATERIALIZED VIEW IF EXISTS v_establishment CASCADE;");
        sb.AppendLine();
        sb.AppendLine("CREATE MATERIALIZED VIEW v_establishment AS");
        sb.AppendLine("SELECT");
        sb.AppendLine("    t.\"URN\"                                AS \"URN\",");
        sb.AppendLine("    t.\"LA (code)\"                          AS \"LAId\",");
        sb.AppendLine("    t.\"LA (name)\"                          AS \"LAName\",");
        sb.AppendLine("    t.\"EstablishmentName\"                  AS \"EstablishmentName\",");
        sb.AppendLine("    clean_int(t.\"EstablishmentNumber\")     AS \"EstablishmentNumber\",");
        sb.AppendLine();
        sb.AppendLine("    clean_int(t.\"Trusts (code)\")            AS \"TrustsId\",");
        sb.AppendLine("    t.\"Trusts (name)\"                      AS \"TrustName\",");
        sb.AppendLine();
        sb.AppendLine("    clean_int(t.\"AdmissionsPolicy (code)\") AS \"AdmissionsPolicyId\",");
        sb.AppendLine("    t.\"AdmissionsPolicy (name)\"            AS \"AdmissionPolicy\",");
        sb.AppendLine();
        sb.AppendLine("    t.\"DistrictAdministrative (code)\"      AS \"DistrictAdministrativeId\",");
        sb.AppendLine("    t.\"DistrictAdministrative (name)\"      AS \"DistrictAdministrativeName\",");
        sb.AppendLine();
        sb.AppendLine("    clean_int(t.\"PhaseOfEducation (code)\") AS \"PhaseOfEducationId\",");
        sb.AppendLine("    t.\"PhaseOfEducation (name)\"             AS \"PhaseOfEducationName\",");
        sb.AppendLine();
        sb.AppendLine("    clean_int(t.\"Gender (code)\")            AS \"GenderId\",");
        sb.AppendLine("    t.\"Gender (name)\"                       AS \"GenderName\",");
        sb.AppendLine();
        sb.AppendLine("    clean_int(t.\"OfficialSixthForm (code)\") AS \"OfficialSixthFormId\",");
        sb.AppendLine("    clean_int(t.\"ReligiousCharacter (code)\") AS \"ReligiousCharacterId\",");
        sb.AppendLine("    t.\"ReligiousCharacter (name)\"           AS \"ReligiousCharacterName\",");
        sb.AppendLine();
        sb.AppendLine("    t.\"TelephoneNum\"                        AS \"TelephoneNum\",");
        sb.AppendLine("    clean_int(t.\"NumberOfPupils\")           AS \"TotalPupils\",");
        sb.AppendLine();
        sb.AppendLine("    clean_int(t.\"TypeOfEstablishment (code)\") AS \"TypeOfEstablishmentId\",");
        sb.AppendLine("    t.\"TypeOfEstablishment (name)\"          AS \"TypeOfEstablishmentName\",");
        sb.AppendLine();
        sb.AppendLine("    clean_int(t.\"ResourcedProvisionOnRoll\") AS \"ResourcedProvision\",");
        sb.AppendLine("    t.\"TypeOfResourcedProvision (name)\"     AS \"ResourcedProvisionName\",");
        sb.AppendLine();
        sb.AppendLine("    clean_int(t.\"UKPRN\")                    AS \"UKPRN\",");
        sb.AppendLine();
        sb.AppendLine("    t.\"Street\"                              AS \"Street\",");
        sb.AppendLine("    t.\"Locality\"                            AS \"Locality\",");
        sb.AppendLine("    t.\"Address3\"                            AS \"Address3\",");
        sb.AppendLine("    t.\"Town\"                                AS \"Town\",");
        sb.AppendLine("    t.\"County (name)\"                       AS \"County\",");
        sb.AppendLine("    t.\"Postcode\"                            AS \"Postcode\",");
        sb.AppendLine();
        sb.AppendLine("    t.\"HeadTitle (name)\"                    AS \"HeadTitle\",");
        sb.AppendLine("    t.\"HeadFirstName\"                       AS \"HeadFirstName\",");
        sb.AppendLine("    t.\"HeadLastName\"                        AS \"HeadLastName\",");
        sb.AppendLine("    t.\"HeadPreferredJobTitle\"               AS \"HeadPreferredJobTitle\",");
        sb.AppendLine();
        sb.AppendLine("    t.\"UrbanRural (code)\"                   AS \"UrbanRuralId\",");
        sb.AppendLine("    t.\"UrbanRural (name)\"                   AS \"UrbanRuralName\",");
        sb.AppendLine();
        sb.AppendLine("    t.\"SchoolWebsite\"                       AS \"Website\",");
        sb.AppendLine("    clean_int(t.\"Easting\")                  AS \"Easting\",");
        sb.AppendLine("    clean_int(t.\"Northing\")                 AS \"Northing\"");
        sb.AppendLine($"FROM {rawTable} t;");
        sb.AppendLine();
        sb.AppendLine("CREATE UNIQUE INDEX idx_v_establishment_urn ON v_establishment (\"URN\");");

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
        sb.AppendLine("WITH");

        var groups = rows.GroupBy(r => r.FileName).ToList();

        for (int i = 0; i < groups.Count; i++)
        {
            var g = groups[i];
            var r0 = g.First();

            if (!tableMap.TryGetValue(g.Key, out var rawTable))
                throw new InvalidOperationException($"Missing table mapping for {g.Key}");

            sb.AppendLine($"src_{i + 1} AS (");
            sb.AppendLine("    SELECT");
            sb.AppendLine($"        t.\"{r0.RecordFilterBy}\" AS \"Id\",");

            var props = g
                .GroupBy(r => r.PropertyName)
                .Select(BuildAggregatedExpression)
                .ToList();

            for (int j = 0; j < props.Count; j++)
            {
                sb.AppendLine($"        {props[j]}{(j == props.Count - 1 ? "" : ",")}");
            }

            sb.AppendLine($"    FROM {rawTable} t");
            sb.AppendLine($"    GROUP BY t.\"{r0.RecordFilterBy}\"");
            sb.AppendLine(i == groups.Count - 1 ? ")" : "),");
        }

        sb.AppendLine();
        sb.AppendLine("SELECT");

        var propertySources = groups
            .SelectMany((g, idx) =>
                g.Select(r => new { r.PropertyName, Source = idx + 1 }))
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.Source).Distinct().OrderBy(x => x).ToList()
            );

        sb.AppendLine(
            $"    COALESCE({string.Join(", ",
                groups.Select((_, idx) => $"src_{idx + 1}.\"Id\"")
            )}) AS \"Id\","
        );

        var orderedProps = propertySources.Keys.OrderBy(p => p).ToList();

        for (int i = 0; i < orderedProps.Count; i++)
        {
            var prop = orderedProps[i];
            var sources = propertySources[prop];
            var comma = i == orderedProps.Count - 1 ? "" : ",";

            var expr = sources.Count == 1
                ? $"src_{sources[0]}.\"{prop}\""
                : $"COALESCE({string.Join(", ",
                    sources.Select(s => $"src_{s}.\"{prop}\"")
                )})";

            sb.AppendLine($"    {expr} AS \"{prop}\"{comma}");
        }

        sb.AppendLine("FROM src_1");

        for (int i = 1; i < groups.Count; i++)
        {
            sb.AppendLine(
                $"LEFT JOIN src_{i + 1} ON src_{i + 1}.\"Id\" = src_1.\"Id\""
            );
        }

        sb.AppendLine(";");

        return sb.ToString();
    }

    // =====================================================
    // HELPERS
    // =====================================================

    private static string BuildAggregatedExpression(IEnumerable<DataMapRow> rows)
    {
        var r = rows.First();
        var conditions = new List<string>();

        if (!string.IsNullOrWhiteSpace(r.Filter))
            conditions.Add($"t.\"{r.Filter}\" = '{r.FilterValue}'");
        if (!string.IsNullOrWhiteSpace(r.Filter2))
            conditions.Add($"t.\"{r.Filter2}\" = '{r.Filter2Value}'");
        if (!string.IsNullOrWhiteSpace(r.Filter3))
            conditions.Add($"t.\"{r.Filter3}\" = '{r.Filter3Value}'");

        var whenClause = conditions.Count == 0 ? "TRUE" : string.Join(" AND ", conditions);

        return
            $"MAX(CASE WHEN {whenClause} THEN {BuildValueExpression(r)} END) AS \"{r.PropertyName}\"";
    }

    private static string BuildValueExpression(DataMapRow r) =>
        r.DataType?.ToLowerInvariant() switch
        {
            "int" => $"clean_int(t.\"{r.Field}\")",
            "percentage" => $"clean_numeric(t.\"{r.Field}\")",
            "numeric" => $"clean_numeric(t.\"{r.Field}\")",
            _ => $"t.\"{r.Field}\""
        };

    private Dictionary<string, string> LoadTableMappings() =>
        File.ReadAllLines(_tableMappingPath)
            .Select(l => l.Split(','))
            .Where(p => p.Length == 2)
            .ToDictionary(p => p[0], p => p[1]);
}
