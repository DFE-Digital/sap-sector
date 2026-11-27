namespace SAPSec.Infrastructure.Redis;

public class RedisSettings
{
    public const string SectionName = "Redis";

    public string ConnectionString { get; set; } = "";
}
