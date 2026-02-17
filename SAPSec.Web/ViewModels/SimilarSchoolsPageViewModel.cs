using SAPSec.Core.Model;

namespace SAPSec.Web.ViewModels;

/// <summary>
/// ViewModel for the View Similar Schools page.
/// Inherits SchoolDetails so the shared school layout can access Name, Urn etc.
/// </summary>
public class SimilarSchoolsPageViewModel
{
    public int Urn { get; set; }
    public string EstablishmentName { get; set; } = string.Empty;
    public string PhaseOfEducation { get; set; } = string.Empty;
    public List<SimilarSchoolViewModel> Schools { get; set; } = new();
    public List<SimilarSchoolViewModel> MapSchools { get; set; } = new();
    public IReadOnlyCollection<SAPSec.Core.Features.SimilarSchools.UseCases.SimilarSchoolsAvailableFilter> FilterOptions { get; set; } = [];
    public IReadOnlyCollection<SAPSec.Core.Features.Sorting.SortOption> SortOptions { get; set; } = [];
    public Dictionary<string, List<string>> CurrentFilters { get; set; } = new(StringComparer.InvariantCultureIgnoreCase);
    public string SortBy { get; set; } = "Att8";
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalResults { get; set; }

    public int TotalPages => (int)Math.Ceiling((double)TotalResults / PageSize);
    public int ShowingFrom => TotalResults == 0 ? 0 : ((CurrentPage - 1) * PageSize) + 1;
    public int ShowingTo => Math.Min(CurrentPage * PageSize, TotalResults);
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
}




















