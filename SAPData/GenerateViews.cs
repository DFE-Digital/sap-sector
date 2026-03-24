using SAPData.Models;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SAPData;

public sealed class GenerateViews
{
    private readonly IReadOnlyList<DataMapRow> _rows;
    private readonly string _tableMappingPath;
    private readonly string _sqlDir;
    private readonly string _jsonDir;
    private readonly string _generatedJsonDir;
    private readonly List<string> _sqlFiles;

    // raw_sources.json path in repo
    private static readonly string[] RawSourcesCandidates =
    {
        "raw_sources.json",
        Path.Combine("SAPData", "raw_sources.json"),
        Path.Combine(AppContext.BaseDirectory, "raw_sources.json"),
        Path.Combine(AppContext.BaseDirectory, "SAPData", "raw_sources.json")
    };

    private enum ViewRange
    {
        Establishment,
        England,
        LA
    }

    private sealed record ViewSpec(
        string ViewName,
        string IdColumn,
        ViewRange Range,
        string Type,
        string ModelName);

    private sealed record RawSource(
        string Type,
        string Subtype,
        string Year,
        string SourceOrg,
        string FileName
    );

    private static readonly ViewSpec[] Views =
    {
        new("v_establishment", "URN", ViewRange.Establishment, "Establishment", "Establishment"),
        new("v_establishment_links", "urn", ViewRange.Establishment, "Establishment", "EstablishmentLinks"),
        new("v_establishment_group_links", "group_id", ViewRange.Establishment, "Establishment", "EstablishmentGroupLinks"),
        new("v_establishment_subject_entries", "school_urn", ViewRange.Establishment, "KS4_Performance", "EstablishmentSubjectEntries"),
        new("v_establishment_email", "URN", ViewRange.Establishment, "Email", "EstablishmentEmail"),

        new("v_establishment_absence", "Id", ViewRange.Establishment, "PupilAbsence", "EstablishmentAbsence"),
        new("v_establishment_destinations", "Id", ViewRange.Establishment, "KS4_Destinations", "EstablishmentDestinations"),
        new("v_establishment_performance", "Id", ViewRange.Establishment, "KS4_Performance", "EstablishmentPerformance"),
        new("v_establishment_workforce", "Id", ViewRange.Establishment, "Workforce", "EstablishmentWorkforce"),

        new("v_england_absence", "Id", ViewRange.England, "PupilAbsence", "EnglandAbsence"),
        new("v_england_destinations", "Id", ViewRange.England, "KS4_Destinations", "EnglandDestinations"),
        new("v_england_performance", "Id", ViewRange.England, "KS4_Performance", "EnglandPerformance"),

        new("v_la_absence", "Id", ViewRange.LA, "PupilAbsence", "LAAbsence"),
        new("v_la_destinations", "Id", ViewRange.LA, "KS4_Destinations", "LADestinations"),
        new("v_la_performance", "Id", ViewRange.LA, "KS4_Performance", "LAPerformance"),
        new("v_la_subject_entries", "old_la_code", ViewRange.LA, "KS4_Performance", "LASubjectEntries")
    };

    public GenerateViews(
        IReadOnlyList<DataMapRow> rows,
        string tableMappingPath,
        string sqlDir,
        string jsonDir,
        string generatedJsonDir,
        List<string> sqlFiles)
    {
        _rows = rows;
        _tableMappingPath = tableMappingPath;
        _sqlDir = sqlDir;
        _jsonDir = jsonDir;
        _generatedJsonDir = generatedJsonDir;
        _sqlFiles = sqlFiles;
    }

    public void Run()
    {
        var tableMap = LoadTableMappings();
        var sources = LoadRawSources();
        var testEstablishmentUrnsFile = Path.Combine(_jsonDir, "TestEstablishmentUrns.json");

        WriteSql("60", "test_establishments_urns", $"""
            drop table if exists test_establishments_urns_import;
            create unlogged table test_establishments_urns_import (doc text);

            drop table if exists test_establishments_urns;
            create table test_establishments_urns ("URN" text);

            \copy test_establishments_urns_import FROM '{testEstablishmentUrnsFile}' with (format text);
            insert into test_establishments_urns select jsonb_array_elements_text(string_agg(doc, E' ')::jsonb) from test_establishments_urns_import;
            """);

        foreach (var view in Views)
        {
            string sql, jsonSql;

            // 1) Establishment dimension (GIAS edubasealldataYYYYmmDD)
            if (view.ViewName.Equals("v_establishment", StringComparison.OrdinalIgnoreCase))
            {
                if (!TryResolveManagedDatasetKey(
                        sources,
                        tableMap,
                        sourceOrg: "GIAS",
                        type: "All establishment",
                        subtype: "Metadata",
                        year: "Current",
                        out var datasetKey))
                {
                    sql = BuildSkippedSql(view.ViewName, "Could not resolve dataset key from raw_sources.json (GIAS/All establishment/Metadata/Current).");
                    WriteSql("03", view.ViewName, sql);
                    continue;
                }

                if (!TryResolveRawTable(tableMap, datasetKey, out var rawTable))
                {
                    sql = BuildSkippedSql(view.ViewName, $"Could not resolve raw table mapping for datasetKey='{datasetKey}'.");
                    WriteSql("03", view.ViewName, sql);
                    continue;
                }

                sql = GenerateEstablishmentDimensionView(rawTable);
                WriteSql("03", view.ViewName, sql);
            }

            // 2) Mirror view (GIAS: all establishment links)
            else if (view.ViewName.Equals("v_establishment_links", StringComparison.OrdinalIgnoreCase))
            {
                if (!TryResolveManagedDatasetKey(
                        sources,
                        tableMap,
                        sourceOrg: "GIAS",
                        type: "All establishment",
                        subtype: "Links",
                        year: "Current",
                        out var datasetKey))
                {
                    sql = BuildSkippedSql(view.ViewName, "Could not resolve dataset key from raw_sources.json (GIAS/All establishment/Links/Current).");
                    WriteSql("04", view.ViewName, sql);
                    continue;
                }

                if (!TryResolveRawTable(tableMap, datasetKey, out var rawTable))
                {
                    sql = BuildSkippedSql(view.ViewName, $"Could not resolve raw table mapping for datasetKey='{datasetKey}'.");
                    WriteSql("04", view.ViewName, sql);
                    continue;
                }

                sql = GenerateMirrorMaterializedView(view.ViewName, rawTable);
                WriteSql("04", view.ViewName, sql);
            }

            // 3) Mirror view (GIAS: academy sponsor/trust links)
            else if (view.ViewName.Equals("v_establishment_group_links", StringComparison.OrdinalIgnoreCase))
            {
                if (!TryResolveManagedDatasetKey(
                        sources,
                        tableMap,
                        sourceOrg: "GIAS",
                        type: "Academy sponsor and trust",
                        subtype: "Links",
                        year: "Current",
                        out var datasetKey))
                {
                    sql = BuildSkippedSql(view.ViewName, "Could not resolve dataset key from raw_sources.json (GIAS/Academy sponsor and trust/Links/Current).");
                    WriteSql("04", view.ViewName, sql);
                    continue;
                }

                if (!TryResolveRawTable(tableMap, datasetKey, out var rawTable))
                {
                    sql = BuildSkippedSql(view.ViewName, $"Could not resolve raw table mapping for datasetKey='{datasetKey}'.");
                    WriteSql("04", view.ViewName, sql);
                    continue;
                }

                sql = GenerateMirrorMaterializedView(view.ViewName, rawTable);
                WriteSql("04", view.ViewName, sql);
            }

            // 4) Mirror view (EES: SubjectEntries_2 = school / establishment subject entries)
            else if (view.ViewName.Equals("v_establishment_subject_entries", StringComparison.OrdinalIgnoreCase))
            {
                if (!TryResolveManagedDatasetKey(
                        sources,
                        tableMap,
                        sourceOrg: "EES",
                        type: "KS4_Performance",
                        subtype: "SubjectEntries_2",
                        year: "Current",
                        out var datasetKey))
                {
                    sql = BuildSkippedSql(view.ViewName, "Could not resolve dataset key from raw_sources.json (EES/KS4_Performance/SubjectEntries_2/Current).");
                    WriteSql("04", view.ViewName, sql);
                    continue;
                }

                if (!TryResolveRawTable(tableMap, datasetKey, out var rawTable))
                {
                    sql = BuildSkippedSql(view.ViewName, $"Could not resolve raw table mapping for datasetKey='{datasetKey}'.");
                    WriteSql("04", view.ViewName, sql);
                    continue;
                }

                sql = GenerateMirrorMaterializedView(view.ViewName, rawTable);
                WriteSql("04", view.ViewName, sql);
            }

            // 5) Mirror view (EES: SubjectEntries = LA subject entries)
            else if (view.ViewName.Equals("v_la_subject_entries", StringComparison.OrdinalIgnoreCase))
            {
                if (!TryResolveManagedDatasetKey(
                        sources,
                        tableMap,
                        sourceOrg: "EES",
                        type: "KS4_Performance",
                        subtype: "SubjectEntries",
                        year: "Current",
                        out var datasetKey))
                {
                    sql = BuildSkippedSql(view.ViewName, "Could not resolve dataset key from raw_sources.json (EES/KS4_Performance/SubjectEntries/Current).");
                    WriteSql("04", view.ViewName, sql);
                    continue;
                }

                if (!TryResolveRawTable(tableMap, datasetKey, out var rawTable))
                {
                    sql = BuildSkippedSql(view.ViewName, $"Could not resolve raw table mapping for datasetKey='{datasetKey}'.");
                    WriteSql("04", view.ViewName, sql);
                    continue;
                }

                sql = GenerateMirrorMaterializedView(view.ViewName, rawTable);
                WriteSql("04", view.ViewName, sql);
            }
            // 6) Everything else uses DataMap-driven materialized view generation
            else
            {
                var viewRows = _rows
                    .Where(r => r.Range == view.Range.ToString())
                    .Where(r => r.Type == view.Type)
                    .Where(r => !string.IsNullOrWhiteSpace(r.PropertyName))
                    .Where(r => !IsIgnored(r))
                    .ToList();

                var ignoredRows = _rows
                    .Where(r => r.Range == view.Range.ToString())
                    .Where(r => r.Type == view.Type)
                    .Where(r => !string.IsNullOrWhiteSpace(r.PropertyName))
                    .Where(IsIgnored)
                    .ToList();

                if (ignoredRows.Count > 0)
                {
                    Console.WriteLine($"Ignoring {ignoredRows.Count} DataMap rows for {view.ViewName}");
                    foreach (var row in ignoredRows)
                        Console.WriteLine($"Ignored mapping: {row.Ref} ({row.PropertyName})");
                }

                if (viewRows.Count == 0)
                {
                    sql = BuildSkippedSql(view.ViewName, $"No DataMap rows found for Range='{view.Range}', Type='{view.Type}'.");
                    WriteSql("04", view.ViewName, sql);
                    continue;
                }

                sql = GenerateMaterializedView(view.ViewName, viewRows, tableMap);
                WriteSql("04", view.ViewName, sql);
            }

            var modelFile = Path.Combine(_generatedJsonDir, $"{view.ModelName}.json");

            jsonSql = view switch
            {
                _ when view.ViewName == "v_establishment_group_links" =>
                    $@"\copy (select json_array(select row_to_json(r) from(select * from {view.ViewName}) r)) to '{modelFile}' with(format text);",

                _ when view.ViewName == "v_la_subject_entries" =>
                    $@"\copy (select json_array(select row_to_json(r) from(select * from {view.ViewName} where ""{view.IdColumn}"" IN (select distinct ""LAId"" from v_establishment where ""URN"" in (select ""URN"" from test_establishments_urns)) and ""subject"" = ANY(ARRAY['Biology','Chemistry','Mathematics','Physics','English Language','English Literature','Combined Science'])) r)) to '{modelFile}' with(format text);",

                _ when view.ViewName == "v_establishment_subject_entries" =>
                    $@"\copy (select json_array(select row_to_json(r) from(select * from {view.ViewName} where ""{view.IdColumn}"" in (select ""URN"" from test_establishments_urns union all select ""NeighbourURN"" from v_similar_schools_secondary_groups where ""URN"" in (select ""URN"" from test_establishments_urns)) and ""subject"" = ANY(ARRAY['Biology','Chemistry','Mathematics','Physics','English Language','English Literature','Combined Science'])) r)) to '{modelFile}' with(format text);",

                _ when view.Range == ViewRange.Establishment =>
                    $@"\copy (select json_array(select row_to_json(r) from(select * from {view.ViewName} where ""{view.IdColumn}"" in (select ""URN"" from test_establishments_urns union all select ""NeighbourURN"" from v_similar_schools_secondary_groups where ""URN"" in (select ""URN"" from test_establishments_urns))) r)) to '{modelFile}' with(format text);",

                _ =>
                    $@"\copy (select json_array(select row_to_json(r) from(select * from {view.ViewName}) r)) to '{modelFile}' with(format text);"
            };

            WriteSql("61", view.ViewName, jsonSql);
        }
    }

    private static string BuildSkippedSql(string viewName, string reason)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"-- AUTO-GENERATED MATERIALIZED VIEW: {viewName}");
        sb.AppendLine("-- NOTE: This file was generated but the view SQL was skipped.");
        sb.AppendLine($"-- REASON: {reason}");
        sb.AppendLine();
        sb.AppendLine($"-- DROP MATERIALIZED VIEW IF EXISTS {viewName};");
        sb.AppendLine($"-- CREATE MATERIALIZED VIEW {viewName} AS");
        sb.AppendLine($"-- SELECT NULL::text AS \"Skipped\";");
        return sb.ToString();
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

    // =====================================================
    // ESTABLISHMENT DIMENSION (curated)
    // =====================================================

    private static string GenerateEstablishmentDimensionView(string? rawTable)
    {
        var sb = new StringBuilder();

        sb.AppendLine("-- AUTO-GENERATED MATERIALIZED VIEW: v_establishment");
        sb.AppendLine();
        sb.AppendLine("DROP MATERIALIZED VIEW IF EXISTS v_establishment CASCADE;");
        sb.AppendLine();
        sb.AppendLine("CREATE MATERIALIZED VIEW v_establishment AS");
        sb.AppendLine("SELECT");
        sb.AppendLine("    t.\"urn\"                                      AS \"URN\",");
        sb.AppendLine("    t.\"la__code_\"                                AS \"LAId\",");
        sb.AppendLine("    t.\"la__name_\"                                AS \"LAName\",");
        sb.AppendLine("    t.\"gor__code_\"                               AS \"RegionId\",");
        sb.AppendLine("    t.\"gor__name_\"                               AS \"RegionName\",");
        sb.AppendLine("    t.\"establishmentname\"                        AS \"EstablishmentName\",");
        sb.AppendLine("    t.\"establishmentnumber\"                      AS \"EstablishmentNumber\",");
        sb.AppendLine();
        sb.AppendLine("    t.\"trusts__code_\"                            AS \"TrustId\",");
        sb.AppendLine("    t.\"trusts__name_\"                            AS \"TrustName\",");
        sb.AppendLine();
        sb.AppendLine("    t.\"admissionspolicy__code_\"                  AS \"AdmissionsPolicyId\",");
        sb.AppendLine("    t.\"admissionspolicy__name_\"                  AS \"AdmissionsPolicyName\",");
        sb.AppendLine();
        sb.AppendLine("    t.\"districtadministrative__code_\"            AS \"DistrictAdministrativeId\",");
        sb.AppendLine("    t.\"districtadministrative__name_\"            AS \"DistrictAdministrativeName\",");
        sb.AppendLine();
        sb.AppendLine("    t.\"phaseofeducation__code_\"                  AS \"PhaseOfEducationId\",");
        sb.AppendLine("    t.\"phaseofeducation__name_\"                  AS \"PhaseOfEducationName\",");
        sb.AppendLine();
        sb.AppendLine("    t.\"gender__code_\"                            AS \"GenderId\",");
        sb.AppendLine("    t.\"gender__name_\"                            AS \"GenderName\",");
        sb.AppendLine();
        sb.AppendLine("    t.\"religiouscharacter__code_\"                AS \"ReligiousCharacterId\",");
        sb.AppendLine("    t.\"religiouscharacter__name_\"                AS \"ReligiousCharacterName\",");
        sb.AppendLine();
        sb.AppendLine("    t.\"telephonenum\"                             AS \"TelephoneNum\",");
        sb.AppendLine("    clean_int(t.\"schoolcapacity\")                AS \"TotalCapacity\",");
        sb.AppendLine("    clean_int(t.\"numberofpupils\")                AS \"TotalPupils\",");
        sb.AppendLine();
        sb.AppendLine("    t.\"typeofestablishment__code_\"               AS \"TypeOfEstablishmentId\",");
        sb.AppendLine("    t.\"typeofestablishment__name_\"               AS \"TypeOfEstablishmentName\",");
        sb.AppendLine();
        sb.AppendLine("    t.\"establishmenttypegroup__code_\"            AS \"EstablishmentTypeGroupId\",");
        sb.AppendLine("    t.\"establishmenttypegroup__name_\"            AS \"EstablishmentTypeGroupName\",");
        sb.AppendLine();
        sb.AppendLine("    t.\"resourcedprovisiononroll\"                 AS \"ResourcedProvisionId\",");
        sb.AppendLine("    t.\"typeofresourcedprovision__name_\"          AS \"ResourcedProvisionName\",");
        sb.AppendLine();
        sb.AppendLine("    t.\"nurseryprovision__name_\"                  AS \"NurseryProvisionName\",");
        sb.AppendLine();
        sb.AppendLine("    t.\"officialsixthform__code_\"                 AS \"OfficialSixthFormId\",");
        sb.AppendLine("    t.\"officialsixthform__name_\"                 AS \"OfficialSixthFormName\",");
        sb.AppendLine();
        sb.AppendLine("    t.\"trustschoolflag__code_\"                   AS \"TrustSchoolFlagId\",");
        sb.AppendLine("    t.\"trustschoolflag__name_\"                   AS \"TrustSchoolFlagName\",");
        sb.AppendLine();
        sb.AppendLine("    t.\"ukprn\"                                    AS \"UKPRN\",");
        sb.AppendLine();
        sb.AppendLine("    t.\"street\"                                   AS \"Street\",");
        sb.AppendLine("    t.\"locality\"                                 AS \"Locality\",");
        sb.AppendLine("    t.\"address3\"                                 AS \"Address3\",");
        sb.AppendLine("    t.\"town\"                                     AS \"Town\",");
        sb.AppendLine("    t.\"county__name_\"                            AS \"County\",");
        sb.AppendLine("    t.\"postcode\"                                 AS \"Postcode\",");
        sb.AppendLine();
        sb.AppendLine("    t.\"headtitle__name_\"                         AS \"HeadTitle\",");
        sb.AppendLine("    t.\"headfirstname\"                            AS \"HeadFirstName\",");
        sb.AppendLine("    t.\"headlastname\"                             AS \"HeadLastName\",");
        sb.AppendLine("    t.\"headpreferredjobtitle\"                    AS \"HeadPreferredJobTitle\",");
        sb.AppendLine();
        sb.AppendLine("    t.\"urbanrural__code_\"                        AS \"UrbanRuralId\",");
        sb.AppendLine("    t.\"urbanrural__name_\"                        AS \"UrbanRuralName\",");
        sb.AppendLine();
        sb.AppendLine("    t.\"schoolwebsite\"                            AS \"Website\",");
        sb.AppendLine();
        sb.AppendLine("    clean_int(t.\"easting\")                       AS \"Easting\",");
        sb.AppendLine("    clean_int(t.\"northing\")                      AS \"Northing\",");
        sb.AppendLine();
        sb.AppendLine("    clean_int(t.\"statutorylowage\")               AS \"AgeRangeLow\",");
        sb.AppendLine("    clean_int(t.\"statutoryhighage\")              AS \"AgeRangeHigh\"");
        sb.AppendLine($"FROM {rawTable} t;");
        sb.AppendLine();
        sb.AppendLine("CREATE UNIQUE INDEX idx_v_establishment_urn ON v_establishment (\"URN\");");

        return sb.ToString();
    }

    // =====================================================
    // MIRROR VIEWS
    // =====================================================

    private static string GenerateMirrorMaterializedView(string viewName, string? rawTable)
    {
        var sql = new StringBuilder();

        sql.AppendLine($"-- AUTO-GENERATED MIRROR MATERIALIZED VIEW: {viewName}");
        sql.AppendLine();
        sql.AppendLine($"DROP MATERIALIZED VIEW IF EXISTS {viewName};");
        sql.AppendLine();
        sql.AppendLine($"CREATE MATERIALIZED VIEW {viewName} AS");
        sql.AppendLine($"SELECT * FROM {rawTable};");

        return sql.ToString();
    }

    // =====================================================
    // GENERIC MATERIALIZED VIEW (DataMap-driven)
    // =====================================================

    private string GenerateMaterializedView(string viewName, List<DataMapRow> rows, Dictionary<string, string> tableMap)
    {
        var sql = new StringBuilder();

        sql.AppendLine($"-- AUTO-GENERATED MATERIALIZED VIEW: {viewName}");
        sql.AppendLine();
        sql.AppendLine($"DROP MATERIALIZED VIEW IF EXISTS {viewName};");
        sql.AppendLine();
        sql.AppendLine($"CREATE MATERIALIZED VIEW {viewName} AS");
        sql.AppendLine("WITH");

        var groups = rows.GroupBy(r => (r.FileName ?? "").Trim().TrimStart('\uFEFF')).ToList();

        for (int i = 0; i < groups.Count; i++)
        {
            var g = groups[i];
            var r0 = g.First();

            var fileKey = (g.Key ?? "").Trim().TrimStart('\uFEFF');

            if (!TryResolveRawTable(tableMap, fileKey, out var rawTable))
                throw new InvalidOperationException($"Missing table mapping for '{g.Key}'");

            var idCol = DbCol(r0.RecordFilterBy);

            sql.AppendLine($"src_{i + 1} AS (");
            sql.AppendLine("    SELECT");
            sql.AppendLine($"        t.\"{idCol}\" AS \"Id\",");

            var props = g
                .Where(r => !string.IsNullOrWhiteSpace(r.PropertyName))
                .GroupBy(r => r.PropertyName!)
                .Select(BuildAggregatedExpression)
                .ToList();

            for (int j = 0; j < props.Count; j++)
                sql.AppendLine($"        {props[j]}{(j == props.Count - 1 ? "" : ",")}");

            sql.AppendLine($"    FROM {rawTable} t");
            sql.AppendLine($"    GROUP BY t.\"{idCol}\"");
            sql.AppendLine(")");
            sql.AppendLine(i == groups.Count - 1 ? "," : ",");
        }

        // all_ids
        sql.AppendLine("all_ids AS (");
        for (int i = 0; i < groups.Count; i++)
        {
            var union = i == 0 ? "    " : "    UNION ";
            sql.AppendLine($"{union}SELECT \"Id\" FROM src_{i + 1}");
        }
        sql.AppendLine(")");
        sql.AppendLine();
        sql.AppendLine("SELECT");

        // Always include Id from all_ids
        sql.AppendLine("    a.\"Id\" AS \"Id\",");

        // If this is an establishment-level fact view, include LA/Region dims
        var includeEstablishmentDims =
            viewName.StartsWith("v_establishment_", StringComparison.OrdinalIgnoreCase);

        if (includeEstablishmentDims)
        {
            sql.AppendLine("    e.\"LAId\" AS \"LAId\",");
            sql.AppendLine("    e.\"LAName\" AS \"LAName\",");
            sql.AppendLine("    e.\"RegionId\" AS \"RegionId\",");
            sql.AppendLine("    e.\"RegionName\" AS \"RegionName\",");
        }

        // Build property -> sources lookup
        var propertySources = groups
            .SelectMany((g, idx) => g.Select(r => new { r.PropertyName, Source = idx + 1, Type = r.DataType }))
            .Where(x => !string.IsNullOrWhiteSpace(x.PropertyName))
            .GroupBy(x => x.PropertyName!)
            .ToDictionary(
                g => g.Key,
                g => new { Sources = g.Select(x => x.Source).Distinct().OrderBy(x => x).ToList(), Type = g.Select(x => x.Type).First() }
            );

        var orderedProps = propertySources.Keys.OrderBy(p => p).ToList();

        for (int i = 0; i < orderedProps.Count; i++)
        {
            var prop = orderedProps[i];
            var sources = propertySources[prop].Sources;

            var isLast = i == orderedProps.Count - 1;
            var comma = isLast ? "" : ",";

            var expr = sources.Count == 1
                ? $"src_{sources[0]}.\"{prop}\""
                : $"COALESCE({string.Join(", ", sources.Select(s => $"src_{s}.\"{prop}\""))})";

            sql.AppendLine($"    {expr} AS \"{prop}\"{comma}");
        }

        sql.AppendLine("FROM all_ids a");

        for (int i = 0; i < groups.Count; i++)
            sql.AppendLine($"LEFT JOIN src_{i + 1} ON src_{i + 1}.\"Id\" = a.\"Id\"");

        if (includeEstablishmentDims)
            sql.AppendLine("LEFT JOIN v_establishment e ON e.\"URN\" = a.\"Id\"");

        sql.AppendLine(";");

        return sql.ToString();
    }

    // =====================================================
    // RAW_SOURCES + RESOLUTION
    // =====================================================

    private static List<RawSource> LoadRawSources()
    {
        var path = RawSourcesCandidates.FirstOrDefault(File.Exists);
        if (path == null)
            throw new FileNotFoundException(
                $"raw_sources.json not found. Tried: {string.Join(", ", RawSourcesCandidates)}");

        var json = File.ReadAllText(path);
        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var sources = JsonSerializer.Deserialize<List<RawSource>>(json, opts) ?? new List<RawSource>();

        return sources.Where(s => !string.IsNullOrWhiteSpace(s.FileName)).ToList();
    }

    private static bool TryResolveManagedDatasetKey(
        List<RawSource> sources,
        Dictionary<string, string> tableMap,
        string sourceOrg,
        string type,
        string subtype,
        string year,
        out string datasetKey)
    {
        datasetKey = "";

        static string Norm(string? s) =>
            Regex.Replace((s ?? "").Trim(), "\\s+", " ").ToLowerInvariant();

        bool Matches(RawSource s) =>
            Norm(s.SourceOrg) == Norm(sourceOrg) &&
            Norm(s.Type) == Norm(type) &&
            Norm(s.Subtype) == Norm(subtype);

        var src = sources.FirstOrDefault(s => Matches(s) && Norm(s.Year) == Norm(year));

        if (src == null && Norm(year) == "current")
        {
            var candidates = sources.Where(Matches).ToList();
            if (candidates.Count == 0) return false;

            int YearScore(string y)
            {
                var ny = Norm(y);
                if (ny == "current") return int.MaxValue;

                var digits = new string(ny.Where(char.IsDigit).ToArray());
                if (digits.Length == 0) return 0;

                digits = digits.Length > 8 ? digits[..8] : digits;
                return int.TryParse(digits, out var v) ? v : 0;
            }

            src = candidates.OrderByDescending(s => YearScore(s.Year)).FirstOrDefault();
        }

        if (src == null || string.IsNullOrWhiteSpace(src.FileName))
            return false;

        var pattern = src.FileName.Trim();

        if (pattern.Contains("YYYYmmDD", StringComparison.OrdinalIgnoreCase))
        {
            var regex = BuildYyyyMmDdRegex(pattern);

            var candidates = tableMap.Keys
                .Select(k => k.Trim().TrimStart('\uFEFF'))
                .Select(k => new { Key = k, Match = regex.Match(k) })
                .Where(x => x.Match.Success)
                .Select(x => new { x.Key, Date = x.Match.Groups["date"].Value })
                .Where(x => x.Date.Length == 8)
                .OrderByDescending(x => x.Date, StringComparer.Ordinal)
                .ToList();

            if (candidates.Count == 0)
                return false;

            datasetKey = candidates[0].Key;
            return true;
        }

        var exact = pattern.Trim();

        var exactMatch = tableMap.Keys
            .Select(k => k.Trim().TrimStart('\uFEFF'))
            .FirstOrDefault(k => k.Equals(exact, StringComparison.OrdinalIgnoreCase));

        if (exactMatch != null)
        {
            datasetKey = exactMatch;
            return true;
        }

        var baseName = exact;

        var versionRegex = new Regex(
            "^" + Regex.Escape(baseName) + "_v(?<ver>[0-9]+\\.[0-9]+(\\.[0-9]+)?)$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        var versionCandidates = tableMap.Keys
            .Select(k => k.Trim().TrimStart('\uFEFF'))
            .Select(k => new { Key = k, Match = versionRegex.Match(k) })
            .Where(x => x.Match.Success)
            .Select(x =>
            {
                var verText = x.Match.Groups["ver"].Value;
                Version.TryParse(verText, out var ver);
                return new { x.Key, Version = ver ?? new Version(0, 0) };
            })
            .OrderByDescending(x => x.Version)
            .ToList();

        if (versionCandidates.Count > 0)
        {
            datasetKey = versionCandidates[0].Key;
            return true;
        }

        return false;
    }

    private static Regex BuildYyyyMmDdRegex(string fileNamePattern)
    {
        const string token = "YYYYmmDD";

        var idx = fileNamePattern.IndexOf(token, StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
            throw new ArgumentException($"Pattern does not contain {token}: '{fileNamePattern}'");

        var prefix = fileNamePattern.Substring(0, idx);
        var suffix = fileNamePattern.Substring(idx + token.Length);

        var rx =
            "^" +
            Regex.Escape(prefix) +
            "(?<date>\\d{8})" +
            Regex.Escape(suffix) +
            "(?:\\.(?:csv|zip)(?:\\.zip)?)?$";

        return new Regex(rx, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    // =====================================================
    // HELPERS
    // =====================================================

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

        var versionRegex = new Regex(
            "^" + Regex.Escape(key) + "_v(?<ver>[0-9]+\\.[0-9]+(\\.[0-9]+)?)$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        var best = tableMap.Keys
            .Select(k => k.Trim().TrimStart('\uFEFF'))
            .Select(k => new { Key = k, Match = versionRegex.Match(k) })
            .Where(x => x.Match.Success)
            .Select(x =>
            {
                var verText = x.Match.Groups["ver"].Value;
                Version.TryParse(verText, out var ver);
                return new { x.Key, Version = ver ?? new Version(0, 0) };
            })
            .OrderByDescending(x => x.Version)
            .FirstOrDefault();

        if (best != null && tableMap.TryGetValue(best.Key, out rawTable))
            return true;

        return false;
    }

    private static string BuildAggregatedExpression(IEnumerable<DataMapRow> rows)
    {
        var r = rows.First();
        var conditions = new List<string>();
        static string SqlLiteral(string? s) => (s ?? "").Replace("'", "''");
        static string Condition(string filter, string filterValue) =>
            "(" + string.Join(" OR ", filterValue.Split('+').Select(p => $"t.\"{DbCol(filter)}\" = '{SqlLiteral(p)}'")) + ")";

        if (!string.IsNullOrWhiteSpace(r.Filter))
        {
            conditions.Add(Condition(r.Filter, r.FilterValue));
        }

        if (!string.IsNullOrWhiteSpace(r.Filter2))
        {
            conditions.Add(Condition(r.Filter2, r.Filter2Value));
        }

        if (!string.IsNullOrWhiteSpace(r.Filter3))
        {
            conditions.Add(Condition(r.Filter3, r.Filter3Value));
        }

        var whenClause = conditions.Count == 0 ? "TRUE" : string.Join(" AND ", conditions);

        return $"MAX(CASE WHEN {whenClause} THEN {BuildValueExpression(r)} END) AS \"{r.PropertyName}\"";
    }

    private static string BuildValueExpression(DataMapRow r)
    {
        var col = DbCol(r.Field);

        return r.DataType?.ToLowerInvariant() switch
        {
            "int" => $"clean_int(t.\"{col}\")",
            "percentage" => $"clean_numeric(t.\"{col}\")",
            "numeric" => $"clean_numeric(t.\"{col}\")",
            _ => $"t.\"{col}\""
        };
    }

    private Dictionary<string, string> LoadTableMappings() =>
        File.ReadAllLines(_tableMappingPath)
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(l => l.Split(','))
            .Where(p => p.Length == 2)
            .ToDictionary(
                p => p[0].Trim().TrimStart('\uFEFF'),
                p => p[1].Trim(),
                StringComparer.OrdinalIgnoreCase
            );

    private static bool IsIgnored(DataMapRow r)
    {
        return string.Equals(
            r.IgnoreMapping?.Trim(),
            "Y",
            StringComparison.OrdinalIgnoreCase);
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
