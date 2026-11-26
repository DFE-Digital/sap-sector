namespace SAPSec.Web.Analytics;

public class CustomEventLocations
{
    public const string SectionName = "CustomEventLocations";

    public string FeedbackForm { get; set; } = string.Empty;
    public string SignIn { get; set; } = string.Empty;
    public string MailTo { get; set; } = "mailto:";
    public string[] ServiceUrls { get; set; } = [];
}
