using System.Text;

namespace SAPData;

public class GenerateSimilarSchoolsIndexes
{
    private readonly string _sqlDir;

    public GenerateSimilarSchoolsIndexes(string sqlDir)
    {
        _sqlDir = sqlDir;
    }

    public void Run()
    {
        string outputPath = Path.Combine(_sqlDir, "51_similar_schools_indexes.sql");

        var sb = new StringBuilder();

        sb.AppendLine("-- ================================================================");
        sb.AppendLine("-- 51_similar_schools_indexes.sql");
        sb.AppendLine("-- Indexes for materialized views (AUTO-GENERATED)");
        sb.AppendLine("-- ================================================================");
        sb.AppendLine();
        sb.AppendLine("-- NOTE:");
        sb.AppendLine("-- - Uses quoted identifiers to respect case-sensitive columns");
        sb.AppendLine();

        // View → column mapping (EXACT column names)
        var indexes = new Dictionary<string, string>
        {
            { "v_similar_schools_primary_groups",   "\"urn\"" },
            { "v_similar_schools_primary_values",   "\"urn\"" },
            { "v_similar_schools_secondary_groups", "\"urn\"" },
            { "v_similar_schools_secondary_values", "\"urn\"" },
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

        File.WriteAllText(outputPath, sb.ToString(), new UTF8Encoding(false));

        Console.WriteLine("Generated view index script:");
        Console.WriteLine(outputPath);
    }
}
