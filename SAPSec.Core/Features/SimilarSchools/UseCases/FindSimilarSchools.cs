using SAPSec.Core.Features.Attendance;
using SAPSec.Core.Features.Filtering;
using SAPSec.Core.Features.Geography;
using SAPSec.Core.Features.Ks4HeadlineMeasures;
using SAPSec.Core.Features.Pagination;
using SAPSec.Core.Features.SimilarSchools.Filtering;
using SAPSec.Core.Features.SimilarSchools.Sorting;
using SAPSec.Core.Features.Sorting;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Model;
using System.Text.RegularExpressions;

namespace SAPSec.Core.Features.SimilarSchools.UseCases;

public class FindSimilarSchools(
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsSecondaryRepository similarSchoolsRepository,
    IKs4PerformanceRepository performanceRepository,
    IAbsenceRepository absenceRepository)
{
    private static readonly Regex UrnRegex = new Regex(@"^\d{6}$", RegexOptions.Compiled);

    public async Task<FindSimilarSchoolsResponse> Execute(FindSimilarSchoolsRequest request)
    {
        if (!UrnRegex.IsMatch(request.CurrentSchoolUrn))
        {
            var ex = new ValidationException();
            ex.AddError("CurrentSchoolUrn", "Current School URN should be a valid URN");
            throw ex;
        }

        var groups = await similarSchoolsRepository.GetSimilarSchoolsGroupAsync(request.CurrentSchoolUrn);
        var urns = groups.Select(g => g.NeighbourURN).Concat([request.CurrentSchoolUrn]);

        var establishments = await establishmentRepository.GetEstablishmentsAsync(urns);
        var performance = await performanceRepository.GetByUrnsAsync(urns);
        var absence = await absenceRepository.GetByUrnsAsync(urns);

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
            currentSchool.Name,
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
    string SchoolName,
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
