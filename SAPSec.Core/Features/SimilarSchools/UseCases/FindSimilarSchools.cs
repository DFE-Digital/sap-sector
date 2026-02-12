using SAPSec.Core.Features.Filtering;
using SAPSec.Core.Features.Geography;
using SAPSec.Core.Features.Pagination;
using SAPSec.Core.Features.SimilarSchools.Filtering;
using SAPSec.Core.Features.SimilarSchools.Sorting;
using SAPSec.Core.Features.Sorting;
using SAPSec.Core.Model;

namespace SAPSec.Core.Features.SimilarSchools.UseCases;

public class FindSimilarSchools(ISimilarSchoolsSecondaryRepository repository)
{
    public const int ItemsPerPage = 10;

    public async Task<FindSimilarSchoolsResponse> Execute(FindSimilarSchoolsRequest request)
    {
        // TODO: Validate inputs

        var (currentSchool, similarSchools) = await repository.GetSimilarSchoolsGroupAsync(request.CurrentSchoolUrn);

        var filters = new SimilarSchoolsFilters(request.FilterBy, currentSchool);
        var sorting = new SimilarSchoolsSorting(request.SortBy);

        var allResults = sorting.Sort(filters.Filter(similarSchools))
            .Select(sortedItem =>
            {
                return new SimilarSchoolResult
                (
                    sortedItem.Item,
                    sortedItem.Item.Coordinates != null ? CoordinateConverter.Convert(sortedItem.Item.Coordinates) : null,
                    sortedItem.Value
                );
            })
            .ToList()
            .AsReadOnly();

        return new(
            currentSchool.Name,
            sorting.GetPossibleOptions(request.SortBy).ToList().AsReadOnly(),
            filters.AsAvailableFilters(similarSchools),
            new PagedCollection<SimilarSchoolResult>(allResults, request.Page, ItemsPerPage),
            allResults
        );
    }
}

public record FindSimilarSchoolsRequest(
    string CurrentSchoolUrn,
    IDictionary<string, IEnumerable<string>> FilterBy,
    string SortBy,
    int Page);

public record FindSimilarSchoolsResponse(
    string SchoolName,
    IReadOnlyCollection<SortOption> SortOptions,
    IReadOnlyCollection<SimilarSchoolsAvailableFilter> FilterOptions,
    IPagedCollection<SimilarSchoolResult> ResultsPage,
    IReadOnlyCollection<SimilarSchoolResult> AllResults);

public record SimilarSchoolsAvailableFilter(
    string Key,
    string Name,
    FilterType Type,
    IReadOnlyCollection<FilterOption> Options,
    string? CurrentSchoolValue);

public record SimilarSchoolResult(
    SimilarSchool SimilarSchool,
    GeographicCoordinates? Coordinates,
    SortOptionValue<DataWithAvailability<decimal>> SortValue);