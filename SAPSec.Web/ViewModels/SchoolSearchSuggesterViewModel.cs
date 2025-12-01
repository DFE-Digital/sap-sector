namespace SAPSec.Web.ViewModels;

public record SchoolSearchSuggesterViewModel
{
    public string? InputElementId { get; init; }
    public string? TargetElementId { get; init; }
    public string? IdFieldName { get; init; }
}