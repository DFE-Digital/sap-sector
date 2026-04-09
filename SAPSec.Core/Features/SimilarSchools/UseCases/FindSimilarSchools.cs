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
    IKs4PerformanceRepository performanceRepository)
{
    private static readonly Regex UrnRegex = new Regex(@"^\d{6}$", RegexOptions.Compiled);
    public const int ItemsPerPage = 10;

    public async Task<FindSimilarSchoolsResponse> Execute(FindSimilarSchoolsRequest request)
    {
        if (!UrnRegex.IsMatch(request.CurrentSchoolUrn))
        {
            var ex = new ValidationException();
            ex.AddError("Current School URN should be a valid URN");
            throw ex;
        }

        var groups = await similarSchoolsRepository.GetSimilarSchoolsGroupAsync(request.CurrentSchoolUrn);
        var urns = groups.Select(g => g.NeighbourURN).Concat([request.CurrentSchoolUrn]);
        var establishments = await establishmentRepository.GetEstablishmentsAsync(urns);
        var performances = await performanceRepository.GetByUrnsAsync(urns);
        var schools = establishments.GroupJoin(performances, e => e.URN, p => p.Urn, SimilarSchool.FromData).ToList();
        var currentSchool = schools.FirstOrDefault(s => s.URN == request.CurrentSchoolUrn);
        if (currentSchool is null)
        {
            throw new NotFoundException($"School with URN {request.CurrentSchoolUrn} was not found");
        }

        var similarSchools = schools.Except([currentSchool]);

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
    DataWithAvailability<string> CurrentSchoolValue);

public record SimilarSchoolResult(
    SimilarSchool SimilarSchool,
    GeographicCoordinates? Coordinates,
    SortOptionValue<DataWithAvailability<string>> SortValue);
