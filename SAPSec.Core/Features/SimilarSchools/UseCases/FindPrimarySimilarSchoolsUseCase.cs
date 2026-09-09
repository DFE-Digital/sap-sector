using SAPSec.Core.Extensions;
using SAPSec.Core.Features.Geography;
using SAPSec.Core.Features.Pagination;
using SAPSec.Core.Features.SimilarSchools.Filtering;
using SAPSec.Core.Features.SimilarSchools.Sorting;
using SAPSec.Core.Features.Sorting;
using SAPSec.Core.UseCases;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.SimilarSchools.UseCases;

public class FindPrimarySimilarSchoolsUseCase(
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsPrimaryRepository similarSchoolsRepository,
    IAbsenceRepository absenceRepository,
    IKs2PerformanceRepository performanceRepository)
    : IUseCase<FindPrimarySimilarSchoolsRequest, FindPrimarySimilarSchoolsResponse>
{
    public async Task<FindPrimarySimilarSchoolsResponse> Execute(FindPrimarySimilarSchoolsRequest request)
    {
        var dataProvider = new PrimarySimilarSchoolsDataProvider(
            establishmentRepository,
            similarSchoolsRepository,
            absenceRepository,
            performanceRepository);

        var data = await dataProvider.GetSimilarSchoolsData(request.Urn);
        var currentSchoolInfo = SchoolInfo.SchoolInfo.FromSimilarSchool(data.CurrentSimilarSchool);

        var filterBy = request.FilterBy.AsCaseInsensitive();
        var filters = new SimilarSchoolsFilters(filterBy, data.CurrentSimilarSchool);
        var validationErrors = filters.Validate();
        var filtered = filters.Filter(data.SimilarSchools, i => i.SimilarSchool);

        var sortBy = request.SortBy ?? string.Empty;
        var sorting = new PrimarySimilarSchoolsSorting(sortBy);
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
            filters.AsAvailableFilters(data.SimilarSchools, i => i.SimilarSchool),
            resultsPage,
            allResults,
            validationErrors);
    }
}

public record FindPrimarySimilarSchoolsRequest(
    string Urn,
    IDictionary<string, IEnumerable<string>>? FilterBy = null,
    string? SortBy = null,
    string? Page = null,
    int ResultsPerPage = 10);

public record FindPrimarySimilarSchoolsResponse(
    SchoolInfo.SchoolInfo CurrentSchool,
    IReadOnlyCollection<SortOption> SortOptions,
    IReadOnlyCollection<SimilarSchoolsAvailableFilter> FilterOptions,
    IPagedCollection<SimilarSchoolResult> ResultsPage,
    IReadOnlyCollection<SimilarSchoolResult> AllResults,
    IReadOnlyCollection<ValidationError> ValidationErrors);