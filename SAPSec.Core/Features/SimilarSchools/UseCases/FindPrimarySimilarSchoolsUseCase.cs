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
            request.FilterBy ?? new Dictionary<string, IEnumerable<string>>(),
            data.CurrentSimilarSchool);

        var validationErrors = filters.Validate();

        var filteredSimilarSchools = filters.Filter(data.SimilarSchools.Select(x => x.SimilarSchool))
            .Select(school => data.SimilarSchools.First(x => x.SimilarSchool.URN == school.URN))
            .ToList();
        var sorting = new PrimarySimilarSchoolsSorting(request.SortBy ?? string.Empty);
        var sortedSimilarSchools = sorting.Sort(filteredSimilarSchools)
            .Select(sortedItem => sortedItem.Item with
            {
                SortMetricName = sortedItem.Value.Name,
                SortMetricDisplayValue = sortedItem.Value.Value
            })
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
                data.CurrentEstablishment.LAName,
                ToCharacteristics(data.CurrentCharacteristics)),
            pagedSimilarSchools.Map(ToSimilarSchool),
            sortedSimilarSchools.Select(ToSimilarSchool).ToList().AsReadOnly(),
            filters.AsAvailableFilters(data.SimilarSchools.Select(x => x.SimilarSchool)),
            sorting.GetPossibleOptions(request.SortBy).ToList().AsReadOnly(),
            validationErrors);
    }

    private static PrimarySimilarSchool ToSimilarSchool(PrimaryRankedSimilarSchoolData school) =>
        ToSimilarSchool(
            school,
            school.SimilarSchool.Coordinates is not null
                ? CoordinateConverter.Convert(school.SimilarSchool.Coordinates)
                : null);

    private static PrimarySimilarSchool ToSimilarSchool(
        PrimaryRankedSimilarSchoolData school,
        GeographicCoordinates? coordinates) =>
        new(
            school.SimilarSchool.URN,
            school.SimilarSchool.Name,
            school.SimilarSchool.LocalAuthority.Name,
            school.Rank,
            school.Distance,
            school.SimilarSchool.Address.Street ?? string.Empty,
            school.SimilarSchool.Address.Locality ?? string.Empty,
            school.SimilarSchool.Address.Address3 ?? string.Empty,
            school.SimilarSchool.Address.Town ?? string.Empty,
            school.SimilarSchool.Address.Postcode ?? string.Empty,
            coordinates?.Latitude,
            coordinates?.Longitude,
            school.SortMetricName,
            school.SortMetricDisplayValue ?? DataWithAvailability.NotAvailable<string>(),
            ToCharacteristics(school.Characteristics));

    private static PrimarySimilarSchoolCharacteristics ToCharacteristics(SimilarSchoolsPrimaryValues values) =>
        new(
            values.ReadMatAverage,
            values.Ks1PriorRwmAverage,
            values.PupilPremiumEligibilityPercentage,
            values.PupilsWithEalPercentage,
            values.Polar4Quintile,
            values.PupilStabilityRate,
            values.AverageIdaciScore,
            values.PupilsWithSenSupportPercentage,
            values.PupilCount,
            values.PupilsWithEhcPlanPercentage);
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
    string LocalAuthorityName,
    PrimarySimilarSchoolCharacteristics Characteristics);

public record PrimarySimilarSchool(
    string Urn,
    string Name,
    string LocalAuthorityName,
    string Rank,
    string Distance,
    string Street,
    string Locality,
    string Address3,
    string Town,
    string Postcode,
    double? Latitude,
    double? Longitude,
    string SortMetricName,
    DataWithAvailability<string> SortMetricDisplayValue,
    PrimarySimilarSchoolCharacteristics Characteristics);

public record PrimarySimilarSchoolCharacteristics(
    decimal ReadMatAverage,
    decimal Ks1PriorRwmAverage,
    decimal PupilPremiumEligibilityPercentage,
    decimal PupilsWithEalPercentage,
    decimal Polar4Quintile,
    decimal PupilStabilityRate,
    decimal AverageIdaciScore,
    decimal PupilsWithSenSupportPercentage,
    decimal PupilCount,
    decimal PupilsWithEhcPlanPercentage);

internal record PrimaryRankedSimilarSchoolData(
    string Rank,
    string Distance,
    SimilarSchool SimilarSchool,
    SimilarSchoolsPrimaryValues Characteristics,
    Ks2PerformanceData? PerformanceData,
    string SortMetricName = "",
    DataWithAvailability<string>? SortMetricDisplayValue = null);
