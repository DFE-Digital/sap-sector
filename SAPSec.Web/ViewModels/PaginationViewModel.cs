using SAPSec.Core.Model;

namespace SAPSec.Web.ViewModels;
public class PaginationViewModel
{
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalResults { get; set; }
    public int PageSize { get; set; } = 10;
    public string? Query { get; set; }
    public string[]? LocalAuthorities { get; set; }
    public bool SecondaryOnly { get; set; } = true;
    public bool SimilarSchoolsOnly { get; set; } = true;

    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;

    public int StartItem => TotalResults == 0 ? 0 : ((CurrentPage - 1) * PageSize) + 1;
    public int EndItem => Math.Min(CurrentPage * PageSize, TotalResults);

    public IEnumerable<PaginationItem> GetPageItems()
    {
        var items = new List<PaginationItem>();

        if (TotalPages <= 1)
            return items;

        items.Add(new PaginationItem { Number = 1, IsCurrent = CurrentPage == 1 });

        if (CurrentPage > 3)
        {
            items.Add(new PaginationItem { IsEllipsis = true });
        }

        var startPage = Math.Max(2, CurrentPage - 1);
        var endPage = Math.Min(TotalPages - 1, CurrentPage + 1);

        if (CurrentPage <= 3)
        {
            startPage = 2;
            endPage = Math.Min(4, TotalPages - 1);
        }

        if (CurrentPage >= TotalPages - 2)
        {
            startPage = Math.Max(2, TotalPages - 3);
            endPage = TotalPages - 1;
        }

        for (var i = startPage; i <= endPage; i++)
        {
            items.Add(new PaginationItem { Number = i, IsCurrent = CurrentPage == i });
        }
        if (CurrentPage < TotalPages - 2)
        {
            items.Add(new PaginationItem { IsEllipsis = true });
        }
      
        if (TotalPages > 1)
        {
            items.Add(new PaginationItem { Number = TotalPages, IsCurrent = CurrentPage == TotalPages });
        }

        return items;
    }
}
