namespace SAPSec.Web.Configuration;

public class CustomEventLocations
{
    public string FeedbackForm { get; set; } = string.Empty;
    public string SignIn { get; set; } = string.Empty;
    public string MailTo { get; set; } = string.Empty;
    public string[] ServiceUrls { get; set; } = [];
}
