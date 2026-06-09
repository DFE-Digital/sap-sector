using SAPSec.Core.Exceptions;
using SAPSec.Core.Measures;
using SAPSec.Core.School.Info;
using SAPSec.Core.UseCases;
using SAPSec.Data.Dto;
using SAPSec.Data.Store;

namespace SAPSec.Core.School.Secondary.Ks4CoreSubjects;

public class GetSchoolComparisonKs4CoreSubjectsUseCase(
    IKs4PerformanceStore performanceStore,
    IEstablishmentStore establishmentStore,
    ISimilarSchoolsSecondaryStore similarSchoolsStore)
    : IUseCase<GetSchoolComparisonKs4CoreSubjectsRequest, GetSchoolComparisonKs4CoreSubjectsResponse>
{
    public async Task<GetSchoolComparisonKs4CoreSubjectsResponse> Execute(GetSchoolComparisonKs4CoreSubjectsRequest request)
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
            null);

        var similarSchoolsUrns = (await similarSchoolsStore.GetSimilarSchoolsGroupAsync(request.CurrentSchoolUrn))
            .Select(g => g.NeighbourURN)
            .Where(urn => !string.IsNullOrWhiteSpace(urn))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var similarSchoolsPerformance = (await performanceStore.GetByUrnsAsync(similarSchoolsUrns) ?? [])
            .ToDictionary(x => x.URN, x => x, StringComparer.Ordinal);

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
                null))
            .ToDictionary(x => x.Urn, x => x, StringComparer.Ordinal);

        var similarSchoolData = similarSchoolsData[request.SimilarSchoolUrn];

        var filterBy = request.FilterBy ?? new Dictionary<string, string>();

        return new(
            new(currentSchool.URN, currentSchool.EstablishmentName),
            new(similarSchool.URN, similarSchool.EstablishmentName),
            similarSchoolsData.Values.Count,
            [
                Ks4CoreSubjects.EnglishLanguage.ForSchoolComparison(currentSchoolData, similarSchoolData, similarSchoolsData, filterBy),
                Ks4CoreSubjects.EnglishLiterature.ForSchoolComparison(currentSchoolData, similarSchoolData, similarSchoolsData, filterBy),
                Ks4CoreSubjects.Biology.ForSchoolComparison(currentSchoolData, similarSchoolData, similarSchoolsData, filterBy),
                Ks4CoreSubjects.Chemistry.ForSchoolComparison(currentSchoolData, similarSchoolData, similarSchoolsData, filterBy),
                Ks4CoreSubjects.Physics.ForSchoolComparison(currentSchoolData, similarSchoolData, similarSchoolsData, filterBy),
                Ks4CoreSubjects.Mathematics.ForSchoolComparison(currentSchoolData, similarSchoolData, similarSchoolsData, filterBy),
                Ks4CoreSubjects.CombinedScience.ForSchoolComparison(currentSchoolData, similarSchoolData, similarSchoolsData, filterBy),
            ]);
    }
}

public record GetSchoolComparisonKs4CoreSubjectsRequest(
    string CurrentSchoolUrn,
    string SimilarSchoolUrn,
    IDictionary<string, string>? FilterBy = null);

public record GetSchoolComparisonKs4CoreSubjectsResponse(
    SchoolInfo CurrentSchool,
    SchoolInfo SimilarSchool,
    int SimilarSchoolsCount,
    IReadOnlyCollection<Measure> Measures);
