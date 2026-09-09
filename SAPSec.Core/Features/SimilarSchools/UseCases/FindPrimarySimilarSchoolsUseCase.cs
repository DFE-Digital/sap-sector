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
    IKs2PerformanceRepository performanceRepository,
    IAbsenceRepository absenceRepository)
    : IUseCase<FindPrimarySimilarSchoolsRequest, FindPrimarySimilarSchoolsResponse>
{
    public async Task<FindPrimarySimilarSchoolsResponse> Execute(FindPrimarySimilarSchoolsRequest request)
    {
        var dataProvider = new PrimarySimilarSchoolsDataProvider(
            establishmentRepository,
            similarSchoolsRepository,
            performanceRepository,
            absenceRepository);

        var data = await dataProvider.GetData(request.Urn);
        var currentSchoolInfo = SchoolInfo.SchoolInfo.FromSimilarSchool(data.CurrentSimilarSchool);

        var filterBy = request.FilterBy.AsCaseInsensitive();
        var filters = new SimilarSchoolsFilters(filterBy, data.CurrentSimilarSchool);
        var validationErrors = filters.Validate();
        var filtered = filters.Filter(data.SimilarSchools, i => i.SimilarSchool);

        var sortBy = request.SortBy ?? string.Empty;
        var sorting = new PrimarySimilarSchoolsSorting(sortBy);
        var sorted = sorting.Sort(filtered);

        var allResults = sorted
            .Select(r => new SimilarSchoolResult(
                r.Item.URN,
                r.Item.Name,
                r.Item.Address,
                r.Item.LocalAuthority,
                r.Item.Coordinates != null ? CoordinateConverter.Convert(r.Item.Coordinates) : null,
                r.Value))
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