using System.Text;

namespace SAPData;

public class GenerateIndexes
{
    private readonly string _sqlDir;
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    public GenerateIndexes(string sqlDir)
    {
        _sqlDir = sqlDir;
    }

    public void Run()
    {
        string outputPath = Path.Combine(_sqlDir, "04_indexes.sql");

        var sb = new StringBuilder();

        sb.AppendLine("-- ================================================================");
        sb.AppendLine("-- 04_indexes.sql");
        sb.AppendLine("-- Indexes for materialized views (AUTO-GENERATED)");
        sb.AppendLine("-- ================================================================");
        sb.AppendLine();
        sb.AppendLine("-- NOTE:");
        sb.AppendLine("-- - Uses quoted identifiers to respect case-sensitive columns");
        sb.AppendLine("-- - Uses current_schema() so it works in any schema (local + pipeline)");
        sb.AppendLine("-- - Guards index creation when a view is not present");
        sb.AppendLine("-- - Emits NOTICE messages so execution output is descriptive");
        sb.AppendLine();

        // You explicitly control which column gets indexed (no guessing).
        // IMPORTANT: Put the identifier exactly as it appears in the materialized view,
        // including quotes where needed (e.g. \"Group UID\").
        var indexes = new Dictionary<string, string>
        {
            // Establishment
            { "v_establishment",                 "\"URN\"" },
            { "v_establishment_links",           "\"urn\"" },
            { "v_establishment_group_links",     "\"group_id\"" },
            { "v_establishment_subject_entries", "\"school_urn\"" },

            { "v_establishment_absence",      "\"Id\"" },
            { "v_establishment_destinations", "\"Id\"" },
            { "v_establishment_performance",  "\"Id\"" },
            { "v_establishment_workforce",    "\"Id\"" },

            // England
            { "v_england_destinations", "\"Id\"" },
            { "v_england_performance",  "\"Id\"" },

            // Local Authority
            { "v_la_destinations", "\"Id\"" },
            { "v_la_performance",  "\"Id\"" }
        };

        foreach (var kvp in indexes)
        {
            string view = kvp.Key;
            string column = kvp.Value;

            string colSlug = column.Trim('"')
                .ToLowerInvariant()
                .Replace(" ", "_");

            string indexName = $"idx_{view}_{colSlug}";

            sb.AppendLine("DO $$");
            sb.AppendLine("DECLARE");
            sb.AppendLine("  v_schema text := current_schema();");
            sb.AppendLine("BEGIN");

            // Check view exists in current schema
            sb.AppendLine($"  IF to_regclass(format('%I.%I', v_schema, '{EscapeSqlLiteral(view)}')) IS NULL THEN");
            sb.AppendLine($"    RAISE NOTICE 'SKIP: view %.% does not exist (index {indexName} not created)', v_schema, '{EscapeSqlLiteral(view)}';");
            sb.AppendLine("  ELSE");

            // Check index exists in current schema
            sb.AppendLine($"    IF to_regclass(format('%I.%I', v_schema, '{EscapeSqlLiteral(indexName)}')) IS NULL THEN");
            sb.AppendLine($"      RAISE NOTICE 'CREATE: index %.% on %.% ({column})', v_schema, '{EscapeSqlLiteral(indexName)}', v_schema, '{EscapeSqlLiteral(view)}';");

            // Create index in current schema on the view in current schema
            // We keep your explicit column string as-is (already quoted as needed).
            sb.AppendLine($"      EXECUTE format('CREATE INDEX %I ON %I.%I ({column})', '{EscapeSqlLiteral(indexName)}', v_schema, '{EscapeSqlLiteral(view)}');");

            sb.AppendLine("    ELSE");
            sb.AppendLine($"      RAISE NOTICE 'OK: index %.% already exists', v_schema, '{EscapeSqlLiteral(indexName)}';");
            sb.AppendLine("    END IF;");
            sb.AppendLine("  END IF;");
            sb.AppendLine("END $$;");
            sb.AppendLine();
        }

        File.WriteAllText(outputPath, sb.ToString(), Utf8NoBom);

        Console.WriteLine("Generated view index script:");
        Console.WriteLine(outputPath);
    }

    private static string EscapeSqlLiteral(string s) => (s ?? "").Replace("'", "''");
}
