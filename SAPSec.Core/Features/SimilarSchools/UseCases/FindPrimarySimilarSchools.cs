using SAPSec.Core.Features.Filtering;
using SAPSec.Core.Features.Geography;
using SAPSec.Core.Features.Pagination;
using SAPSec.Core.Features.SimilarSchools.Filtering;
using SAPSec.Core.UseCases;
using SAPSec.Data.Repositories;

namespace SAPSec.Core.Features.SimilarSchools.UseCases;

public class FindPrimarySimilarSchools(
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsPrimaryRepository similarSchoolsRepository,
    IAbsenceRepository absenceRepository)
    : IUseCase<FindPrimarySimilarSchoolsRequest, FindPrimarySimilarSchoolsResponse>
{
    public async Task<FindPrimarySimilarSchoolsResponse> Execute(FindPrimarySimilarSchoolsRequest request)
    {
        var groups = (await similarSchoolsRepository.GetGroupAsync(request.Urn))
            .Where(group => !string.IsNullOrWhiteSpace(group.NeighbourURN))
            .ToList();

        var similarSchoolUrns = groups
            .Select(group => group.NeighbourURN)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var urns = similarSchoolUrns
            .Concat([request.Urn])
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var establishments = (await establishmentRepository.GetEstablishmentsAsync(urns))
            .ToDictionary(establishment => establishment.URN, StringComparer.Ordinal);

        if (!establishments.TryGetValue(request.Urn, out var currentEstablishment))
        {
            throw new NotFoundException($"School not found with URN: {request.Urn}");
        }

        var characteristics = SimilarSchoolsPrimaryValues.FromData(
                await similarSchoolsRepository.GetValuesByUrnsAsync(urns))
            .ToDictionary(values => values.Urn, StringComparer.Ordinal);

        if (!characteristics.TryGetValue(request.Urn, out var currentCharacteristics))
        {
            throw new NotFoundException($"No similar schools characteristics found for URN {request.Urn}");
        }

        var absences = (await absenceRepository.GetByUrnsAsync(urns))
            .ToDictionary(absence => absence.Urn, StringComparer.Ordinal);

        var currentSimilarSchool = SimilarSchool.FromData(
            currentEstablishment,
            null,
            absences.GetValueOrDefault(request.Urn)?.EstablishmentAbsence);

        var similarSchools = groups
            .Select(group =>
            {
                if (!establishments.TryGetValue(group.NeighbourURN, out var establishment))
                {
                    return null;
                }

                if (!characteristics.TryGetValue(group.NeighbourURN, out var values))
                {
                    return null;
                }

                var similarSchool = SimilarSchool.FromData(
                    establishment,
                    null,
                    absences.GetValueOrDefault(group.NeighbourURN)?.EstablishmentAbsence);

                return new PrimaryRankedSimilarSchoolData(
                    group.Rank,
                    group.Dist,
                    similarSchool,
                    values);
            })
            .Where(school => school is not null)
            .Select(school => school!)
            .ToList();

        var filters = new SimilarSchoolsFilters(
            request.FilterBy ?? new Dictionary<string, IEnumerable<string>>(),
            currentSimilarSchool);

        var validationErrors = filters.Validate();

        var filteredSimilarSchools = filters.Filter(similarSchools.Select(x => x.SimilarSchool))
            .Select(school => similarSchools.First(x => x.SimilarSchool.URN == school.URN))
            .ToList();

        var page = int.TryParse(request.Page, out var parsedPage) ? parsedPage : 1;
        var pagedSimilarSchools = new PagedCollection<PrimaryRankedSimilarSchoolData>(
            filteredSimilarSchools,
            page,
            request.ResultsPerPage);

        return new(
            new PrimaryCurrentSchool(
                currentEstablishment.URN,
                currentEstablishment.EstablishmentName,
                currentEstablishment.LAName,
                ToCharacteristics(currentCharacteristics)),
            pagedSimilarSchools.Map(ToSimilarSchool),
            filteredSimilarSchools.Select(ToSimilarSchool).ToList().AsReadOnly(),
            filters.AsAvailableFilters(similarSchools.Select(x => x.SimilarSchool)),
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
            school.SimilarSchool.Address.Street,
            school.SimilarSchool.Address.Locality,
            school.SimilarSchool.Address.Address3,
            school.SimilarSchool.Address.Town,
            school.SimilarSchool.Address.Postcode,
            coordinates?.Latitude,
            coordinates?.Longitude,
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
    string? Page = null,
    int ResultsPerPage = 10);

public record FindPrimarySimilarSchoolsResponse(
    PrimaryCurrentSchool CurrentSchool,
    IPagedCollection<PrimarySimilarSchool> SimilarSchoolsPage,
    IReadOnlyCollection<PrimarySimilarSchool> AllSimilarSchools,
    IReadOnlyCollection<SimilarSchoolsAvailableFilter> FilterOptions,
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
    SimilarSchoolsPrimaryValues Characteristics);
