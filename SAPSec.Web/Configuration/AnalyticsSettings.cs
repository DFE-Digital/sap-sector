namespace SAPSec.Web.Configuration;

public class AnalyticsSettings
{
    public string? GoogleTagManagerId { get; set; }
    public Dictionary<string, string>? GoogleTagManagerIds { get; set; }
    public Dictionary<string, string>? GoogleTagManagerAdditionals { get; set; }
    public string? GoogleTagManagerAdditional { get; set; }
    public string? ClarityId { get; set; }
    public Dictionary<string, string>? ClarityIds { get; set; }
}
