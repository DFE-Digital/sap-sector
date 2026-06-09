using SAPSec.Core.Exceptions;
using SAPSec.Core.Measures;
using SAPSec.Core.School.Info;
using SAPSec.Core.UseCases;
using SAPSec.Data.Dto;
using SAPSec.Data.Store;

namespace SAPSec.Core.School.Secondary.Ks4HeadlineMeasures;

public class GetSchoolKs4HeadlineMeasuresUseCase(
    IKs4PerformanceStore performanceStore,
    IKs4DestinationsStore destinationsStore,
    IEstablishmentStore establishmentStore,
    ISimilarSchoolsSecondaryStore similarSchoolsStore)
    : IUseCase<GetSchoolKs4HeadlineMeasuresRequest, GetSchoolKs4HeadlineMeasuresResponse>
{
    public async Task<GetSchoolKs4HeadlineMeasuresResponse> Execute(GetSchoolKs4HeadlineMeasuresRequest request)
    {
        var school = await establishmentStore.GetEstablishmentAsync(request.Urn);
        if (school is null)
        {
            throw new NotFoundException($"School not found with URN: {request.Urn}");
        }

        var schoolData = new SchoolData(
            request.Urn,
            school.EstablishmentName,
            await performanceStore.GetByUrnAsync(request.Urn),
            await destinationsStore.GetByUrnAsync(request.Urn));

        var similarSchoolUrns = (await similarSchoolsStore.GetSimilarSchoolsGroupAsync(request.Urn))
            .Select(s => s.NeighbourURN);
        var similarSchoolPerformanceData = (await performanceStore.GetByUrnsAsync(similarSchoolUrns) ?? [])
            .ToDictionary(x => x.URN, x => x, StringComparer.Ordinal);
        var similarSchoolDestinationsData = (await destinationsStore.GetByUrnsAsync(similarSchoolUrns) ?? [])
            .ToDictionary(x => x.Urn, x => x, StringComparer.Ordinal);
        var similarSchoolDetails = (await establishmentStore.GetEstablishmentsAsync(similarSchoolUrns)
                ?? Array.Empty<Establishment>())
            .Where(x => !string.IsNullOrWhiteSpace(x.URN))
            .ToDictionary(x => x.URN, StringComparer.Ordinal);

        var similarSchools = similarSchoolUrns
            .Where(similarSchoolDetails.ContainsKey)
            .Select(urn => new SchoolData(
                urn,
                similarSchoolDetails[urn].EstablishmentName,
                similarSchoolPerformanceData.GetValueOrDefault(urn),
                similarSchoolDestinationsData.GetValueOrDefault(urn)))
            .ToArray();

        var filterBy = request.FilterBy ?? new Dictionary<string, string>();

        return new(
            new(school.URN, school.EstablishmentName),
            similarSchools.Length,
            Ks4HeadlineMeasures.Attainment8.ForSchool(
                schoolData,
                similarSchools,
                filterBy),
            Ks4HeadlineMeasures.EnglishAndMaths.ForSchool(
                schoolData,
                similarSchools,
                filterBy),
            Ks4HeadlineMeasures.Destinations.ForSchool(
                schoolData,
                similarSchools,
                filterBy));
    }
}

public record GetSchoolKs4HeadlineMeasuresRequest(
    string Urn,
    IDictionary<string, string>? FilterBy = null);

public record GetSchoolKs4HeadlineMeasuresResponse(
    SchoolInfo School,
    int SimilarSchoolsCount,
    Measure Attainment8,
    Measure EnglishAndMaths,
    Measure Destinations);