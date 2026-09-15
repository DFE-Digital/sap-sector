using SAPSec.Core;
using SAPSec.Core.Features.Pagination;
using SAPSec.Core.Features.SchoolInfo;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Features.Sorting;
using SAPSec.Web.Helpers;

namespace SAPSec.Web.Areas.Shared.ViewModels.SimilarSchools;

/// <summary>
/// Shape needed by the shared "View similar schools" views, implemented by both the
/// primary and secondary page ViewModels. All URLs are pre-built by the implementer
/// so the shared views never need to know which controller/area they're rendering for.
/// </summary>
public class SimilarSchoolsPageViewModel
{
    public const string NoResultsMessage = "There are no similar schools available for this school.";

    public required string Urn { get; init; }
    public required string SchoolName { get; init; }
    public required string PhaseLabel { get; init; }
    public required string ViewSimilarSchoolsUrl { get; init; }
    public required string WhatIsASimilarSchoolUrl { get; init; }

    public required IReadOnlyCollection<SimilarSchoolViewModel> SimilarSchools { get; init; }
    public required IReadOnlyCollection<SimilarSchoolViewModel> MapSchools { get; init; }
    public required List<SimilarSchoolsFilterGroupViewModel> FilterGroups { get; init; }
    public required List<SimilarSchoolsSelectedFilterTagViewModel> SelectedFilterTags { get; init; }
    public required IReadOnlyCollection<SortOption> SortOptions { get; init; }
    public required IReadOnlyCollection<ValidationError> ValidationErrors { get; init; }

    public required int CurrentPage { get; init; }
    public required int PageSize { get; init; } = 10;
    public required int TotalResults { get; init; }
    public required string SortBy { get; init; } = "";
    public required Dictionary<string, List<string>> CurrentFilters { get; init; } = new(StringComparer.InvariantCultureIgnoreCase);

    public int TotalPages => (int)Math.Ceiling((double)TotalResults / PageSize);
    public int ShowingFrom => TotalResults == 0 ? 0 : (CurrentPage - 1) * PageSize + 1;
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
        if (CurrentPage > 3) items.Add(SimilarSchoolsPagination.Ellipsis);

        var start = Math.Max(2, CurrentPage - 1);
        var end = Math.Min(TotalPages - 1, CurrentPage + 1);
        for (var i = start; i <= end; i++) items.Add(i);

        if (CurrentPage < TotalPages - 2) items.Add(SimilarSchoolsPagination.Ellipsis);
        items.Add(TotalPages);

        return items;
    }

    public static SimilarSchoolsPageViewModel Build(
        string phaseLabel,
        IQueryCollection query,
        SchoolInfo currentSchool,
        IReadOnlyCollection<SortOption> sortOptions,
        IReadOnlyCollection<SimilarSchoolsAvailableFilter> filterOptions,
        IPagedCollection<SimilarSchoolResult> resultsPage,
        IReadOnlyCollection<SimilarSchoolResult> allResults,
        IReadOnlyCollection<ValidationError> validationErrors,
        string viewSimilarSchoolsUrl,
        string whatIsASimilarSchoolUrl,
        Func<string, string> comparisonUrl)
    {
        var sortBy = sortOptions.FirstOrDefault(o => o.Selected)?.Key
            ?? sortOptions.First(o => o.Selected).Key;

        var filterBy = BuildCoreFilters(query);
        var currentFilters = ExtractCurrentFilters(query);

        var schools = resultsPage
            .Select(result => MapToViewModel(result, comparisonUrl))
            .ToList();

        var allSchools = allResults
            .Select(result => MapToViewModel(result, comparisonUrl))
            .ToList();

        return new()
        {
            PhaseLabel = phaseLabel,
            Urn = currentSchool.Urn,
            SchoolName = currentSchool.Name,
            SortOptions = sortOptions,
            SortBy = sortBy,
            FilterGroups = SimilarSchoolsViewModelHelpers.BuildFilterGroups(filterOptions),
            CurrentFilters = currentFilters,
            SelectedFilterTags = SimilarSchoolsViewModelHelpers.BuildSelectedFilterTags(
                filterOptions,
                currentFilters,
                sortBy,
                viewSimilarSchoolsUrl),
            SimilarSchools = schools,
            MapSchools = allSchools,
            ValidationErrors = validationErrors,
            CurrentPage = resultsPage.CurrentPage,
            PageSize = resultsPage.ItemsPerPage,
            TotalResults = allResults.Count,
            ViewSimilarSchoolsUrl = viewSimilarSchoolsUrl,
            WhatIsASimilarSchoolUrl = whatIsASimilarSchoolUrl
        };
    }

    public static Dictionary<string, IEnumerable<string>> BuildCoreFilters(IQueryCollection query)
    {
        return query
            .Where(kvp => kvp.Key != "sortBy" && kvp.Key != "page")
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Where(v => !string.IsNullOrWhiteSpace(v))!.Select(v => v!),
                StringComparer.InvariantCultureIgnoreCase);
    }

    private static Dictionary<string, List<string>> ExtractCurrentFilters(IQueryCollection query)
    {
        var result = new Dictionary<string, List<string>>(StringComparer.InvariantCultureIgnoreCase);
        foreach (var (key, values) in query)
        {
            if (key == "sortBy" || key == "page")
            {
                continue;
            }

            result[key] = values.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v!).ToList();
        }

        return result;
    }

    private static SimilarSchoolViewModel MapToViewModel(
        SimilarSchoolResult result,
        Func<string, string> comparisonUrl)
    {
        return new SimilarSchoolViewModel
        {
            Urn = result.URN,
            Name = result.Name,
            LocalAuthorityName = result.LocalAuthority.Name,
            FullAddress = result.Address.ToString(),
            Latitude = result.Coordinates?.Latitude.ToString(),
            Longitude = result.Coordinates?.Longitude.ToString(),
            SortMetricName = result.SortValue.Name,
            SortMetricDisplayValue = result.SortValue.Value.Display(),
            ComparisonUrl = comparisonUrl(result.URN)
        };
    }
}
