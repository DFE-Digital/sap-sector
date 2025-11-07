// ReSharper disable MemberCanBePrivate.Global

namespace SAPSec.Web.ViewModels;

public class SchoolSearchResultsViewModel : SchoolSearchQueryViewModel
{
    public string? URN { get; init; }

    public string? SchoolName { get; init; }

    public SchoolSearchResultViewModel[] Results { get; set; } = [];
}