using SAPSec.Core.Features.Measures;
using SAPSec.Core.Interfaces.Services;
using SAPSec.Core.Model;
using SAPSec.Data.Dto;
using SAPSec.Data.Store;

namespace SAPSec.Core.Features.Ks4CoreSubjects.UseCases;

public class GetSchoolComparisonKs4CoreSubjects(
    IKs4PerformanceStore performanceStore,
    ISchoolDetailsService schoolDetailsService,
    IEstablishmentStore establishmentStore,
    ISimilarSchoolsSecondaryStore similarSchoolsStore)
{
    public async Task<GetSchoolComparisonKs4CoreSubjectsResponse> Execute(GetSchoolComparisonKs4CoreSubjectsRequest request)
    {
        var currentSchoolDetails = await schoolDetailsService.GetByUrnAsync(request.CurrentSchoolUrn);
        var similarSchoolDetails = await schoolDetailsService.GetByUrnAsync(request.SimilarSchoolUrn);

        var currentSchoolData = new SchoolData(
            request.CurrentSchoolUrn,
            currentSchoolDetails.Name,
            await performanceStore.GetByUrnAsync(request.CurrentSchoolUrn),
            null);

        var similarSchoolsUrns = (await similarSchoolsStore.GetSimilarSchoolsGroupAsync(request.CurrentSchoolUrn))
            .Select(g => g.NeighbourURN)
            .Where(urn => !string.IsNullOrWhiteSpace(urn))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var similarSchoolsPerformance = ((await performanceStore.GetByUrnsAsync(similarSchoolsUrns)) ?? [])
            .ToDictionary(x => x.URN, x => x, StringComparer.Ordinal);

        var similarSchoolsDetails = ((await establishmentStore.GetEstablishmentsAsync(similarSchoolsUrns))
                ?? Array.Empty<Establishment>())
            .Where(x => !string.IsNullOrWhiteSpace(x.URN))
            .ToDictionary(x => x.URN, StringComparer.Ordinal);

        var similarSchoolsData = similarSchoolsUrns
            .Where(similarSchoolsDetails.ContainsKey)
            .Select(urn => new SchoolData(
                urn,
                similarSchoolsDetails[urn].EstablishmentName,
                similarSchoolsPerformance.GetValueOrDefault(urn),
                null))
            .ToDictionary(x => x.Urn, x => x, StringComparer.Ordinal);

        var similarSchoolData = similarSchoolsData[request.SimilarSchoolUrn];

        var filterBy = request.FilterBy ?? new Dictionary<string, string>();

        return new(
            currentSchoolDetails,
            similarSchoolDetails,
            similarSchoolsData.Values.Count,
            [
                Measures.Ks4CoreSubjects.EnglishLanguage.ForSchoolComparison(currentSchoolData, similarSchoolData, similarSchoolsData, filterBy),
                Measures.Ks4CoreSubjects.EnglishLiterature.ForSchoolComparison(currentSchoolData, similarSchoolData, similarSchoolsData, filterBy),
                Measures.Ks4CoreSubjects.Biology.ForSchoolComparison(currentSchoolData, similarSchoolData, similarSchoolsData, filterBy),
                Measures.Ks4CoreSubjects.Chemistry.ForSchoolComparison(currentSchoolData, similarSchoolData, similarSchoolsData, filterBy),
                Measures.Ks4CoreSubjects.Physics.ForSchoolComparison(currentSchoolData, similarSchoolData, similarSchoolsData, filterBy),
                Measures.Ks4CoreSubjects.Mathematics.ForSchoolComparison(currentSchoolData, similarSchoolData, similarSchoolsData, filterBy),
                Measures.Ks4CoreSubjects.CombinedScience.ForSchoolComparison(currentSchoolData, similarSchoolData, similarSchoolsData, filterBy),
            ]);
    }
}

public record GetSchoolComparisonKs4CoreSubjectsRequest(
    string CurrentSchoolUrn,
    string SimilarSchoolUrn,
    IDictionary<string, string>? FilterBy = null);

public record GetSchoolComparisonKs4CoreSubjectsResponse(
    SchoolDetails CurrentSchoolDetails,
    SchoolDetails SimilarSchoolDetails,
    int SimilarSchoolsCount,
    IReadOnlyCollection<Measure> Measures);
