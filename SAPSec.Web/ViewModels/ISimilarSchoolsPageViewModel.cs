using SAPSec.Core;
using SAPSec.Core.Features.Sorting;

namespace SAPSec.Web.ViewModels;

/// <summary>
/// Shape needed by the shared "View similar schools" views, implemented by both the
/// primary and secondary page ViewModels. All URLs are pre-built by the implementer
/// so the shared views never need to know which controller/area they're rendering for.
/// </summary>
public class SimilarSchoolsPageViewModel
{
    public required string Urn { get; init; }
    public required string SchoolName { get; init; }
    public required string PhaseLabel { get; init; }
    public required string NoResultsMessage { get; init; }
    public required string FilterFormUrl { get; init; }
    public required string ResultsBaseUrl { get; init; }
    public required string WhatIsASimilarSchoolUrl { get; init; }

    public required IReadOnlyCollection<SimilarSchoolViewModel> SimilarSchools { get; init; }
    public required IReadOnlyCollection<SimilarSchoolViewModel> MapSchools { get; init; }
    public required List<SimilarSchoolsFilterGroupViewModel> FilterGroups { get; init; }
    public required List<SimilarSchoolsSelectedFilterTagViewModel> SelectedFilterTags { get; init; }
    public required IReadOnlyCollection<SortOption> SortOptions { get; init; }
    public required IReadOnlyCollection<ValidationError> ValidationErrors { get; init; }

    public int TotalPages => (int)Math.Ceiling((double)TotalResults / PageSize);
    public int ShowingFrom => TotalResults == 0 ? 0 : (CurrentPage - 1) * PageSize + 1;
    public int ShowingTo => Math.Min(CurrentPage * PageSize, TotalResults);
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
    public bool HasActiveFilters => SelectedFilterTags.Any();

    public required int CurrentPage { get; init; }
    public required int PageSize { get; init; } = 10;
    public required int TotalResults { get; init; }
    //public required int TotalPages { get; init; }
    //public required int ShowingFrom { get; init; }
    //public required int ShowingTo { get; init; }
    //public required bool HasPreviousPage { get; init; }
    //public required bool HasNextPage { get; init; }
    //public required bool HasActiveFilters { get; init; }
    public required string SortBy { get; init; } = "RwmExpected";
    public required Dictionary<string, List<string>> CurrentFilters { get; init; } = new(StringComparer.InvariantCultureIgnoreCase);

    public string BuildPaginationQueryString(int page)
    {
        var queryParts = new List<string> { $"page={page}" };

        if (!string.IsNullOrWhiteSpace(SortBy))
        {
            queryParts.Add($"sortBy={Uri.EscapeDataString(SortBy)}");
        }

        foreach (var (key, values) in CurrentFilters)
        {
            foreach (var value in values)
            {
                queryParts.Add($"{key}={Uri.EscapeDataString(value)}");
            }
        }

        return "?" + string.Join("&", queryParts);
    }

    public List<int> GetPaginationItems()
    {
        var items = new List<int>();

        if (TotalPages <= 7)
        {
            for (var i = 1; i <= TotalPages; i++) items.Add(i);
            return items;
        }

        items.Add(1);
        if (CurrentPage > 3) items.Add(SimilarSchoolsPagination.Ellipsis);

        var start = Math.Max(2, CurrentPage - 1);
        var end = Math.Min(TotalPages - 1, CurrentPage + 1);
        for (var i = start; i <= end; i++) items.Add(i);

        if (CurrentPage < TotalPages - 2) items.Add(SimilarSchoolsPagination.Ellipsis);
        items.Add(TotalPages);

        return items;
    }
}
