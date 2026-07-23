using SAPSec.Core;
using SAPSec.Core.Features.Sorting;

namespace SAPSec.Web.ViewModels;

/// <summary>
/// Shape needed by the shared "View similar schools" views, implemented by both the
/// primary and secondary page ViewModels. All URLs are pre-built by the implementer
/// so the shared views never need to know which controller/area they're rendering for.
/// </summary>
public interface ISimilarSchoolsPageViewModel
{
    string Urn { get; }
    string SchoolName { get; }
    string PhaseLabel { get; }
    string NoResultsMessage { get; }
    string FilterFormUrl { get; }
    string ResultsBaseUrl { get; }
    string WhatIsASimilarSchoolUrl { get; }

    IReadOnlyCollection<ISimilarSchoolRowViewModel> SimilarSchools { get; }
    IReadOnlyCollection<ISimilarSchoolRowViewModel> MapSchools { get; }
    List<SimilarSchoolsFilterGroupViewModel> FilterGroups { get; }
    List<SimilarSchoolsSelectedFilterTagViewModel> SelectedFilterTags { get; }
    IReadOnlyCollection<SortOption> SortOptions { get; }
    IReadOnlyCollection<ValidationError> ValidationErrors { get; }

    int CurrentPage { get; }
    int TotalResults { get; }
    int TotalPages { get; }
    int ShowingFrom { get; }
    int ShowingTo { get; }
    bool HasPreviousPage { get; }
    bool HasNextPage { get; }
    bool HasActiveFilters { get; }

    List<int> GetPaginationItems();
    string BuildPaginationQueryString(int page);
}

public static class SimilarSchoolsPagination
{
    public const int Ellipsis = -1;
}
