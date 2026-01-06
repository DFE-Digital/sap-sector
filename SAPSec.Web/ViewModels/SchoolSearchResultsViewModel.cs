namespace SAPSec.Web.ViewModels;

public class SchoolSearchResultsViewModel : SchoolSearchQueryViewModel
{
    public string? URN { get; init; }

    public string? SchoolName { get; init; }

    public SchoolSearchResultViewModel[] Results { get; set; } = [];
    public string[] LocalAuthorities { get; set; } = [];
    public string[]? SelectedLocalAuthorities { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalResults { get; set; }

    public int TotalPages => (int)Math.Ceiling((double)TotalResults / PageSize);

    public PaginationViewModel Pagination => new PaginationViewModel
    {
        CurrentPage = CurrentPage,
        TotalPages = TotalPages,
        TotalResults = TotalResults,
        PageSize = PageSize,
        Query = Query,
        LocalAuthorities = SelectedLocalAuthorities
    };
}