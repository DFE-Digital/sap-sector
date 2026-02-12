using SAPSec.Core.Features.Filtering;
using SAPSec.Core.Features.Pagination;
using SAPSec.Core.Features.Sorting;

namespace SAPSec.Web.ViewModels;

public record DemoSimilarSchoolsViewModel
{
    public required string Name { get; set; }
    public required IEnumerable<SortOption> SortOptions { get; set; }
    public required IReadOnlyDictionary<string, IReadOnlyCollection<FilterOption>> FilterOptions { get; set; }
    public required IPagedCollection<DemoSimilarSchoolResultViewModel> ResultsPage { get; set; }
    public required IEnumerable<DemoSimilarSchoolResultViewModel> AllResults { get; set; }
}
