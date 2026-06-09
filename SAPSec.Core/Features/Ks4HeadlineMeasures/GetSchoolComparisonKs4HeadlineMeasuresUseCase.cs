using SAPSec.Core.Features.Measures;
using SAPSec.Data.Dto;
using SAPSec.Data.Store;

namespace SAPSec.Core.Features.Ks4HeadlineMeasures;

public class GetSchoolComparisonKs4HeadlineMeasuresUseCase(
    IKs4PerformanceStore performanceStore,
    IKs4DestinationsStore destinationsStore,
    IEstablishmentStore establishmentStore,
    ISimilarSchoolsSecondaryStore similarSchoolsStore)
    : IUseCase<GetSchoolComparisonKs4HeadlineMeasuresRequest, GetSchoolComparisonKs4HeadlineMeasuresResponse>
{
    public async Task<GetSchoolComparisonKs4HeadlineMeasuresResponse> Execute(GetSchoolComparisonKs4HeadlineMeasuresRequest request)
    {
        var currentSchool = await establishmentStore.GetEstablishmentAsync(request.CurrentSchoolUrn);
        if (currentSchool is null)
        {
            throw new NotFoundException($"School not found with URN: {request.CurrentSchoolUrn}");
        }

        var similarSchool = await establishmentStore.GetEstablishmentAsync(request.SimilarSchoolUrn);
        if (similarSchool is null)
        {
            throw new NotFoundException($"School not found with URN: {request.SimilarSchoolUrn}");
        }

        var currentSchoolData = new SchoolData(
            request.CurrentSchoolUrn,
            currentSchool.EstablishmentName,
            await performanceStore.GetByUrnAsync(request.CurrentSchoolUrn),
            await destinationsStore.GetByUrnAsync(request.CurrentSchoolUrn));

        var similarSchoolsUrns = (await similarSchoolsStore.GetSimilarSchoolsGroupAsync(request.CurrentSchoolUrn))
            .Select(g => g.NeighbourURN)
            .Where(urn => !string.IsNullOrWhiteSpace(urn))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var similarSchoolsPerformance = (await performanceStore.GetByUrnsAsync(similarSchoolsUrns) ?? [])
            .ToDictionary(x => x.URN, x => x, StringComparer.Ordinal);

        var similarSchoolsDestinations = (await destinationsStore.GetByUrnsAsync(similarSchoolsUrns) ?? [])
            .ToDictionary(x => x.Urn, x => x, StringComparer.Ordinal);

        var similarSchoolsDetails = (await establishmentStore.GetEstablishmentsAsync(similarSchoolsUrns)
                ?? Array.Empty<Establishment>())
            .Where(x => !string.IsNullOrWhiteSpace(x.URN))
            .ToDictionary(x => x.URN, StringComparer.Ordinal);

        var similarSchoolsData = similarSchoolsUrns
            .Where(similarSchoolsDetails.ContainsKey)
            .Select(urn => new SchoolData(
                urn,
                similarSchoolsDetails[urn].EstablishmentName,
                similarSchoolsPerformance.GetValueOrDefault(urn),
                similarSchoolsDestinations.GetValueOrDefault(urn)))
            .ToDictionary(x => x.Urn, x => x, StringComparer.Ordinal);

        var similarSchoolData = similarSchoolsData[request.SimilarSchoolUrn];

        var filterBy = request.FilterBy ?? new Dictionary<string, string>();

        return new(
            new(currentSchool.URN, currentSchool.EstablishmentName),
            new(similarSchool.URN, similarSchool.EstablishmentName),
            similarSchoolsData.Values.Count,
            Measures.Ks4HeadlineMeasures.Attainment8.ForSchoolComparison(currentSchoolData, similarSchoolData, similarSchoolsData, filterBy),
            Measures.Ks4HeadlineMeasures.EnglishAndMaths.ForSchoolComparison(currentSchoolData, similarSchoolData, similarSchoolsData, filterBy),
            Measures.Ks4HeadlineMeasures.Destinations.ForSchoolComparison(currentSchoolData, similarSchoolData, similarSchoolsData, filterBy));
    }
}

public record GetSchoolComparisonKs4HeadlineMeasuresRequest(
    string CurrentSchoolUrn,
    string SimilarSchoolUrn,
    IDictionary<string, string>? FilterBy = null);

public record GetSchoolComparisonKs4HeadlineMeasuresResponse(
    SchoolInfo CurrentSchool,
    SchoolInfo SimilarSchool,
    int SimilarSchoolsCount,
    Measure Attainment8,
    Measure EnglishAndMaths,
    Measure Destinations);