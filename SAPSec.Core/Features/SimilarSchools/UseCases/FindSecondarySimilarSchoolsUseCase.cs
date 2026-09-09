using SAPSec.Core.Extensions;
using SAPSec.Core.Features.Geography;
using SAPSec.Core.Features.Pagination;
using SAPSec.Core.Features.SimilarSchools.Filtering;
using SAPSec.Core.Features.SimilarSchools.Sorting;
using SAPSec.Core.Features.Sorting;
using SAPSec.Core.UseCases;
using SAPSec.Data.Dto.KS4.Performance;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.SimilarSchools.UseCases;

public class FindSecondarySimilarSchoolsUseCase(
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsSecondaryRepository similarSchoolsRepository,
    IKs4PerformanceRepository performanceRepository,
    IAbsenceRepository absenceRepository)
    : IUseCase<FindSecondarySimilarSchoolsRequest, FindSecondarySimilarSchoolsResponse>
{
    public async Task<FindSecondarySimilarSchoolsResponse> Execute(FindSecondarySimilarSchoolsRequest request)
    {
        // TODO: Validate request

        var groups = await similarSchoolsRepository.GetGroupAsync(request.CurrentSchoolUrn);
        var urns = groups.Select(g => g.NeighbourURN).Concat([request.CurrentSchoolUrn]);

        var establishments = await establishmentRepository.GetEstablishmentsAsync(urns);
        var performance = await performanceRepository.GetByUrnsAsync(urns);
        var absence = await absenceRepository.GetByUrnsAsync(urns);

        var schools =
            from e in establishments
            join p in performance on e.URN equals p.Urn into perf
            join a in absence on e.URN equals a.Urn into abs
            select new SimilarSchoolSortItem<EstablishmentPerformance>(
                SimilarSchool.FromData(e, abs.FirstOrDefault()?.EstablishmentAbsence),
                perf.FirstOrDefault()?.EstablishmentPerformance);

        var currentSchool = schools.FirstOrDefault(s => s.SimilarSchool.URN == request.CurrentSchoolUrn);
        if (currentSchool is null)
        {
            throw new NotFoundException($"School with URN {request.CurrentSchoolUrn} was not found");
        }

        var currentSchoolInfo = SchoolInfo.SchoolInfo.FromSimilarSchool(currentSchool.SimilarSchool);

        var similarSchools = schools.Except([currentSchool]);

        var filterBy = request.FilterBy.AsCaseInsensitive();
        var filters = new SimilarSchoolsFilters(filterBy, currentSchool.SimilarSchool);
        var validationErrors = filters.Validate();
        var filtered = filters.Filter(similarSchools, i => i.SimilarSchool);

        var sortBy = request.SortBy ?? string.Empty;
        var sorting = new SecondarySimilarSchoolsSorting(sortBy);
        var sorted = sorting.Sort(filtered);

        var allResults = sorted
            .Select(sortedItem =>
            {
                return new SimilarSchoolResult
                (
                    sortedItem.Item.URN,
                    sortedItem.Item.Name,
                    sortedItem.Item.Address,
                    sortedItem.Item.LocalAuthority,
                    sortedItem.Item.Coordinates != null ? CoordinateConverter.Convert(sortedItem.Item.Coordinates) : null,
                    sortedItem.Value
                );
            })
            .ToList()
            .AsReadOnly();

        var page = int.TryParse(request.Page, out int parsed) ? parsed : 1;
        var resultsPage = new PagedCollection<SimilarSchoolResult>(allResults, page, request.ResultsPerPage);

        return new(
            currentSchoolInfo,
            sorting.GetPossibleOptions(sortBy).ToList().AsReadOnly(),
            filters.AsAvailableFilters(similarSchools, i => i.SimilarSchool),
            resultsPage,
            allResults,
            validationErrors
        );
    }
}

public record FindSecondarySimilarSchoolsRequest(
    string CurrentSchoolUrn,
    IDictionary<string, IEnumerable<string>>? FilterBy = null,
    string? SortBy = null,
    string? Page = null,
    int ResultsPerPage = 10);

public record FindSecondarySimilarSchoolsResponse(
    SchoolInfo.SchoolInfo CurrentSchool,
    IReadOnlyCollection<SortOption> SortOptions,
    IReadOnlyCollection<SimilarSchoolsAvailableFilter> FilterOptions,
    IPagedCollection<SimilarSchoolResult> ResultsPage,
    IReadOnlyCollection<SimilarSchoolResult> AllResults,
    IReadOnlyCollection<ValidationError> ValidationErrors);
