// ReSharper disable MemberCanBePrivate.Global

namespace SAPSec.Web.ViewModels;

public class SchoolSearchResultsViewModel : SchoolSearchQueryViewModel
{
    public string? URN { get; init; }

    public string? SchoolName { get; init; }

    public SchoolSearchResultViewModel[] Results { get; set; } = [];
    public string[] LocalAuthorities { get; set; } = [];
    public string[]? SelectedLocalAuthorities { get; set; }
}