using SAPSec.Core.Extensions;
using SAPSec.Core.Features.Filtering;
using SAPSec.Core.Features.Geography;
using SAPSec.Core.Features.Pagination;
using SAPSec.Core.Features.SimilarSchools.Filtering;
using SAPSec.Core.Features.SimilarSchools.Sorting;
using SAPSec.Core.Features.Sorting;
using SAPSec.Core.Model;
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

        var filters = new SimilarSchoolsFilters(
             request.FilterBy.AsCaseInsensitive(),
            data.CurrentSimilarSchool);

        var validationErrors = filters.Validate();

        var filteredSimilarSchools = filters.Filter(data.SimilarSchools.Select(x => x.SimilarSchool))
            .Select(school => data.SimilarSchools.First(x => x.SimilarSchool.URN == school.URN))
            .ToList();
        var sorting = new PrimarySimilarSchoolsSorting(request.SortBy ?? string.Empty);
        var sortedSimilarSchools = sorting.Sort(filteredSimilarSchools)
            .Select(sortedItem => sortedItem.Item with { SortValue = sortedItem.Value })
            .ToList();

        var page = int.TryParse(request.Page, out var parsedPage) ? parsedPage : 1;
        var pagedSimilarSchools = new PagedCollection<PrimaryRankedSimilarSchoolData>(
            sortedSimilarSchools,
            page,
            request.ResultsPerPage);

        return new(
            new PrimaryCurrentSchool(
                data.CurrentEstablishment.URN,
                data.CurrentEstablishment.EstablishmentName,
                data.CurrentEstablishment.LAName),
            pagedSimilarSchools.Map(ToSimilarSchool),
            sortedSimilarSchools.Select(ToSimilarSchool).ToList().AsReadOnly(),
            filters.AsAvailableFilters(data.SimilarSchools.Select(x => x.SimilarSchool)),
            sorting.GetPossibleOptions(request.SortBy).ToList().AsReadOnly(),
            validationErrors);
    }

    private static PrimarySimilarSchool ToSimilarSchool(PrimaryRankedSimilarSchoolData school) =>
        new(
            school.SimilarSchool,
            school.SimilarSchool.Coordinates is not null
                ? CoordinateConverter.Convert(school.SimilarSchool.Coordinates)
                : null,
            school.Rank,
            school.Distance,
            school.SortValue ?? new SortOptionValue<DataWithAvailability<string>>(
                string.Empty,
                string.Empty,
                DataWithAvailability.NotAvailable<string>()));
}

public record FindPrimarySimilarSchoolsRequest(
    string Urn,
    IDictionary<string, IEnumerable<string>>? FilterBy = null,
    string? SortBy = null,
    string? Page = null,
    int ResultsPerPage = 10);

public record FindPrimarySimilarSchoolsResponse(
    PrimaryCurrentSchool CurrentSchool,
    IPagedCollection<PrimarySimilarSchool> SimilarSchoolsPage,
    IReadOnlyCollection<PrimarySimilarSchool> AllSimilarSchools,
    IReadOnlyCollection<SimilarSchoolsAvailableFilter> FilterOptions,
    IReadOnlyCollection<SortOption> SortOptions,
    IReadOnlyCollection<ValidationError> ValidationErrors);

public record PrimaryCurrentSchool(
    string Urn,
    string Name,
    string LocalAuthorityName);

public record PrimarySimilarSchool(
    SimilarSchool SimilarSchool,
    GeographicCoordinates? Coordinates,
    string Rank,
    string Distance,
    SortOptionValue<DataWithAvailability<string>> SortValue);

internal record PrimaryRankedSimilarSchoolData(
    string Rank,
    string Distance,
    SimilarSchool SimilarSchool,
    Ks2PerformanceData? PerformanceData,
    SortOptionValue<DataWithAvailability<string>>? SortValue = null);
