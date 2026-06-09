using SAPSec.Core.Exceptions;
using SAPSec.Core.Measures;
using SAPSec.Core.SchoolDetails;
using SAPSec.Core.UseCases;
using SAPSec.Data.Dto;
using SAPSec.Data.Store;

namespace SAPSec.Core.Ks4CoreSubjects;

public class GetSchoolKs4CoreSubjectsUseCase(
    IKs4PerformanceStore performanceStore,
    IEstablishmentStore establishmentStore,
    ISimilarSchoolsSecondaryStore similarSchoolsStore)
    : IUseCase<GetSchoolKs4CoreSubjectsRequest, GetSchoolKs4CoreSubjectsResponse>
{
    public async Task<GetSchoolKs4CoreSubjectsResponse> Execute(GetSchoolKs4CoreSubjectsRequest request)
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
            null);

        var similarSchoolUrns = (await similarSchoolsStore.GetSimilarSchoolsGroupAsync(request.Urn))
            .Select(g => g.NeighbourURN)
            .Where(urn => !string.IsNullOrWhiteSpace(urn))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var similarSchoolData = (await performanceStore.GetByUrnsAsync(similarSchoolUrns) ?? [])
            .ToDictionary(x => x.URN, x => x, StringComparer.Ordinal);
        var similarSchoolDetails = (await establishmentStore.GetEstablishmentsAsync(similarSchoolUrns)
                ?? Array.Empty<Establishment>())
            .Where(x => !string.IsNullOrWhiteSpace(x.URN))
            .ToDictionary(x => x.URN, StringComparer.Ordinal);

        var similarSchools = similarSchoolUrns
            .Where(similarSchoolDetails.ContainsKey)
            .Select(urn => new SchoolData(
                urn,
                similarSchoolDetails[urn].EstablishmentName,
                similarSchoolData.GetValueOrDefault(urn),
                null))
            .ToArray();

        var filterBy = request.FilterBy ?? new Dictionary<string, string>();

        return new(
            new(school.URN, school.EstablishmentName),
            similarSchools.Length,
            [
                Measures.Ks4CoreSubjects.EnglishLanguage.ForSchool(schoolData, similarSchools, filterBy),
                Measures.Ks4CoreSubjects.EnglishLiterature.ForSchool(schoolData, similarSchools, filterBy),
                Measures.Ks4CoreSubjects.Biology.ForSchool(schoolData, similarSchools, filterBy),
                Measures.Ks4CoreSubjects.Chemistry.ForSchool(schoolData, similarSchools, filterBy),
                Measures.Ks4CoreSubjects.Physics.ForSchool(schoolData, similarSchools, filterBy),
                Measures.Ks4CoreSubjects.Mathematics.ForSchool(schoolData, similarSchools, filterBy),
                Measures.Ks4CoreSubjects.CombinedScience.ForSchool(schoolData, similarSchools, filterBy),
            ]);
    }
}

public record GetSchoolKs4CoreSubjectsRequest(
    string Urn,
    IDictionary<string, string>? FilterBy = null);

public record GetSchoolKs4CoreSubjectsResponse(
    SchoolInfo School,
    int SimilarSchoolsCount,
    IReadOnlyCollection<Measure> Measures);
