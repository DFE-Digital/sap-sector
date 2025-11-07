namespace SAPSec.Web.Domain;

public class HealthCheckResponse
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public List<HealthCheckItem> Checks { get; set; } = new();
}