using SAPSec.Core.Features.Measures;
using SAPSec.Core.Features.SimilarSchools;
using SAPSec.Core.Interfaces.Repositories;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Core.Model.Generated;

namespace SAPSec.Core.Features.Ks4HeadlineMeasures.UseCases;

public class GetSchoolComparisonKs4HeadlineMeasures(
    IKs4PerformanceRepository performanceRepository,
    IKs4DestinationsRepository destinationsRepository,
    ISchoolDetailsService schoolDetailsService,
    IEstablishmentRepository establishmentRepository,
    ISimilarSchoolsSecondaryRepository similarSchoolsRepository)
{
    public async Task<GetSchoolComparisonKs4HeadlineMeasuresResponse> Execute(GetSchoolComparisonKs4HeadlineMeasuresRequest request)
    {
        var currentSchoolDetails = await schoolDetailsService.GetByUrnAsync(request.CurrentSchoolUrn);
        var similarSchoolDetails = await schoolDetailsService.GetByUrnAsync(request.SimilarSchoolUrn);

        var currentSchoolData = new SchoolData(
            request.CurrentSchoolUrn,
            currentSchoolDetails.Name,
            await performanceRepository.GetByUrnAsync(request.CurrentSchoolUrn),
            await destinationsRepository.GetByUrnAsync(request.CurrentSchoolUrn));

        var similarSchoolsUrns = (await similarSchoolsRepository.GetSimilarSchoolsGroupAsync(request.CurrentSchoolUrn))
            .Select(g => g.NeighbourURN)
            .Where(urn => !string.IsNullOrWhiteSpace(urn))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var similarSchoolsPerformance = (await performanceRepository.GetByUrnsAsync(similarSchoolsUrns) ?? [])
            .ToDictionary(x => x.URN, x => x, StringComparer.Ordinal);

        var similarSchoolsDestinations = (await destinationsRepository.GetByUrnsAsync(similarSchoolsUrns) ?? [])
            .ToDictionary(x => x.Urn, x => x, StringComparer.Ordinal);

        var similarSchoolsDetails = (await establishmentRepository.GetEstablishmentsAsync(similarSchoolsUrns)
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
            currentSchoolDetails,
            similarSchoolDetails,
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
    SchoolDetails CurrentSchoolDetails,
    SchoolDetails SimilarSchoolDetails,
    int SimilarSchoolsCount,
    Measure Attainment8,
    Measure EnglishAndMaths,
    Measure Destinations);