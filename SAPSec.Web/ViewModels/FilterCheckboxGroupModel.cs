namespace SAPSec.Web.ViewModels;

public class FilterCheckboxGroupModel
{
    public string Legend { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public List<string> SelectedValues { get; set; } = new();
    public bool IsSingleSelect { get; set; }
}
