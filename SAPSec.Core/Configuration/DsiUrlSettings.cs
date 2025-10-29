namespace SAPSec.Core.Configuration;

public class DsiUrlSettings
{
    public const string SectionName = "DsiUrlSettings";

    public string ServicesUrl { get; set; } = "https://services.signin.education.gov.uk/";

    public string ManageConsoleUrl { get; set; } = "https://services.signin.education.gov.uk/";

    public string HelpUrl { get; set; } = "https://help.signin.education.gov.uk/";

    public string RegisterUrl { get; set; } = "https://profile.signin.education.gov.uk/register";
}