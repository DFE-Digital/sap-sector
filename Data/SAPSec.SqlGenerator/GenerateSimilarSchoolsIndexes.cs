using System.Text;

namespace SAPSec.SqlGenerator;

public class GenerateSimilarSchoolsIndexes
{
    private readonly string _sqlDir;
    private readonly List<string> _sqlFiles;

    public GenerateSimilarSchoolsIndexes(string sqlDir, List<string> sqlFiles)
    {
        _sqlDir = sqlDir;
        _sqlFiles = sqlFiles;
    }

    public void Run()
    {
        var sb = new StringBuilder();

        sb.AppendLine("-- ================================================================");
        sb.AppendLine("-- Indexes for materialized views (AUTO-GENERATED)");
        sb.AppendLine("-- ================================================================");
        sb.AppendLine();
        sb.AppendLine("-- NOTE:");
        sb.AppendLine("-- - Uses quoted identifiers to respect case-sensitive columns");
        sb.AppendLine();

        // View → column mapping (EXACT column names)
        var indexes = new Dictionary<string, string>
        {
            { "v_similar_schools_primary_groups",   "\"URN\"" },
            { "v_similar_schools_primary_values",   "\"URN\"" },
            { "v_similar_schools_secondary_groups", "\"URN\"" },
            { "v_similar_schools_secondary_values", "\"URN\"" },
        };

        foreach (var kvp in indexes)
        {
            string view = kvp.Key;
            string column = kvp.Value;
            string indexName = $"idx_{view}_{column.Trim('"').ToLower()}";

            sb.AppendLine($"CREATE INDEX IF NOT EXISTS {indexName}");
            sb.AppendLine($"ON public.{view} ({column});");
            sb.AppendLine();
        }

        WriteSql("60", "similar_schools_indexes", sb.ToString());
    }

    private void WriteSql(string prefix, string viewName, string sql)
    {
        var fileName = $"{prefix}_{viewName}.sql";

        File.WriteAllText(
            Path.Combine(_sqlDir, fileName),
            sql,
            new UTF8Encoding(false));
        _sqlFiles.Add($"{prefix}_{viewName}.sql");

        Console.WriteLine($"Generated view index script: {fileName}");
    }
}
