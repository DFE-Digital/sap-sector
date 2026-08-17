using SAPSec.Core;
using SAPSec.Core.Features.SimilarSchools.UseCases;
using SAPSec.Core.Features.Sorting;
using SAPSec.Core.Model;
using SAPSec.Web.Constants;
using SAPSec.Web.ViewModels;
using System.Globalization;

namespace SAPSec.Web.Areas.Primary.ViewModels;

public class PrimarySimilarSchoolsPageViewModel : ISimilarSchoolsPageViewModel
{
    public const int PaginationEllipsis = SimilarSchoolsPagination.Ellipsis;

    public SchoolInfoViewModel CurrentSchool { get; init; } = null!;
    public string CurrentSchoolLocalAuthorityName { get; init; } = string.Empty;
    public IReadOnlyCollection<PrimarySimilarSchoolsRowViewModel> SimilarSchools { get; init; } = [];
    public IReadOnlyCollection<PrimarySimilarSchoolsRowViewModel> MapSchools { get; init; } = [];
    public int Urn { get; init; }
    public Dictionary<string, List<string>> CurrentFilters { get; init; } = new(StringComparer.InvariantCultureIgnoreCase);
    public List<SimilarSchoolsFilterGroupViewModel> FilterGroups { get; init; } = [];
    public List<SimilarSchoolsSelectedFilterTagViewModel> SelectedFilterTags { get; init; } = [];
    public IReadOnlyCollection<SortOption> SortOptions { get; init; } = [];
    public string SortBy { get; init; } = "RwmExpected";
    public IReadOnlyCollection<ValidationError> ValidationErrors { get; init; } = [];
    public int CurrentPage { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public int TotalResults { get; init; }

    public int TotalPages => (int)Math.Ceiling((double)TotalResults / PageSize);
    public int ShowingFrom => TotalResults == 0 ? 0 : ((CurrentPage - 1) * PageSize) + 1;
    public int ShowingTo => Math.Min(CurrentPage * PageSize, TotalResults);
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
    public bool HasActiveFilters => CurrentFilters.Any(kvp => kvp.Value.Any(v => !string.IsNullOrWhiteSpace(v)));

    string ISimilarSchoolsPageViewModel.Urn => CurrentSchool.Urn;
    string ISimilarSchoolsPageViewModel.SchoolName => CurrentSchool.Name;
    string ISimilarSchoolsPageViewModel.PhaseLabel => "primary";
    string ISimilarSchoolsPageViewModel.NoResultsMessage => "There are no similar schools available for this school.";
    string ISimilarSchoolsPageViewModel.FilterFormUrl => Routes.PrimarySchool(CurrentSchool.Urn).ViewSimilarSchools;
    string ISimilarSchoolsPageViewModel.ResultsBaseUrl => Routes.PrimarySchool(CurrentSchool.Urn).ViewSimilarSchools;
    string ISimilarSchoolsPageViewModel.WhatIsASimilarSchoolUrl => Routes.PrimarySchool(CurrentSchool.Urn).WhatIsASimilarSchool;
    IReadOnlyCollection<ISimilarSchoolRowViewModel> ISimilarSchoolsPageViewModel.SimilarSchools => SimilarSchools;
    IReadOnlyCollection<ISimilarSchoolRowViewModel> ISimilarSchoolsPageViewModel.MapSchools => MapSchools;

    public static PrimarySimilarSchoolsPageViewModel FromResponse(
        FindPrimarySimilarSchoolsResponse response,
        IQueryCollection query)
    {
        var currentFilters = ExtractCurrentFilters(query);
        var filterOptions = response.FilterOptions;

        return new PrimarySimilarSchoolsPageViewModel
        {
            CurrentSchool = new SchoolInfoViewModel(
                response.CurrentSchool.Urn,
                response.CurrentSchool.Name,
                string.Empty),
            CurrentSchoolLocalAuthorityName = response.CurrentSchool.LocalAuthorityName,
            SimilarSchools = MapRows(response.SimilarSchoolsPage, response.CurrentSchool.Urn),
            MapSchools = MapRows(response.AllSimilarSchools, response.CurrentSchool.Urn),
            Urn = int.TryParse(response.CurrentSchool.Urn, out var urn) ? urn : 0,
            CurrentFilters = currentFilters,
            FilterGroups = SimilarSchoolsViewModelHelpers.BuildFilterGroups(filterOptions),
            SelectedFilterTags = SimilarSchoolsViewModelHelpers.BuildSelectedFilterTags(
                filterOptions,
                currentFilters,
                response.SortOptions.FirstOrDefault(o => o.Selected)?.Key ?? "RwmExpected",
                Routes.PrimarySchool(response.CurrentSchool.Urn).ViewSimilarSchools),
            SortOptions = response.SortOptions,
            SortBy = response.SortOptions.FirstOrDefault(o => o.Selected)?.Key ?? "RwmExpected",
            ValidationErrors = response.ValidationErrors,
            CurrentPage = response.SimilarSchoolsPage.CurrentPage,
            PageSize = response.SimilarSchoolsPage.ItemsPerPage,
            TotalResults = response.AllSimilarSchools.Count
        };
    }

    public static Dictionary<string, List<string>> ExtractCurrentFilters(IQueryCollection query)
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

    private static IReadOnlyCollection<PrimarySimilarSchoolsRowViewModel> MapRows(
        IEnumerable<PrimarySimilarSchool> rows,
        string currentSchoolUrn) =>
        rows
            .Select(row => new PrimarySimilarSchoolsRowViewModel(
                row.SimilarSchool.URN,
                row.SimilarSchool.Name,
                row.SimilarSchool.LocalAuthority.Name,
                row.Rank,
                row.Distance,
                Routes.PrimarySchool(currentSchoolUrn).Comparison(row.SimilarSchool.URN).Overview,
                BuildFullAddress(row.SimilarSchool.Address.Street, row.SimilarSchool.Address.Town, row.SimilarSchool.Address.Postcode),
                row.Coordinates?.Latitude.ToString(CultureInfo.InvariantCulture),
                row.Coordinates?.Longitude.ToString(CultureInfo.InvariantCulture),
                row.SortValue.Name,
                DisplaySortValue(row.SortValue.Value)))
            .ToList()
            .AsReadOnly();

    private static string BuildFullAddress(
        string street,
        string town,
        string postcode)
    {
        var parts = new[] { street, town, postcode }
            .Where(part => !string.IsNullOrWhiteSpace(part));

        return string.Join(", ", parts);
    }

    private static string DisplaySortValue(DataWithAvailability<string> value) =>
        value.HasValue && value.Value is not null
            ? value.Value
            : "No data available";

}

public record PrimarySimilarSchoolsRowViewModel(
    string Urn,
    string Name,
    string LocalAuthorityName,
    string Rank,
    string Distance,
    string ComparisonUrl,
    string FullAddress,
    string? Latitude,
    string? Longitude,
    string SortMetricName,
    string SortMetricDisplayValue) : ISimilarSchoolRowViewModel;
