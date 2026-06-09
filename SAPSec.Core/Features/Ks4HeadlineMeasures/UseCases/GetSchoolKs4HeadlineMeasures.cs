using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Data.Store;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Data.Dto;

namespace SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;

public class GetSchoolKs4HeadlineMeasures(
    IKs4PerformanceStore performanceStore,
    IKs4DestinationsStore destinationsStore,
    ISchoolDetailsService schoolDetailsService,
    IEstablishmentStore establishmentStore,
    ISimilarSchoolsSecondaryStore similarSchoolsStore)
{
    public async Task<GetSchoolKs4HeadlineMeasuresResponse> Execute(GetSchoolKs4HeadlineMeasuresRequest request)
    {
        var schoolDetails = await schoolDetailsService.GetByUrnAsync(request.Urn);

        var schoolData = new SchoolData(
            request.Urn,
            schoolDetails.Name,
            await performanceStore.GetByUrnAsync(request.Urn),
            await destinationsStore.GetByUrnAsync(request.Urn));

        var similarSchoolUrns = (await similarSchoolsStore.GetSimilarSchoolsGroupAsync(request.Urn))
            .Select(s => s.NeighbourURN);
        var similarSchoolPerformanceData = ((await performanceStore.GetByUrnsAsync(similarSchoolUrns)) ?? [])
            .ToDictionary(x => x.URN, x => x, StringComparer.Ordinal);
        var similarSchoolDestinationsData = ((await destinationsStore.GetByUrnsAsync(similarSchoolUrns)) ?? [])
            .ToDictionary(x => x.Urn, x => x, StringComparer.Ordinal);
        var similarSchoolDetails = ((await establishmentStore.GetEstablishmentsAsync(similarSchoolUrns))
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
            schoolDetails,
            similarSchools.Length,
            Measures.Ks4HeadlineMeasures.Attainment8.ForSchool(
                schoolData,
                similarSchools,
                filterBy),
            Measures.Ks4HeadlineMeasures.EnglishAndMaths.ForSchool(
                schoolData,
                similarSchools,
                filterBy),
            Measures.Ks4HeadlineMeasures.Destinations.ForSchool(
                schoolData,
                similarSchools,
                filterBy));
    }
}

public record GetSchoolKs4HeadlineMeasuresRequest(
    string Urn,
    IDictionary<string, string>? FilterBy = null);

public record GetSchoolKs4HeadlineMeasuresResponse(
    SchoolDetails SchoolDetails,
    int SimilarSchoolsCount,
    Measure Attainment8,
    Measure EnglishAndMaths,
    Measure Destinations);