using SAPSec.Core.Model;

namespace SAPSec.Web.ViewModels;

/// <summary>
/// ViewModel for the View Similar Schools page.
/// Inherits SchoolDetails so the shared school layout can access Name, Urn etc.
/// </summary>
public class SimilarSchoolsPageViewModel
{
    public const int PaginationEllipsis = -1;

    public int Urn { get; set; }
    public string EstablishmentName { get; set; } = string.Empty;
    public string PhaseOfEducation { get; set; } = string.Empty;
    public List<SimilarSchoolViewModel> Schools { get; set; } = new();
    public List<SimilarSchoolViewModel> MapSchools { get; set; } = new();
    public IReadOnlyCollection<SAPSec.Core.Features.SimilarSchools.UseCases.SimilarSchoolsAvailableFilter> FilterOptions { get; set; } = [];
    public IReadOnlyCollection<SAPSec.Core.Features.Sorting.SortOption> SortOptions { get; set; } = [];
    public Dictionary<string, List<string>> CurrentFilters { get; set; } = new(StringComparer.InvariantCultureIgnoreCase);
    public List<SimilarSchoolsFilterGroupViewModel> FilterGroups { get; set; } = new();
    public List<SimilarSchoolsSelectedFilterTagViewModel> SelectedFilterTags { get; set; } = new();
    public string SortBy { get; set; } = "Att8";
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalResults { get; set; }

    public int TotalPages => (int)Math.Ceiling((double)TotalResults / PageSize);
    public int ShowingFrom => TotalResults == 0 ? 0 : ((CurrentPage - 1) * PageSize) + 1;
    public int ShowingTo => Math.Min(CurrentPage * PageSize, TotalResults);
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
    public bool HasActiveFilters => SelectedFilterTags.Any();

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
        if (CurrentPage > 3) items.Add(PaginationEllipsis);

        var start = Math.Max(2, CurrentPage - 1);
        var end = Math.Min(TotalPages - 1, CurrentPage + 1);
        for (var i = start; i <= end; i++) items.Add(i);

        if (CurrentPage < TotalPages - 2) items.Add(PaginationEllipsis);
        items.Add(TotalPages);

        return items;
    }
}

public record SimilarSchoolsFilterGroupViewModel(
    string Heading,
    List<SAPSec.Core.Features.SimilarSchools.UseCases.SimilarSchoolsAvailableFilter> Filters);

public record SimilarSchoolsSelectedFilterTagViewModel(
    string Label,
    string RemoveUrl);




















