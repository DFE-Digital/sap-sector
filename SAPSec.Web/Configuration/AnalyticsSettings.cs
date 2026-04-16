namespace SAPSec.Web.Configuration;

public class AnalyticsSettings
{
    public string? GoogleMeasurementId { get; set; }
    public Dictionary<string, string>? GoogleMeasurementIds { get; set; }
    public string? GoogleTagManagerId { get; set; }
    public string? GoogleTagManagerAuth { get; set; }
    public string? GoogleTagManagerPreview { get; set; }
    public string? ClarityId { get; set; }
    public Dictionary<string, string>? ClarityIds { get; set; }
}
