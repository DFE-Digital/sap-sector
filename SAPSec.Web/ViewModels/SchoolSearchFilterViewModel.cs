namespace SAPSec.Web.ViewModels;

public class SchoolSearchFilterViewModel
{
    public string Query { get; set; } = string.Empty;
    public string[] LocalAuthorities { get; set; } = [];
    public string[]? SelectedLocalAuthorities { get; set; }
}
