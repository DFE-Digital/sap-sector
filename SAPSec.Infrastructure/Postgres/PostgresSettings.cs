namespace SAPSec.Infrastructure.Postgres;

public class PostgresSettings
{
    public const string SectionName = "Postgres";

    public string ConnectionString { get; set; } = "";
    public int? MaxPoolSize { get; set; }
    public int? ConnectionTimeoutSeconds { get; set; }
}