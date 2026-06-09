using SAPSec.Core.DataPoints;
using SAPSec.Core.Filtering;
using SAPSec.Core.Geography;
using SAPSec.Core.Pagination;
using SAPSec.Core.SchoolDetails;
using SAPSec.Core.SimilarSchools.Filtering;
using SAPSec.Core.SimilarSchools.Sorting;
using SAPSec.Core.Sorting;
using SAPSec.Core.UseCases;
using SAPSec.Core.Validation;
using SAPSec.Data.Store;

namespace SAPSec.Core.SimilarSchools;

public class FindSimilarSchoolsUseCase(
    IEstablishmentStore establishmentStore,
    ISimilarSchoolsSecondaryStore similarSchoolsStore,
    IKs4PerformanceStore performanceStore,
    IAbsenceStore absenceStore)
    : IUseCase<FindSimilarSchoolsRequest, FindSimilarSchoolsResponse>
{
    public async Task<FindSimilarSchoolsResponse> Execute(FindSimilarSchoolsRequest request)
    {
        // TODO: Validate request

        var groups = await similarSchoolsStore.GetSimilarSchoolsGroupAsync(request.CurrentSchoolUrn);
        var urns = groups.Select(g => g.NeighbourURN).Concat([request.CurrentSchoolUrn]);

        var establishments = await establishmentStore.GetEstablishmentsAsync(urns);
        var performance = await performanceStore.GetByUrnsAsync(urns);
        var absence = await absenceStore.GetByUrnsAsync(urns);

        var schools =
            from e in establishments
            join p in performance on e.URN equals p.URN into perf
            join a in absence on e.URN equals a.URN into abs
            select SimilarSchool.FromData(e, perf.FirstOrDefault()?.EstablishmentPerformance, abs.FirstOrDefault()?.EstablishmentAbsence);

        var currentSchool = schools.FirstOrDefault(s => s.URN == request.CurrentSchoolUrn);
        if (currentSchool is null)
        {
            throw new NotFoundException($"School with URN {request.CurrentSchoolUrn} was not found");
        }

        var similarSchools = schools.Except([currentSchool]);

        var filterBy = request.FilterBy ?? new Dictionary<string, IEnumerable<string>>();
        var filters = new SimilarSchoolsFilters(filterBy, currentSchool);

        var errors = filters.Validate();

        var sortBy = request.SortBy ?? "";
        var sorting = new SimilarSchoolsSorting(sortBy);

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

        var page = int.TryParse(request.Page, out int parsed) ? parsed : 1;
        var resultsPage = new PagedCollection<SimilarSchoolResult>(allResults, page, request.ResultsPerPage);

        return new(
            new(currentSchool.URN, currentSchool.Name),
            sorting.GetPossibleOptions(sortBy).ToList().AsReadOnly(),
            filters.AsAvailableFilters(similarSchools),
            resultsPage,
            allResults,
            errors
        );
    }
}

public record FindSimilarSchoolsRequest(
    string CurrentSchoolUrn,
    IDictionary<string, IEnumerable<string>>? FilterBy = null,
    string? SortBy = null,
    string? Page = null,
    int ResultsPerPage = 10);

public record FindSimilarSchoolsResponse(
    SchoolInfo School,
    IReadOnlyCollection<SortOption> SortOptions,
    IReadOnlyCollection<SimilarSchoolsAvailableFilter> FilterOptions,
    IPagedCollection<SimilarSchoolResult> ResultsPage,
    IReadOnlyCollection<SimilarSchoolResult> AllResults,
    IReadOnlyCollection<ValidationError> ValidationErrors);

public abstract record SimilarSchoolsAvailableFilter(
    string Key,
    string Name,
    DataWithAvailability<string>? CurrentSchoolValue);

public record SimilarSchoolsSingleValueAvailableFilter(
    string Key,
    string Name,
    IReadOnlyCollection<FilterOption> Options,
    DataWithAvailability<string>? CurrentSchoolValue)
    : SimilarSchoolsAvailableFilter(Key, Name, CurrentSchoolValue);

public record SimilarSchoolsMultiValueAvailableFilter(
    string Key,
    string Name,
    IReadOnlyCollection<FilterOption> Options,
    DataWithAvailability<string>? CurrentSchoolValue)
    : SimilarSchoolsAvailableFilter(Key, Name, CurrentSchoolValue);

public record SimilarSchoolsNumericRangeAvailableFilter(
    string Key,
    string Name,
    SimilarSchoolsNumericRangeAvailableFilterField From,
    SimilarSchoolsNumericRangeAvailableFilterField To,
    DataWithAvailability<string>? CurrentSchoolValue,
    IReadOnlyCollection<ValidationError> ValidationErrors)
    : SimilarSchoolsAvailableFilter(Key, Name, CurrentSchoolValue);

public record SimilarSchoolsNumericRangeAvailableFilterField(string Key, string Value);

public record SimilarSchoolResult(
    SimilarSchool SimilarSchool,
    GeographicCoordinates? Coordinates,
    SortOptionValue<DataWithAvailability<string>> SortValue);
